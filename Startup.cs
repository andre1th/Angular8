using Angular8.Data;
using Angular8.Data.Repositories;
using Angular8.Models.IRepositories;
using Chifang.Lib.MultiLang;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSB.Data.NetCore;
using SSB.Data.NetCore.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SpaServices.AngularCli;

namespace Angular8
{
    public class Startup
    {
        const string SystemId = "mvc";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            
            //services.AddSingleton<MainHelper>();
            // 從 appsettings.json 中取得 JWT 配置
            //var jwtOptions = Configuration.GetSection(nameof(JwtOptions));
            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            //// 設定 JwtIssuerOptions
            //services.Configure<JwtOptions>(options =>
            //{
            //    options.Issuer = jwtOptions[nameof(JwtOptions.Issuer)];
            //    options.Audience = jwtOptions[nameof(JwtOptions.Audience)];
            //    options.ValidFor = TimeSpan.FromMinutes(10);
            //    options.SigningCredentials = new SigningCredentials(
            //        new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions[nameof(JwtOptions.SecretKey)])), SecurityAlgorithms.HmacSha256);
            //});
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "http://localhost:5000";
                options.RequireHttpsMetadata = false;
                options.ClientId = "mvc";
                options.SaveTokens = true;
            })
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = "http://localhost:5000";
                options.ApiName = "web_api";
                options.NameClaimType = "name";
                options.RequireHttpsMetadata = false;
            });

            services.AddCors(o => o.AddPolicy("CORSP", builder =>
            {
                builder.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
            }));

            //services
            //.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //.AddJwtBearer(options =>
            //{
            //    // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
            //    options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉
            //    options.Audience = "web_api";
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            //        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            //        // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            //        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            //        // 一般我們都會驗證 Issuer
            //        ValidateIssuer = true,
            //        ValidIssuer = Configuration.GetValue<string>("JwtSettings:Issuer"),

            //        // 若是單一伺服器通常不太需要驗證 Audience
            //        ValidateAudience = false,
            //        //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            //        // 一般我們都會驗證 Token 的有效期間
            //        ValidateLifetime = true,

            //        // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            //        ValidateIssuerSigningKey = false,

            //        // "1234567890123456" 應該從 IConfiguration 取得
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("JwtSettings:SignKey")))
            //    };
            //});

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddSingleton<IConfiguration>(Configuration);
            JsonMultiLang.CreateInstance("en", "wwwroot\\assets\\i18n");
            services.Configure<SystemInfo>(opt =>
            {
                opt.SystemId = "mvc";
            });
            services.AddEntityFrameworkSqlServer()
                   .AddDbContext<SHCMESDbContext>(option => option.UseSqlServer(connectionString));
            services.AddSsbAllRepositories();
            //services.AddScoped<ISeasonRepository, SeasonRepository>();
            //services.AddTransient(typeof(ISeasonRepository).GetType(), typeof(ISeasonRepository).GetType().Assembly.GetType());
            services.AddTransient<ISeasonRepository, SeasonRepository>();
            services.AddTransient<IMultiDictTreeRepository, MultiDictTreeRepository>();
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IAllLangRepository, AllLangRepository>();
            services.AddTransient<IAllMultiDictDtlRepository, AllMultiDictDtlRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            app.UseCors("CORSP");
            app.UseAuthentication();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseSpaStaticFiles();           

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                  name: "areas",
                  template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                );
            });
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
