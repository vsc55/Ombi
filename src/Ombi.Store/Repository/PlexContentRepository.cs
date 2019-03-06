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
using Microsoft.EntityFrameworkCore;
using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class PlexServerContentRepository : ExternalRepository<PlexServerContent>, IPlexContentRepository
    {

        public PlexServerContentRepository(string db) : base(db)
        {
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
            return await Db.PlexServerContent.Include(x => x.Seasons).FirstOrDefaultAsync(x => x.Key == key);
        }

        public IEnumerable<PlexServerContent> GetWhereContentByCustom(Expression<Func<PlexServerContent, bool>> predicate)
        {
            return Db.PlexServerContent.Where(predicate);
        }

        public async Task<PlexServerContent> GetFirstContentByCustom(Expression<Func<PlexServerContent, bool>>  predicate)
        {
            return await Db.PlexServerContent
                .Include(x => x.Seasons)
                .Include(x => x.Episodes)
                .FirstOrDefaultAsync(predicate);
        }

        public async Task Update(PlexServerContent existingContent)
        {
            Db.PlexServerContent.Update(existingContent);
            await Db.SaveChangesAsync();
        }
        public void UpdateWithoutSave(PlexServerContent existingContent)
        {
            Db.PlexServerContent.Update(existingContent);
        }

        public async Task UpdateRange(IEnumerable<PlexServerContent> existingContent)
        {
            Db.PlexServerContent.UpdateRange(existingContent);
            await Db.SaveChangesAsync();
        }

        public IQueryable<PlexEpisode> GetAllEpisodes()
        {
            return Db.PlexEpisode.Include(x => x.Series).AsQueryable();
        }

        public void DeleteWithoutSave(PlexServerContent content)
        {
            Db.PlexServerContent.Remove(content);
        }

        public void DeleteWithoutSave(PlexEpisode content)
        {
            Db.PlexEpisode.Remove(content);
        }

        public async Task<PlexEpisode> Add(PlexEpisode content)
        {
            await Db.PlexEpisode.AddAsync(content);
            await Db.SaveChangesAsync();
            return content;
        }

        public async Task DeleteEpisode(PlexEpisode content)
        {
            Db.PlexEpisode.Remove(content);
            await Db.SaveChangesAsync();
        }

        public async Task<PlexEpisode> GetEpisodeByKey(int key)
        {
            return await Db.PlexEpisode.FirstOrDefaultAsync(x => x.Key == key);
        }
        public async Task AddRange(IEnumerable<PlexEpisode> content)
        {
            Db.PlexEpisode.AddRange(content);
            await Db.SaveChangesAsync();
        }
    }
}