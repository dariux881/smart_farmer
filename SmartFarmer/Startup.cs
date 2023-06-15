using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Hubs;
using SmartFarmer.Security;
using SmartFarmer.Services;

namespace SmartFarmer
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
            services.AddScoped<ISmartFarmerUserAuthenticationService, SmartFarmerUserJWTAuthenticationService>();
            services.AddScoped<ISmartFarmerGardenControllerService, SmartFarmerGardenControllerService>();
            services.AddScoped<ISmartFarmerRepository, SmartFarmerInMemoryRepository>();

            services.AddDbContext<SmartFarmerDbContext>(
                options => options.UseInMemoryDatabase("SmartFarmerInMemoryDB"));

            services.AddOptions();
            // configure strongly typed settings object
            services.Configure<SecretKey>(Configuration.GetSection("Jwt"));

            services
                .AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    o =>
                    {
                        o.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidIssuer = Configuration["Jwt:Issuer"],
                            ValidAudience = Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"])),
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = true
                        };

                        o.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                if (string.IsNullOrWhiteSpace(accessToken))
                                {
                                    accessToken = context.Request.Headers["Authorization"]
                                        .ToString()
                                        .Replace("Bearer ", "");
                                }
                                
                                // If the request is for our hub...
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) 
                                    && (path.StartsWithSegments("/" + nameof(FarmerGardenHub), System.StringComparison.OrdinalIgnoreCase)))
                                {
                                    // Read the token out of the query string
                                    context.Token = accessToken;
                                }
                                
                                return Task.CompletedTask;
                            }
                        };
                    });

            services.AddAuthorization();

            services.AddControllers();
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, FarmerUserIdProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                }); 
                // c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartFarmer", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFarmer v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<FarmerGardenHub>(nameof(FarmerGardenHub));
            });
        }
    }
}
