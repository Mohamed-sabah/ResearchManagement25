using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ResearchManagement.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repositories
        private IResearchRepository? _research;
        private IGenericRepository<ResearchAuthor>? _researchAuthors;
        private IGenericRepository<ResearchFile>? _researchFiles;
        private IReviewRepository? _reviews;
        private IResearchStatusHistoryRepository? _statusHistory;
        private IUserRepository? _users;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IResearchRepository Research
        {
            get
            {
                if (_research == null)
                {
                    // الحصول على logger من DI container
                    var researchLogger = _serviceProvider.GetRequiredService<ILogger<ResearchRepository>>();
                    _research = new ResearchRepository(_context, researchLogger);
                }
                return _research;
            }
        }

        public IGenericRepository<ResearchAuthor> ResearchAuthors =>
            _researchAuthors ??= new GenericRepository<ResearchAuthor>(_context);

        public IGenericRepository<ResearchFile> ResearchFiles =>
            _researchFiles ??= new GenericRepository<ResearchFile>(_context);

        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

        public IResearchStatusHistoryRepository StatusHistory =>
            _statusHistory ??= new ResearchStatusHistoryRepository(_context);

        public IUserRepository Users => _users ??= new UserRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("بدء حفظ التغييرات في قاعدة البيانات");

                // طباعة عدد التغييرات المعلقة
                var pendingChanges = _context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added ||
                               e.State == EntityState.Modified ||
                               e.State == EntityState.Deleted)
                    .ToList();

                _logger.LogInformation("عدد التغييرات المعلقة: {Count}", pendingChanges.Count);

                foreach (var entry in pendingChanges)
                {
                    _logger.LogInformation("تغيير معلق: {EntityType} - {State}",
                        entry.Entity.GetType().Name, entry.State);

                    // طباعة القيم المتغيرة
                    if (entry.State == EntityState.Modified)
                    {
                        foreach (var prop in entry.Properties.Where(p => p.IsModified))
                        {
                            _logger.LogInformation("تغيير في الخاصية: {PropertyName} من {OriginalValue} إلى {CurrentValue}",
                                prop.Metadata.Name,
                                prop.OriginalValue ?? "null",
                                prop.CurrentValue ?? "null");
                        }
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            _logger.LogInformation("خاصية جديدة: {PropertyName} = {CurrentValue}",
                                prop.Metadata.Name,
                                prop.CurrentValue ?? "null");
                        }
                    }
                }

                var result = await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("تم حفظ التغييرات بنجاح. عدد السجلات المتأثرة: {Count}", result);

                return result;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "خطأ في تحديث قاعدة البيانات");

                // طباعة تفاصيل الخطأ الرئيسي
                _logger.LogError("رسالة الخطأ الرئيسية: {Message}", dbEx.Message);

                // البحث عن الاستثناء الداخلي الأعمق
                var innerException = dbEx.InnerException;
                var level = 1;

                while (innerException != null)
                {
                    _logger.LogError("الاستثناء الداخلي المستوى {Level}: {Message}", level, innerException.Message);

                    // إذا كان SQL Server Exception
                    if (innerException.GetType().Name.Contains("SqlException"))
                    {
                        _logger.LogError("تفاصيل خطأ SQL Server: {SqlError}", innerException.ToString());
                    }

                    // فحص أنواع الأخطاء الشائعة
                    if (innerException.Message.Contains("FOREIGN KEY constraint"))
                    {
                        _logger.LogError("خطأ في قيد المفتاح الخارجي - تحقق من العلاقات بين الجداول");
                        ExtractForeignKeyError(innerException.Message);
                    }
                    else if (innerException.Message.Contains("PRIMARY KEY constraint"))
                    {
                        _logger.LogError("خطأ في قيد المفتاح الأساسي - محاولة إدراج قيمة مكررة");
                    }
                    else if (innerException.Message.Contains("UNIQUE constraint"))
                    {
                        _logger.LogError("خطأ في قيد الفرادة - محاولة إدراج قيمة مكررة في حقل فريد");
                    }
                    else if (innerException.Message.Contains("Cannot insert the value NULL"))
                    {
                        _logger.LogError("خطأ في قيمة NULL - محاولة إدراج قيمة فارغة في حقل مطلوب");
                    }

                    innerException = innerException.InnerException;
                    level++;
                }

                // طباعة معلومات الكيانات التي فشلت
                foreach (var entry in dbEx.Entries)
                {
                    _logger.LogError("فشل تحديث الكيان: {EntityType} - {State}",
                        entry.Entity.GetType().Name, entry.State);

                    // طباعة الخصائص المتغيرة
                    if (entry.State == EntityState.Modified)
                    {
                        foreach (var prop in entry.Properties.Where(p => p.IsModified))
                        {
                            _logger.LogError("خاصية متغيرة: {PropertyName} = {CurrentValue} (الأصلية: {OriginalValue})",
                                prop.Metadata.Name,
                                prop.CurrentValue ?? "NULL",
                                prop.OriginalValue ?? "NULL");
                        }
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        foreach (var prop in entry.Properties)
                        {
                            _logger.LogError("خاصية جديدة: {PropertyName} = {CurrentValue}",
                                prop.Metadata.Name,
                                prop.CurrentValue ?? "NULL");
                        }
                    }

                    // محاولة تسلسل الكيان (مع معالجة الدورات المرجعية)
                    try
                    {
                        var entityJson = System.Text.Json.JsonSerializer.Serialize(entry.Entity, new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true,
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                        });
                        _logger.LogError("قيم الكيان: {EntityValues}", entityJson);
                    }
                    catch (Exception jsonEx)
                    {
                        _logger.LogError("فشل في تسلسل الكيان: {JsonError}", jsonEx.Message);
                    }
                }

                throw new InvalidOperationException($"خطأ في تحديث قاعدة البيانات: {dbEx.Message} - Inner: {dbEx.InnerException?.Message}", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ عام في حفظ البيانات: {Message}", ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("الاستثناء الداخلي: {InnerException}", ex.InnerException.Message);
                }

                throw new InvalidOperationException($"خطأ في حفظ البيانات: {ex.Message}", ex);
            }
        }

        private void ExtractForeignKeyError(string errorMessage)
        {
            try
            {
                // استخراج اسم الجدول والعمود من رسالة الخطأ
                if (errorMessage.Contains("table \"") && errorMessage.Contains("column '"))
                {
                    var tableStart = errorMessage.IndexOf("table \"") + 7;
                    var tableEnd = errorMessage.IndexOf("\"", tableStart);
                    var tableName = errorMessage.Substring(tableStart, tableEnd - tableStart);

                    var columnStart = errorMessage.IndexOf("column '") + 8;
                    var columnEnd = errorMessage.IndexOf("'", columnStart);
                    var columnName = errorMessage.Substring(columnStart, columnEnd - columnStart);

                    _logger.LogError("تفاصيل خطأ المفتاح الخارجي - الجدول: {TableName}, العمود: {ColumnName}",
                        tableName, columnName);
                }

                // استخراج القيمة المُدرجة
                if (errorMessage.Contains("The INSERT statement conflicted"))
                {
                    _logger.LogError("فشل في إدراج سجل جديد بسبب انتهاك قيد المفتاح الخارجي");
                }
                else if (errorMessage.Contains("The UPDATE statement conflicted"))
                {
                    _logger.LogError("فشل في تحديث سجل بسبب انتهاك قيد المفتاح الخارجي");
                }
                else if (errorMessage.Contains("The DELETE statement conflicted"))
                {
                    _logger.LogError("فشل في حذف سجل بسبب انتهاك قيد المفتاح الخارجي");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل في تحليل رسالة خطأ المفتاح الخارجي");
            }
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    throw new InvalidOperationException("Transaction already started");
                }

                _logger.LogInformation("بدء معاملة جديدة");
                _transaction = await _context.Database.BeginTransactionAsync();
                _logger.LogInformation("تم بدء المعاملة بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في بدء المعاملة");
                throw;
            }
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction started");
                }

                _logger.LogInformation("بدء تأكيد المعاملة");
                await SaveChangesAsync();
                await _transaction.CommitAsync();
                _logger.LogInformation("تم تأكيد المعاملة بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تأكيد المعاملة");
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    _logger.LogWarning("بدء إلغاء المعاملة");
                    await _transaction.RollbackAsync();
                    _logger.LogWarning("تم إلغاء المعاملة بنجاح");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إلغاء المعاملة");
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
                _logger.LogInformation("تم تحرير المعاملة");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.Logging;
//using ResearchManagement.Application.Interfaces;
//using ResearchManagement.Domain.Entities;
//using ResearchManagement.Infrastructure.Data;
//using ResearchManagement.Infrastructure.Repositories;
//using Microsoft.EntityFrameworkCore;

//namespace ResearchManagement.Infrastructure.Services
//{
//    public class UnitOfWork : IUnitOfWork, IDisposable
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<UnitOfWork> _logger;
//        private readonly ILoggerFactory _loggerFactory; // إضافة ILoggerFactory
//        private IDbContextTransaction? _transaction;
//        private bool _disposed = false;

//        // Repositories
//        private IResearchRepository? _research;
//        private IGenericRepository<ResearchAuthor>? _researchAuthors;
//        private IGenericRepository<ResearchFile>? _researchFiles;
//        private IReviewRepository? _reviews;
//        private IResearchStatusHistoryRepository? _statusHistory;
//        private IUserRepository? _users;

//        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
//        }

//        public IResearchRepository Research => _research ??= new ResearchRepository(_context, _loggerFactory.CreateLogger<ResearchRepository>());

//        public IGenericRepository<ResearchAuthor> ResearchAuthors =>
//            _researchAuthors ??= new GenericRepository<ResearchAuthor>(_context);

//        public IGenericRepository<ResearchFile> ResearchFiles =>
//            _researchFiles ??= new GenericRepository<ResearchFile>(_context);

//        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

//        public IResearchStatusHistoryRepository StatusHistory =>
//            _statusHistory ??= new ResearchStatusHistoryRepository(_context);

//        public IUserRepository Users => _users ??= new UserRepository(_context);

//        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                _logger.LogInformation("بدء حفظ التغييرات في قاعدة البيانات");

//                // طباعة عدد التغييرات المعلقة
//                var pendingChanges = _context.ChangeTracker.Entries()
//                    .Where(e => e.State == EntityState.Added ||
//                               e.State == EntityState.Modified ||
//                               e.State == EntityState.Deleted)
//                    .ToList();

//                _logger.LogInformation("عدد التغييرات المعلقة: {Count}", pendingChanges.Count);

//                foreach (var entry in pendingChanges)
//                {
//                    _logger.LogInformation("تغيير معلق: {EntityType} - {State}",
//                        entry.Entity.GetType().Name, entry.State);
//                }

//                var result = await _context.SaveChangesAsync(cancellationToken);

//                _logger.LogInformation("تم حفظ التغييرات بنجاح. عدد السجلات المتأثرة: {Count}", result);

//                return result;
//            }
//            catch (DbUpdateException dbEx)
//            {
//                _logger.LogError(dbEx, "خطأ في تحديث قاعدة البيانات");

//                // طباعة تفاصيل الخطأ
//                if (dbEx.InnerException != null)
//                {
//                    _logger.LogError("التفاصيل الداخلية: {InnerException}", dbEx.InnerException.Message);
//                }

//                // طباعة معلومات الكيانات التي فشلت
//                foreach (var entry in dbEx.Entries)
//                {
//                    _logger.LogError("فشل تحديث الكيان: {EntityType} - {State}",
//                        entry.Entity.GetType().Name, entry.State);
//                }

//                throw new InvalidOperationException($"خطأ في تحديث قاعدة البيانات: {dbEx.Message}", dbEx);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ عام في حفظ البيانات: {Message}", ex.Message);

//                if (ex.InnerException != null)
//                {
//                    _logger.LogError("الاستثناء الداخلي: {InnerException}", ex.InnerException.Message);
//                }

//                throw new InvalidOperationException($"خطأ في حفظ البيانات: {ex.Message}", ex);
//            }
//        }

//        public async Task BeginTransactionAsync()
//        {
//            try
//            {
//                if (_transaction != null)
//                {
//                    throw new InvalidOperationException("Transaction already started");
//                }

//                _logger.LogInformation("بدء معاملة جديدة");
//                _transaction = await _context.Database.BeginTransactionAsync();
//                _logger.LogInformation("تم بدء المعاملة بنجاح");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في بدء المعاملة");
//                throw;
//            }
//        }

//        public async Task CommitTransactionAsync()
//        {
//            try
//            {
//                if (_transaction == null)
//                {
//                    throw new InvalidOperationException("No transaction started");
//                }

//                _logger.LogInformation("بدء تأكيد المعاملة");
//                await SaveChangesAsync();
//                await _transaction.CommitAsync();
//                _logger.LogInformation("تم تأكيد المعاملة بنجاح");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في تأكيد المعاملة");
//                await RollbackTransactionAsync();
//                throw;
//            }
//            finally
//            {
//                await DisposeTransactionAsync();
//            }
//        }

//        public async Task RollbackTransactionAsync()
//        {
//            try
//            {
//                if (_transaction != null)
//                {
//                    _logger.LogWarning("بدء إلغاء المعاملة");
//                    await _transaction.RollbackAsync();
//                    _logger.LogWarning("تم إلغاء المعاملة بنجاح");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في إلغاء المعاملة");
//            }
//            finally
//            {
//                await DisposeTransactionAsync();
//            }
//        }

//        private async Task DisposeTransactionAsync()
//        {
//            if (_transaction != null)
//            {
//                await _transaction.DisposeAsync();
//                _transaction = null;
//                _logger.LogInformation("تم تحرير المعاملة");
//            }
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!_disposed)
//            {
//                if (disposing)
//                {
//                    _transaction?.Dispose();
//                    _context.Dispose();
//                }
//                _disposed = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}

//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.Extensions.Logging;
//using ResearchManagement.Application.Interfaces;
//using ResearchManagement.Domain.Entities;
//using ResearchManagement.Infrastructure.Data;
//using ResearchManagement.Infrastructure.Repositories;

//namespace ResearchManagement.Infrastructure.Services
//{
//    public class UnitOfWork : IUnitOfWork, IDisposable
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<ResearchRepository> _researchLogger;
//        private IDbContextTransaction? _transaction;
//        private bool _disposed = false;

//        // Repositories
//        private IResearchRepository? _research;
//        private IGenericRepository<ResearchAuthor>? _researchAuthors;
//        private IGenericRepository<ResearchFile>? _researchFiles;
//        private IReviewRepository? _reviews;
//        private IResearchStatusHistoryRepository? _statusHistory;
//        private IUserRepository? _users;

//        public UnitOfWork(ApplicationDbContext context, ILogger<ResearchRepository> researchLogger)
//        {
//            _context = context ?? throw new ArgumentNullException(nameof(context));
//            _researchLogger = researchLogger ?? throw new ArgumentNullException(nameof(researchLogger));
//        }

//        public IResearchRepository Research => _research ??= new ResearchRepository(_context, _researchLogger);

//        public IGenericRepository<ResearchAuthor> ResearchAuthors =>
//            _researchAuthors ??= new GenericRepository<ResearchAuthor>(_context);

//        public IGenericRepository<ResearchFile> ResearchFiles =>
//            _researchFiles ??= new GenericRepository<ResearchFile>(_context);

//        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

//        public IResearchStatusHistoryRepository StatusHistory =>
//            _statusHistory ??= new ResearchStatusHistoryRepository(_context);

//        public IUserRepository Users => _users ??= new UserRepository(_context);

//        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                return await _context.SaveChangesAsync(cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                throw new InvalidOperationException("خطأ في حفظ البيانات", ex);
//            }
//        }

//        public async Task BeginTransactionAsync()
//        {
//            if (_transaction != null)
//            {
//                throw new InvalidOperationException("Transaction already started");
//            }

//            _transaction = await _context.Database.BeginTransactionAsync();
//        }

//        public async Task CommitTransactionAsync()
//        {
//            try
//            {
//                if (_transaction == null)
//                {
//                    throw new InvalidOperationException("No transaction started");
//                }

//                await SaveChangesAsync();
//                await _transaction.CommitAsync();
//            }
//            catch
//            {
//                await RollbackTransactionAsync();
//                throw;
//            }
//            finally
//            {
//                await DisposeTransactionAsync();
//            }
//        }

//        public async Task RollbackTransactionAsync()
//        {
//            try
//            {
//                if (_transaction != null)
//                {
//                    await _transaction.RollbackAsync();
//                }
//            }
//            finally
//            {
//                await DisposeTransactionAsync();
//            }
//        }

//        private async Task DisposeTransactionAsync()
//        {
//            if (_transaction != null)
//            {
//                await _transaction.DisposeAsync();
//                _transaction = null;
//            }
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!_disposed)
//            {
//                if (disposing)
//                {
//                    _transaction?.Dispose();
//                    _context.Dispose();
//                }
//                _disposed = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}