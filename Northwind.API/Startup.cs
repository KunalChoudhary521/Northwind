using System.Collections.Generic;
using AutoMapper;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Northwind.API.Repositories;
using Northwind.API.Services;
using Northwind.Data.Contexts;
using Northwind.Data.Entities;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Northwind.API
{
    public class Startup
    {
        private const string SwaggerDocName = "northwind";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            ConfigureDbContext(services);
            ConfigureThirdPartyDependencies(services);
            ConfigureRepositories(services);
            ConfigureAppServices(services);
            services.AddHealthChecks()
                    .AddDbContextCheck<NorthwindContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseProblemDetails();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/api/health");
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{SwaggerDocName}/swagger.json", "Northwind API");
                options.DocExpansion(DocExpansion.None);
            });
        }

        private void ConfigureDbContext(IServiceCollection services)
        {
            services.AddDbContext<NorthwindContext>(options =>
            {
                var connString = Configuration.GetConnectionString("NorthWindDB");
                options.UseNpgsql(connString);
                if(_env.IsDevelopment())
                    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
            });
        }

        private void ConfigureThirdPartyDependencies(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddProblemDetails();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc($"{SwaggerDocName}", new OpenApiInfo
                {
                    Title = "Northwind API",
                    Description = "API documentation for Northwind"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using Bearer scheme. <br> Enter: 'Bearer' [token]",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
        }

        private void ConfigureRepositories(IServiceCollection services)
        {
            services.AddScoped<IRepository<Category>, CategoryRepository>();
            services.AddScoped<IRepository<Product>, ProductRepository>();
            services.AddScoped<IRepository<Location>, LocationRepository>();
            services.AddScoped<IRepository<Supplier>, SupplierRepository>();
            services.AddScoped<IRepository<Customer>, CustomerRepository>();
            services.AddScoped<IRepository<Order>, OrderRepository>();
            services.AddScoped<IRepository<Shipper>, ShipperRepository>();
        }

        private void ConfigureAppServices(IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IShipperService, ShipperService>();
        }
    }
}