using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;
using JOIEnergy.Generator;
using JOIEnergy.Services;
using JOIEnergy.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace JOIEnergy
{
    public class Startup
    {
        private const string MOST_EVIL_PRICE_PLAN_ID = "price-plan-0";
        private const string RENEWABLES_PRICE_PLAN_ID = "price-plan-1";
        private const string STANDARD_PRICE_PLAN_ID = "price-plan-2";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Database
            services.AddDbContext<JOIEnergyDbContext>(options =>
                options.UseSqlite("Data Source=JOIEnergy.db"));

            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IMeterReadingService, MeterReadingService>();
            services.AddTransient<IPricePlanService, PricePlanService>();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Initialize database and seed data
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<JOIEnergyDbContext>();
                SeedDatabase(context);
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JOIEnergy API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void SeedDatabase(JOIEnergyDbContext context)
        {
            context.Database.EnsureCreated();

            // Seed Price PlansMicrosoft.Data.SqlClient.SqlException: 'A connection was successfully established with the server, but then an error occurred during the login process. (provider: SSL Provider, error: 0 - The certificate chain was issued by an authority that is not trusted.)'

            if (!context.PricePlans.Any())
            {
                context.PricePlans.AddRange(
                    new PricePlanEntity { PlanName = MOST_EVIL_PRICE_PLAN_ID, EnergySupplier = (int)Enums.Supplier.DrEvilsDarkEnergy, UnitRate = 10m },
                    new PricePlanEntity { PlanName = RENEWABLES_PRICE_PLAN_ID, EnergySupplier = (int)Enums.Supplier.TheGreenEco, UnitRate = 2m },
                    new PricePlanEntity { PlanName = STANDARD_PRICE_PLAN_ID, EnergySupplier = (int)Enums.Supplier.PowerForEveryone, UnitRate = 1m }
                );
            }

            // Seed Smart Meter Accounts
            if (!context.SmartMeterAccounts.Any())
            {
                context.SmartMeterAccounts.AddRange(
                    new SmartMeterAccount { SmartMeterId = "smart-meter-0", PricePlanId = MOST_EVIL_PRICE_PLAN_ID },
                    new SmartMeterAccount { SmartMeterId = "smart-meter-1", PricePlanId = RENEWABLES_PRICE_PLAN_ID },
                    new SmartMeterAccount { SmartMeterId = "smart-meter-2", PricePlanId = MOST_EVIL_PRICE_PLAN_ID },
                    new SmartMeterAccount { SmartMeterId = "smart-meter-3", PricePlanId = STANDARD_PRICE_PLAN_ID },
                    new SmartMeterAccount { SmartMeterId = "smart-meter-4", PricePlanId = RENEWABLES_PRICE_PLAN_ID }
                );
            }

            // Seed Sample Electricity Readings
            if (!context.ElectricityReadings.Any())
            {
                var generator = new ElectricityReadingGenerator();
                var smartMeterIds = new[] { "smart-meter-0", "smart-meter-1", "smart-meter-2", "smart-meter-3", "smart-meter-4" };

                foreach (var smartMeterId in smartMeterIds)
                {
                    var readings = generator.Generate(20);
                    var entities = readings.Select(r => new ElectricityReadingEntity
                    {
                        SmartMeterId = smartMeterId,
                        Reading = r.Reading,
                        Time = r.Time
                    }).ToList();
                    
                    context.ElectricityReadings.AddRange(entities);
                }
            }

            context.SaveChanges();
        }
    }
}
