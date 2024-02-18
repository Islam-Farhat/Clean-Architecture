using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Domian
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetById(int id);
        Task Add(T entity);
        void Update(T entity);
        void Update<TSource>(TSource entityDTO) where TSource : class;
        void Delete(T entity);
        int Delete(int Id);
        Task<bool> Save();
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, string[] includes = null);
        Task<object> FindAllAsync(Expression<Func<T, object>> Selector, Expression<Func<T, bool>> criteria, string[] includes = null);        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int take, int skip);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> criteria, int? take, int? skip,
            Expression<Func<T, object>> orderBy = null, string orderByDirection = "Ascending"); 
        Task<object> SqlRaw(string Query);
        public Task<T?> GetObj(Expression<Func<T, bool>> filter);
    }
}
