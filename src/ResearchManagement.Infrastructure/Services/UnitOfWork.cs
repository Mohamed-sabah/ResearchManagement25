

using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Domain.Entities;
using ResearchManagement.Infrastructure.Data;
using ResearchManagement.Infrastructure.Repositories;

namespace ResearchManagement.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ResearchRepository> _researchLogger;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repositories
        private IResearchRepository? _research;
        private IGenericRepository<ResearchAuthor>? _researchAuthors;
        private IGenericRepository<ResearchFile>? _researchFiles;
        private IReviewRepository? _reviews;
        private IResearchStatusHistoryRepository? _statusHistory;
        private IUserRepository? _users;

        public UnitOfWork(ApplicationDbContext context, ILogger<ResearchRepository> researchLogger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _researchLogger = researchLogger ?? throw new ArgumentNullException(nameof(researchLogger));
        }

        public IResearchRepository Research => _research ??= new ResearchRepository(_context, _researchLogger);

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
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("خطأ في حفظ البيانات", ex);
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("No transaction started");
                }

                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
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
                    await _transaction.RollbackAsync();
                }
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