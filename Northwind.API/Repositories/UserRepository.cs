using Northwind.Data.Contexts;
using Northwind.Data.Entities;

namespace Northwind.API.Repositories
{
    public class UserRepository : RepositoryBase<User>
    {
        public UserRepository(NorthwindContext context) : base(context)
        {
        }
    }
}