using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class ExternalRepository<T> : BaseRepository<T> where T : Entity
    {
        public ExternalRepository(string externalConn) : base(externalConn)
        {
        }
    }
}