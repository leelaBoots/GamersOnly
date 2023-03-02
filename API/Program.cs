var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSignalR();

//Config.EnableCors(cors);

// configure the http request pipeline

var app = builder.Build();

// our requests go though a series of middleware on the way in and on the way out

// our own Middleware class will handle all exceptions
// middleware will occur for every single request. the alternative action filters can be applied more selectively
app.UseMiddleware<ExceptionMiddleware>();

// redirect http to https request
app.UseHttpsRedirection();

app.UseCors(builder => builder.AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials()
  .WithOrigins("https://localhost:4200")); // placement of this after routing and before authorization is important

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
// will implement this later in lesson when we use SignalR
//app.MapHub<PresenceHub>("hubs/presence");
//app.MapHub<MessageHub>("hubs/message");
//app.MapFallbackToController("Index", "Fallback");

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior, true");
// create a scope for the services that we create here
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try {
    var context = services.GetRequiredService<DataContext>();
    //var userManager = services.GetRequiredService<UserManager<AppUser>>();
    //var roleManager = services.GetRequiredService<RoleManager<AppUser>>();

    // automatically recreate our database if it is dropped
    // also allows us to just restart app to apply any migrations
    await context.Database.MigrateAsync();
    // seed our database with our json file containing fake data
    await Seed.SeedUsers(context);
    // await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex) {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");

}

await app.RunAsync();
//app.Run();

/*

// old version of program class from dotnet 5.

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // create a scope for the services that we create here
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try {
                var context = services.GetRequiredService<DataContext>();
                // automatically recreate our database if it is dropped
                // also allows us to just restart app to apply any migrations
                await context.Database.MigrateAsync();
                // seed our database with our json file containing fake data
                await Seed.SeedUsers(context);
            }
            catch (Exception ex) {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occured during migration");

            }

            // run the app here since we moved it our form the .Build() statement above
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
*/
