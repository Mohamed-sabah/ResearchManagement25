using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ResearchManagement.Application.Interfaces;
using ResearchManagement.Infrastructure.Data;

namespace ResearchManagement.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في استرجاع العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في استرجاع العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في استرجاع جميع العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في البحث: {ex.Message}", ex);
            }
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(predicate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في البحث عن العنصر الأول: {ex.Message}", ex);
            }
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var entityEntry = await _dbSet.AddAsync(entity);
                return entityEntry.Entity;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في إضافة العنصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                var entitiesList = entities.ToList();
                await _dbSet.AddRangeAsync(entitiesList);
                return entitiesList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في إضافة العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Update(entity);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في تحديث العنصر: {ex.Message}", ex);
            }
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                _dbSet.UpdateRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في تحديث العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task DeleteAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Remove(entity);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في حذف العنصر: {ex.Message}", ex);
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    await DeleteAsync(entity);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في حذف العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual async Task DeleteAsync(string id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    await DeleteAsync(entity);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في حذف العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null)
                    throw new ArgumentNullException(nameof(entities));

                _dbSet.RemoveRange(entities);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في حذف العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في عد العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.CountAsync(predicate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في عد العناصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في فحص وجود العنصر: {ex.Message}", ex);
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في فحص وجود العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual async Task<bool> ExistsAsync(string id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                return entity != null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في فحص وجود العنصر بالمعرف {id}: {ex.Message}", ex);
            }
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                return await _dbSet
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في استرجاع البيانات بالصفحات: {ex.Message}", ex);
            }
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;

                var query = _dbSet.AsQueryable();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"خطأ في استرجاع البيانات بالصفحات: {ex.Message}", ex);
            }
        }
    }
}