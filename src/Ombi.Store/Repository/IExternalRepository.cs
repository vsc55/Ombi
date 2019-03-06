using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IExternalRepository<T> : IDisposable where T : Entity
    {
        Task<T> Find(object key);
        IQueryable<T> GetAll();
        Task AddRange(IEnumerable<T> content, bool save = true);
        Task<T> Add(T content);
        Task DeleteRange(IEnumerable<T> req);
        Task Delete(T request);
        Task<int> SaveChangesAsync();

        Task ExecuteSql(string sql);
        DbSet<T> _db { get; }
    }
}