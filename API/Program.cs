using API.Entities;
using API.SignalR;
using Microsoft.AspNetCore.Identity;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// add services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCors();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 0;
});
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSignalR();

//Config.EnableCors(cors);

// Comment all this stuff out while using the sqlite DB
var connString = "";
if (builder.Environment.IsDevelopment()) 
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else 
{
// Use connection string provided at runtime by FlyIO.
  var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

  // Parse connection URL to connection string for Npgsql
  connUrl = connUrl.Replace("postgres://", string.Empty);
  var pgUserPass = connUrl.Split("@")[0];
  var pgHostPortDb = connUrl.Split("@")[1];
  var pgHostPort = pgHostPortDb.Split("/")[0];
  var pgDb = pgHostPortDb.Split("/")[1];
  var pgUser = pgUserPass.Split(":")[0];
  var pgPass = pgUserPass.Split(":")[1];
  var pgHost = pgHostPort.Split(":")[0];
  var pgPort = pgHostPort.Split(":")[1];
  var updatedHost = pgHost.Replace("flycast", "internal");

  connString = $"Server={updatedHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});


// configure the http request pipeline

var app = builder.Build();

// our requests go though a series of middleware on the way in and on the way out

// our own Middleware class will handle all exceptions
// middleware will occur for every single request. the alternative action filters can be applied more selectively
app.UseMiddleware<ExceptionMiddleware>();

// redirect http to https request
app.UseHttpsRedirection();

app.UseCors(builder => builder
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials()
  .WithOrigins("https://localhost:4200")); // placement of this after routing and before authorization is important

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();  // finds the index html files from the root folder by default
app.UseStaticFiles();   // by default, will look for a wwwroot folder then serve content from there

app.MapControllers();
// SignalR
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior, true");
// create a scope for the services that we create here
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try {
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

    // automatically recreate our database if it is dropped
    // also allows us to just restart app to apply any migrations
    await context.Database.MigrateAsync();

    // we need to clear out all message group connections from DB, just in case the application had to restart
    // this works for small scale, but not good if there are thousands of entries
    //context.Connections.RemoveRange(context.Connections);
    // becareful with this approach, we are modifying database without using Entity Framework
    // this SQL command does not work in SQLite:
    //await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");

    //await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]"); // This line is for SQLite
    await Seed.ClearConnections(context);  // This line is for Postgres

    // seed our database with our json file containing fake data
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex) {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occured during migration");

}

//await app.RunAsync();
app.Run();

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
