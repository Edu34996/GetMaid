using Business.Profiles; // Added to reference AuthProfiles
using Business.Services;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.Entities;
using Data;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utils.Helpers;


namespace Business
{
    public static class IOC
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<GetMaidContext>(options => 
                options.UseSqlite(configuration.GetConnectionString("app_db")));

            services.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<GetMaidContext>()
                .AddDefaultTokenProviders();

            // Explicit manual registration of AutoMapper profiles
            services.AddAutoMapper(config =>
            {
                config.AddProfile<AuthProfiles>();
                config.AddProfile<WorkerProfiles>();
                config.AddProfile<CustomerProfiles>();
                config.AddProfile<JobProfiles>();
                config.AddProfile<BookingProfiles>();
                config.AddProfile<ReviewProfiles>();
            });
            
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IWorkerService, WorkerService>();
            services.AddScoped<IMessageService, MessageService>();
            
            services.AddHttpClient<IGeocodingService, GeocodingService>();
            
            return services;
        }
    }
}