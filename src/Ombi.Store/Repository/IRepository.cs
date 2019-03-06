using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IRepository<T> : IDisposable where T : Entity
    {
        Task<T> Find(object key);
        Task<IEnumerable<T>> GetAll();
        bool AddRange(IEnumerable<T> content);
        Task<T> Add(T content);
        void DeleteRange(IEnumerable<T> req);
        Task Delete(T request);
        Task<IEnumerable<T>> CustomAsync(Func<IDbConnection, Task<IEnumerable<T>>> func);
        Task<IEnumerable<T>> CustomAsync(Func<IDbConnection, Task<T>> func);
        Task ExecuteSql(string sql);
    }
}