using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class LocationRepository : RepositoryBase<Location>
    {
        public LocationRepository(NorthwindContext context) : base(context)
        {
        }
    }
}