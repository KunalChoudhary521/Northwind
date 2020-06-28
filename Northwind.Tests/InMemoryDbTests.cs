using System.Linq;
using Microsoft.EntityFrameworkCore;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;
using Xunit;

namespace Northwind.Tests
{
    public class InMemoryDbTests
    {
        [Fact]
        public void Category_InsertSingleCategory_CategoryInserted()
        {
            var builder = new DbContextOptionsBuilder<NorthwindContext>();
            builder.UseInMemoryDatabase("InsertSingleCategory");

            using (var insertContext = new NorthwindContext(builder.Options))
            {
                var category = new Category
                {
                    CategoryName = "Beverages",
                    Description = "Soft drinks, coffees, teas, beers, and ales",
                };

                insertContext.Categories.Add(category);
                insertContext.SaveChanges();
            }

            using (var getContext = new NorthwindContext(builder.Options))
            {
                Assert.Equal(1, getContext.Categories.Count());

                var categoryRetrieved = getContext.Categories.FirstOrDefault();
                Assert.NotNull(categoryRetrieved);
                Assert.Equal("Beverages", categoryRetrieved.CategoryName);
                Assert.Equal("Soft drinks, coffees, teas, beers, and ales", categoryRetrieved.Description);
            }
        }
    }
}