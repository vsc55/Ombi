using System.Linq;
using System.Threading.Tasks;
using Ombi.Core.Models.Search;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Repository;

namespace Ombi.Core.Rule.Rules.Search
{
    public class FullyAvailableTvRule : BaseSearchRule, IRules<SearchViewModel>
    {
        public int Priority => 200;
        public FullyAvailableTvRule(IRepository<SonarrCache> db)
        {
            _db = db;
        }

        private readonly IRepository<SonarrCache> _db;

        public Task<RuleResult> Execute(SearchViewModel obj)
        {
            if (obj.Type == RequestType.TvShow)
            {
                var vm = (SearchTvShowViewModel)obj;
                foreach (var season in vm.SeasonRequests)
                {
                    if (season.Episodes.All(x => x.Approved))
                    {
                        vm.FullyAvailable = true;
                    }
                }
            }
            return Task.FromResult(Success());
        }
    }
}