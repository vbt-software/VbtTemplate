using AutoMapper;
using Core;
using Core.Caching;
using Core.Configuration;
using Core.CoreContext;
using DB.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Repository;
using Services.Customers;
using Services.Employees;
using Services.Login;
using Services.Roles;
using Services.SecurityService;
using Services.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TemplateProject.Infrastructure;

namespace TemplateProject
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<VbtContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped(typeof(IRepository<>), typeof(GeneralRepository<>));
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IEmployeesService, EmployeesService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ILoginService, LoginService>();
            services.AddTransient<IRoleService, RoleService>();
            services.AddSingleton<ICoreContext, CoreContext>();

            services.AddScoped<IWorkContext, WorkContext>();
            services.AddScoped<LoginFilter>();

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);

            services.Configure<VbtConfig>(Configuration.GetSection("VbtConfig"));

            services.AddTransient<IRedisCacheService, RedisCacheService>();
            services.AddTransient<IEncryptionService, EncryptionService>();

            services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()));

            // Register the Swagger generator, defining one or more Swagger documents

            //Add Localization             
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("tr-TR"),
                    new CultureInfo("en-US")
                };

                //options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
                options.DefaultRequestCulture = new RequestCulture("tr-TR","tr-TR");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            //End Localization

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations(); //Amaç Swagger'da Açýklama Girmek
                c.SwaggerDoc("CoreSwagger", new OpenApiInfo
                {
                    Title = "Swagger on ASP.NET Core",
                    Version = "1.0.0",
                    Description = "VBT Web Api",
                    TermsOfService = new Uri("http://swagger.io/terms/")
                });
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                // {
                //    {
                //        new OpenApiSecurityScheme
                //        {
                //            Type=SecuritySchemeType.ApiKey,
                //                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" }
                //            },
                //            new[] { "readAccess", "writeAccess" }
                //        }
                // });

                //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                //{
                //    Description = "VBT Web Api Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                //    Name = "Authorization",
                //    In = ParameterLocation.Header,
                //    Type = SecuritySchemeType.ApiKey,
                //});

                //c.OperationFilter<AddRequiredHeaderParameter>();

                c.AddSecurityDefinition("Bearer", //Name the security scheme
                    new OpenApiSecurityScheme
                    {
                        Description = "Custom Authorization Token Header Using the Bearer scheme.",
                        Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                        Scheme = "bearer" //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                        {
                               new OpenApiSecurityScheme{
                                Reference = new OpenApiReference{
                                    Id = "Bearer", //The name of the previously defined security scheme.
                                    Type = ReferenceType.SecurityScheme
                                }
                            },new List<string>()
                        }
                });
                c.OperationFilter<AddRequiredHeaderParameter>();
            });

            //3.1'de Destek Yok. services.AddSession();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseStaticFiles();

            //3.1'de Destek Yok. app.UseSession();

            app.UseCors("AllowAll");

            //Add Localization
            //var localizationOption = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            //app.UseRequestLocalization(localizationOption.Value);
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            var cookieProvider = options.Value.RequestCultureProviders
                .OfType<CookieRequestCultureProvider>()
                .First();
            var urlProvider = options.Value.RequestCultureProviders
                .OfType<QueryStringRequestCultureProvider>().First();

            //Set Cookie for default Language
            cookieProvider.Options.DefaultRequestCulture = new RequestCulture("tr-TR");
            urlProvider.Options.DefaultRequestCulture = new RequestCulture("tr-TR");

            cookieProvider.CookieName = "UserCulture";

            options.Value.RequestCultureProviders.Clear();
            options.Value.RequestCultureProviders.Add(cookieProvider);
            options.Value.RequestCultureProviders.Add(urlProvider);
            app.UseRequestLocalization(options.Value);
            //Finish Localization--------------------------------------------

            app.UseSwagger()
           .UseSwaggerUI(c =>
           {
               //TODO: Either use the SwaggerGen generated Swagger contract (generated from C# classes)
               c.SwaggerEndpoint("/swagger/CoreSwagger/swagger.json", "Swagger Test .Net Core");

               //TODO: Or alternatively use the original Swagger contract that's included in the static files
               // c.SwaggerEndpoint("/swagger-original.json", "Swagger Petstore Original");
           });
        }
    }
}
