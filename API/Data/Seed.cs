using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
        // the logic to get the data out of the seed json file and into our database.
        // it is recommended that we call this method from the Program.cs class to seed data
        public static async Task SeedUsers(DataContext context) {
            if (await context.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");

            // this will create a List of AppUser. basically the json is now stored in a C# data structure that we can use
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            foreach (var user in users) {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Password"));
                user.PasswordSalt = hmac.Key;

                // we do not need to use await here because it does not do anything with the database
                context.Users.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}