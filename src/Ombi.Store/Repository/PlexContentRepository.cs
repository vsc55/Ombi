#region Copyright
// /************************************************************************
//    Copyright (c) 2017 Jamie Rees
//    File: PlexServerContentRepository.cs
//    Created By: Jamie Rees
//   
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//   
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//   
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  ************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Ombi.Store.Context;
using Ombi.Store.Entities;
using Ombi.Store.SQL;

namespace Ombi.Store.Repository
{
    public class PlexServerContentRepository : ExternalRepository<PlexServerContent>, IPlexContentRepository
    {

        private readonly ISqliteQueryService _queryService;

        public PlexServerContentRepository(string db, ISqliteQueryService q) : base(db)
        {
            _queryService = q;
        }



        public async Task<bool> ContentExists(string providerId)
        {
            var any = await AnyAsync(async (con) =>
            {
                con.Open();
                var result = await con.QueryFirstOrDefaultAsync<PlexServerContent>("select * from PlexServerContent where ImdbId = @providerId", new { providerId });
                return result != null;
            });
            if (!any)
            {
                any = await AnyAsync(async (con) =>
                {
                    con.Open();
                    var result = await con.QueryFirstOrDefaultAsync<PlexServerContent>("select * from PlexServerContent where TheMovieDbId = @providerId", new { providerId });
                    return result != null;
                });
                if (!any)
                {
                    any = await AnyAsync(async (con) =>
                    {
                        con.Open();
                        var result = await con.QueryFirstOrDefaultAsync<PlexServerContent>("select * from PlexServerContent where TvDbId = @providerId", new { providerId });
                        return result != null;
                    });
                }
            }
            return any;
        }

        public async Task<PlexServerContent> Get(string providerId)
        {
            var item = await CustomAsync(async (con) =>
            {
                con.Open();
                return await con.QueryFirstOrDefaultAsync<PlexServerContent>("Select * from PlexServerContent where ImdbId = @providerId", new { providerId });
            });
            if (item == null)
            {
                item = item = await CustomAsync(async (con) =>
                {
                    con.Open();
                    return await con.QueryFirstOrDefaultAsync<PlexServerContent>("Select * from PlexServerContent where TheMovieDbId = @providerId", new { providerId });
                });
                if (item == null)
                {
                    item = await CustomAsync(async (con) =>
                    {
                        con.Open();
                        return await con.QueryFirstOrDefaultAsync<PlexServerContent>("Select * from PlexServerContent where TvDbId = @providerId", new { providerId });
                    });
                }
            }
            return item;
        }

        public async Task<PlexServerContent> GetByKey(int key)
        {
            var query = _queryService.GetQuery("Ombi.Store.Sql.Plex.GetPlexServerContentIncludeSeasons.sql");
            return await CustomAsync(async (con) =>
            {
                con.Open();
                return await con.QueryFirstOrDefaultAsync<PlexServerContent>(query, new { key });
            });
        }


        public async Task<PlexServerContent> GetFirstContentByCustom(Func<PlexServerContent, bool> predicate)
        {
            var query = _queryService.GetQuery("Ombi.Store.Sql.Plex.GetPlexServerContentIncludeEverything.sql");
            using (var con = Connection)
            {
                con.Open();
                var result = await con.QueryAsync<PlexServerContent>(query);

                return result.FirstOrDefault(predicate);
            }
        }

        public async Task Update(PlexServerContent existingContent)
        {
            using (var con = Connection)
            {
                con.Open();
                await con.UpdateAsync(existingContent);
            }
        }

        public async Task<IEnumerable<PlexEpisode>> GetAllEpisodes()
        {
            var query = _queryService.GetQuery("Ombi.Store.Sql.Plex.GetAllPlexEpisodesIncludingSeries.sql");
            using (var con = Connection)
            {

                con.Open();
                return await con.QueryAsync<PlexEpisode>(query);
            }
        }


        public async Task<PlexEpisode> Add(PlexEpisode content)
        {
            using (var c = Connection)
            {
                c.Open();
                var id = await c.InsertAsync(content);

                content.Id = id;
                return content;
            }
        }

        public async Task DeleteEpisode(PlexEpisode content)
        {
            using (var c = Connection)
            {
                c.Open();
                await c.DeleteAsync(content);
            }
        }

        public async Task<PlexEpisode> GetEpisodeByKey(int key)
        {
            using (var c = Connection)
            {
                c.Open();
                return await c.GetAsync<PlexEpisode>(key);
            }
        }
        public void AddRange(IEnumerable<PlexEpisode> content)
        {
            var enumerable = content as PlexEpisode[] ?? content.ToArray();
            if (!enumerable.Any())
            {
                return;
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
                        return; ;
                    }
                    tran.Rollback();
                    return;
                }
            }
        }

        void IPlexContentRepository.UpdateRange(IEnumerable<PlexServerContent> existingContent)
        {
            base.UpdateRange<PlexServerContent>(existingContent);
        }

        async async Task<IEnumerable<PlexServerContent>> IExternalRepository<PlexServerContent>.GetAll()
        {
            return await base.GetAll();
        }

        public Task AddRange(IEnumerable<PlexServerContent> content, bool save = true)
        {
            base.AddRange<PlexServerContent>(content);
            return Task.CompletedTask;
        }

        Task IExternalRepository<PlexServerContent>.DeleteRange(IEnumerable<PlexServerContent> req)
        {
            base.DeleteRange<PlexServerContent>(req);
            return Task.CompletedTask;
        }
    }
}