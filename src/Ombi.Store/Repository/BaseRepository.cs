using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Nito.AsyncEx;
using Ombi.Store.Context;
using Dapper;
using Ombi.Store.Entities;
using Dapper.Contrib.Extensions;

namespace Ombi.Store.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : Entity
    {
        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        private const string CorruptMessage =
          "The database is corrupt, this could be due to the application exiting unexpectedly. See here to fix it: http://www.dosomethinghere.com/2013/02/20/fixing-the-sqlite-error-the-database-disk-image-is-malformed/";

        private readonly string _connectionString;
        protected IDbConnection Connection => GetConnection();

        public DbSet<T> _db => throw new NotImplementedException();

        private readonly AsyncLock _mutex = new AsyncLock();

        public async Task<T> Find(object key)
        {
            using (var c = Connection)
            {
                c.Open();
                return await c.GetAsync<T>(key);
            }
        }
        public async Task<IEnumerable<T>> CustomAsync(Func<IDbConnection, Task<IEnumerable<T>>> func)
        {
            using (var cnn = Connection)
            {
                return await func(cnn);
            }
        }
        public async Task<T> CustomAsync(Func<IDbConnection, Task<T>> func)
        {
            using (var cnn = Connection)
            {
                return await func(cnn);
            }
        }

        public async Task<bool> AnyAsync(Func<IDbConnection, Task<bool>> func)
        {
            using (var cnn = Connection)
            {
                return await func(cnn);
            }
        }


        public async Task<IEnumerable<T>> GetAll()
        {
            using (var c = Connection)
            {
                c.Open();
                return await c.GetAllAsync<T>();
            }
        }


        public bool AddRange(IEnumerable<T> content)
        {
            return AddRange<T>(content);
        }

        public bool AddRange<T>(IEnumerable<T> content) where T : Entity
        {
            // If we have nothing to update, then it didn't fail...
            var enumerable = content as T[] ?? content.ToArray();
            if (!enumerable.Any())
            {
                return true;
            }

            using (var db = Connection)
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    var result = enumerable.Sum(e => db.Insert(e));
                    if (result != 0)
                    {
                        tran.Commit();
                        return true;
                    }
                    tran.Rollback();
                    return false;
                }
            }
        }

        public bool UpdateRange(IEnumerable<T> content)
        {
            return UpdateRange<T>(content);
        }
        public bool UpdateRange<T>(IEnumerable<T> content) where T : Entity
        {
            // If we have nothing to update, then it didn't fail...
            var enumerable = content as T[] ?? content.ToArray();
            if (!enumerable.Any())
            {
                return true;
            }

            using (var db = Connection)
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    var result = enumerable.All(e => db.Update(e));
                    if (result)
                    {
                        tran.Commit();
                        return true;
                    }
                    tran.Rollback();
                    return false;
                }
            }
        }

        public async Task<T> Add(T content)
        {
            using (var c = Connection)
            {
                c.Open();
                var id = await c.InsertAsync(content);

                content.Id = id;
                return content;
            }
        }

        public async Task Delete(T request)
        {
            using(var c = Connection)
            {
                c.Open();
                await c.DeleteAsync(request);
            }
        }

        public void DeleteRange(IEnumerable<T> content)
        {
            DeleteRange<T>(content);
        }

        protected void DeleteRange<T>(IEnumerable<T> content) where T : Entity
        {
            // If we have nothing to update, then it didn't fail...
            var enumerable = content as T[] ?? content.ToArray();
            if (!enumerable.Any())
            {
                return;
            }

            using (var db = Connection)
            {
                db.Open();
                using (var tran = db.BeginTransaction())
                {
                    var result = enumerable.All(e => db.Delete(e));
                    if (result)
                    {
                        tran.Commit();
                    }
                    else
                    {
                        tran.Rollback();
                    }
                }
            }
        }

        public async Task ExecuteSql(string sql)
        {
            using(var c = Connection)
            {
                c.Open();
                await c.QueryAsync(sql);
            }
        }

        private bool _disposed;
        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Connection?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IDbConnection GetConnection()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = _connectionString;
            var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            return Connection;
        }

    }
}