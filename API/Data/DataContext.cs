// namespace should match folder structure
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
      IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
      IdentityRoleClaim<int>, IdentityUserToken<int>> 
    {
        public DataContext( DbContextOptions options) : base(options)
        {
        }

        // We need a DbSet here so that we can query UserLike
        public DbSet<UserLike> Likes { get; set; }

        // this is for messages between users
        public DbSet<Message> Messages { get; set; }

        // this is for marking messages as read in real time using database.
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections {get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
          base.OnModelCreating(builder);

          builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

          builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles)
            .WithOne(u => u.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

          builder.Entity<UserLike>()
            .HasKey(k => new {k.SourceUserId, k.TargetUserId});

          builder.Entity<UserLike>()
            .HasOne(s => s.SourceUser)
            .WithMany(l => l.LikedUsers)
            .HasForeignKey(s => s.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);

          builder.Entity<UserLike>()
            .HasOne(s => s.TargetUser)
            .WithMany(l => l.LikedByUsers)
            .HasForeignKey(s => s.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);

          // user cannot delete messge from database
          builder.Entity<Message>()
            .HasOne(u => u.Recipient)
            .WithMany(m => m.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);
          
          builder.Entity<Message>()
            .HasOne(u => u.Sender)
            .WithMany(m => m.MessagesSent)
            .OnDelete(DeleteBehavior.Restrict);

        }
    }
}