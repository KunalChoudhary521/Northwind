using Microsoft.AspNetCore.Authorization;
using Northwind.Data.Entities;

namespace Northwind.API.Services
{
    public class PolicyService
    {
        public static bool Admin(AuthorizationHandlerContext context)
        {
            return context.User.IsInRole(nameof(Role.Admin));
        }

        public static bool SupplierAdmin(AuthorizationHandlerContext context)
        {
            return Admin(context) || context.User.IsInRole(nameof(Role.SupplierAdmin));
        }

        public static bool Supplier(AuthorizationHandlerContext context)
        {
            return Admin(context) || SupplierAdmin(context) || context.User.IsInRole(nameof(Role.Supplier));
        }

        public static void AddAdminPolicy(AuthorizationOptions options)
        {
            options.AddPolicy(nameof(Admin),policy => policy.RequireAssertion(Admin));
        }

        public static void AddSupplierPolicy(AuthorizationOptions options)
        {
            options.AddPolicy(nameof(SupplierAdmin),policy => policy.RequireAssertion(SupplierAdmin));
            options.AddPolicy(nameof(Supplier),policy => policy.RequireAssertion(Supplier));
        }
    }
}