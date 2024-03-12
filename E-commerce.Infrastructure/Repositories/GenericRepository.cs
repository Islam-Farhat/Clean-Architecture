using E_commerce.Domian;
using E_commerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly EcommerceReadContext _contextRead;
        private readonly EcommerceWriteContext _contextWrite;

        public GenericRepository(EcommerceReadContext contextRead,EcommerceWriteContext contextWrite)
        {
            this._contextRead = contextRead;
            this._contextWrite = contextWrite;
        }

        public async Task Add(T entity)
        {
            await _contextWrite.AddAsync(entity);
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }

        public int Delete(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            IQueryable<T> query = _contextRead.Set<T>();

            if (includes != null)
                foreach (var include in includes)
                    query = query.Include(include);

            return await query.Where(criteria).ToListAsync();
        }

        public Task<object> FindAllAsync(Expression<Func<T, object>> Selector, Expression<Func<T, bool>> criteria, string[] includes = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int take, int skip)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? take, int? skip, Expression<Func<T, object>> orderBy = null, string orderByDirection = "Ascending")
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Save()
        {
            return await _contextWrite.SaveChangesAsync() > 0;
        }

        public Task<object> SqlRaw(string Query)
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public void Update<TSource>(TSource entityDTO) where TSource : class
        {
            throw new NotImplementedException();
        }
        public async Task<T?> GetObj(Expression<Func<T, bool>> filter) => await _contextRead.Set<T>().AsQueryable<T>().FirstOrDefaultAsync(filter);
    }
}
