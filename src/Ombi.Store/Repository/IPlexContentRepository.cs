using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public interface IPlexContentRepository : IExternalRepository<PlexServerContent>
    {
        Task<bool> ContentExists(string providerId);
        Task<PlexServerContent> Get(string providerId);
        Task<PlexServerContent> GetByKey(int key);
        Task Update(PlexServerContent existingContent);
        Task<IEnumerable<PlexEpisode>> GetAllEpisodes();
        Task<PlexEpisode> Add(PlexEpisode content);
        Task<PlexEpisode> GetEpisodeByKey(int key);
        void AddRange(IEnumerable<PlexEpisode> content);
        Task<PlexServerContent> GetFirstContentByCustom(Expression<Func<PlexServerContent, bool>> predicate);
        Task DeleteEpisode(PlexEpisode content);
        void UpdateRange(IEnumerable<PlexServerContent> existingContent);
    }
}