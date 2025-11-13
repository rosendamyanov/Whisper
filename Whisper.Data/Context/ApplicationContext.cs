using Microsoft.EntityFrameworkCore;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Data.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        // DbSets for your models
        public DbSet<User> Users { get; set; }
        public DbSet<UserCredentials> UserCredentials { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(user =>
            {
                user.HasIndex(u => u.Username).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();

                // 1:1 User -> UserCredentials
                user.HasOne(u => u.Credentials)
                    .WithOne(c => c.User)
                    .HasForeignKey<UserCredentials>(c => c.UserId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                // 1:N User -> RefreshTokens
                user.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 1:N User -> RevokedTokens
                user.HasMany(u => u.RevokedTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserCredentials configuration
            modelBuilder.Entity<UserCredentials>(uc =>
            {
                uc.HasIndex(x => x.UserId).IsUnique();
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(rt =>
            {
                rt.HasIndex(r => r.TokenHash).IsUnique();
            });

            // RevokedToken configuration
            modelBuilder.Entity<RevokedToken>(rvt =>
            {
                rvt.HasIndex(r => r.TokenHash);
                rvt.HasIndex(r => r.RevokedAt);
            });

            // 👇 NEW SECTION: Chat and Message relationships
            modelBuilder.Entity<Chat>(chat =>
            {
                // Many-to-many: Chat ↔ Users
                chat.HasMany(c => c.Users)
                    .WithMany(u => u.Chats)
                    .UsingEntity(j => j.ToTable("UserChats"));

                // 1:N Chat -> Messages
                chat.HasMany(c => c.Messages)
                    .WithOne(m => m.Chat)
                    .HasForeignKey(m => m.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Message>(msg =>
            {
                // 1:N User -> MessagesSent
                msg.HasOne(m => m.User)
                    .WithMany(u => u.MessagesSent)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Example of optional seeding (you can re-enable later if needed)
            /*
            var seededUserId = Guid.Parse("a1b2c3d4-1234-5678-9012-abcdef123456");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = seededUserId,
                Username = "test",
                Email = "test@test.com",
                CreatedAt = DateTime.Parse("2025-03-31 13:42:21"),
                Role = "User"
            });

            modelBuilder.Entity<UserCredentials>().HasData(new UserCredentials
            {
                Id = Guid.NewGuid(),
                UserId = seededUserId,
                PasswordHash = "$2a$...hash...",
                CreatedAt = DateTime.UtcNow
            });
            */
        }
    }
}
