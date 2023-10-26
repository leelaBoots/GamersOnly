using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {

        public static async Task ClearConnections(DataContext context) {
          // this way of deleting the connections would be ineficient if we had thousands of rows. truncating the table directly would be faster, but would
          // have to make raw sql commands directly to the database which can be frowned upon
          context.Connections.RemoveRange(context.Connections);
          await context.SaveChangesAsync();

        }


        // the logic to get the data out of the seed json file and into our database.
        // it is recommended that we call this method from the Program.cs class to seed data
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            // this will create a List of AppUser. basically the json is now stored in a C# data structure that we can use
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            var roles = new List<AppRole> {
              new AppRole{Name = "Member" },
              new AppRole{Name = "Admin" },
              new AppRole{Name = "Moderator" }
            };

            foreach (var role in roles) {
              await roleManager.CreateAsync(role);
            }

            foreach (var user in users) {
                // using AspNetCore.Identity to handle this now.
                //using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();

                // using AspNetCore.Identity to handle this now.
                //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password"));
                //user.PasswordSalt = hmac.Key;

                // need to specify these dates from Seed data as Utc because Postgres requires it
                user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
                user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);

                // this creates and saves the changes in the database so we dont need to call the SaveChangesAsync() method anymore after this
                if (user.UserName == "lisa") {
                  // special case for user lisa from UserSeedData.json, use a simple password, this user is used for demos
                  await userManager.CreateAsync(user, "Password");
                } else {
                  await userManager.CreateAsync(user, "Pa$$w0rd");
                }
                await userManager.AddToRoleAsync(user, "Member");
            }

            var admin = new AppUser {
              UserName = "Admin"
            };

            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
            
        }
    }
}