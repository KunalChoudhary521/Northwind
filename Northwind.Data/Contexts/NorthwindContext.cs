using Microsoft.EntityFrameworkCore;
using Northwind.Data.Entities;

namespace Northwind.Data.Contexts
{
    public class NorthwindContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        public NorthwindContext(DbContextOptions<NorthwindContext> options) : base(options)
        {
        }

        // TODO: remove this method and setup DB connection in API project
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source=Northwind.db;Version=3;")
                              .EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                        .HasIndex(e => new { e.FirstName, e.LastName })
                        .HasName("FullName");

            modelBuilder.Entity<OrderDetail>()
                        .ToTable("Order Details")
                        .HasKey(od => new { od.OrderId, od.ProductId });

            modelBuilder.Entity<Order>()
                        .Property(o => o.ShipperId)
                        .HasColumnName("ShipVia");
        }
    }
}