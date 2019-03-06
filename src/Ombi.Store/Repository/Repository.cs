using Ombi.Store.Context;
using Ombi.Store.Entities;

namespace Ombi.Store.Repository
{
    public class Repository<T> : BaseRepository<T>, IRepository<T> where T : Entity
    {
        public Repository(string ombiConnection) : base(ombiConnection)
        {
        }
    }
}