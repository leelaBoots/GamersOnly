using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        /* this extends IServiceCollection with any identity related stuff */
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config) {

            services.AddIdentityCore<AppUser>(opt => 
            {
              opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options=>
                {
                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };

                    options.Events = new JwtBearerEvents {
                      OnMessageReceived = context => {
                        // we need to do this because we dont have access to HTTP request headers. "access_token" is a SignalR property
                        var accessToken  = context.Request.Query["access_token"];

                        var path = context.HttpContext.Request.Path;

                        // "/hubs" is configured in Program.cs for SignalR 
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs")) {
                          context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                      }
                    };
                });


            services.AddAuthorization(opt => {
              // corresponds to setup in AdminControllers.cs
              opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
              opt.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;

        }
    }
}