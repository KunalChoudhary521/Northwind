using Microsoft.EntityFrameworkCore;
using Northwind.Data.Entities;

namespace Northwind.Data.Contexts
{
    public class NorthwindContext : DbContext
    {
        private const string UuidOssp = "uuid-ossp";
        private const string UuidGeneratev4 = "uuid_generate_v4()";

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<User> Users { get; set; }

        public NorthwindContext(DbContextOptions<NorthwindContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>()
                        .HasIndex(e => new { e.FirstName, e.LastName })
                        .HasName("FullName");

            modelBuilder.Entity<Location>().ToTable("Locations")
                        .HasIndex(l => l.Address)
                        .IsUnique();

            modelBuilder.Entity<OrderDetail>()
                        .ToTable("Order Details")
                        .HasKey(od => new { od.OrderId, od.ProductId });

            modelBuilder.Entity<Order>()
                        .Property(o => o.ShipperId)
                        .HasColumnName("ShipVia");

            modelBuilder.HasPostgresExtension(UuidOssp)
                        .Entity<User>()
                        .Property(u => u.UserIdentifier)
                        .HasDefaultValueSql(UuidGeneratev4);

            //Table sharing
            modelBuilder.Entity<User>()
                        .HasOne(u => u.RefreshToken)
                        .WithOne()
                        .HasForeignKey<RefreshToken>(rt => rt.RefreshTokenId);

            modelBuilder.Entity<RefreshToken>().ToTable("Users");
        }
    }
}