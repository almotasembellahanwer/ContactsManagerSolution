﻿using ContactsManager.Core.Domain.IdentityEntities;
using CRUDExample.Filters.ActionFilter;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
    public static class ConfigureServicesExtension
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ResponseHeaderActionFilter>();
            services.AddControllersWithViews(options =>
            {
                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>(); ;
                options.Filters.Add(new ResponseHeaderActionFilter(logger)
                {
                    Key = "My-Key-From-Global",
                    Value = "My-Value-From-Global",
                    Order = 2
                });
            }

            );

            //add services into IoC container

            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            services.AddScoped<ICountriesAdderService, CountriesAdderService>();
            services.AddScoped<ICountriesGetterService, CountriesGetterService>();
            services.AddScoped<ICountriesUploaderService, CountriesUploaderService>();

            services.AddScoped<IPersonsAdderService, PersonsAdderService>();
            services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
            services.AddScoped<PersonsGetterService, PersonsGetterService>();
            services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
            services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
            services.AddScoped<IPersonsSorterService, PersonsSorterService>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser,ApplicationRole,ApplicationDbContext,Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole,ApplicationDbContext,Guid>>();
        }
    }
}
