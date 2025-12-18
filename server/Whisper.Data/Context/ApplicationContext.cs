using Microsoft.EntityFrameworkCore;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.Data.Models.Messages;

namespace Whisper.Data.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        // --- CORE TABLES ---
        public DbSet<User> Users { get; set; }
        public DbSet<UserCredentials> UserCredentials { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        // --- CHAT & MESSAGING TABLES ---
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageReceipt> MessageReceipts { get; set; }
        public DbSet<MessageReaction> MessageReactions { get; set; }

        // REMOVED: Streams, VoiceSessions, VoiceParticipants

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================= USER CONFIG =================
            modelBuilder.Entity<User>(user =>
            {
                user.HasIndex(u => u.Username).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();

                user.HasOne(u => u.Credentials)
                    .WithOne(c => c.User)
                    .HasForeignKey<UserCredentials>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                user.HasMany(u => u.RefreshTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Restrict);
                user.HasMany(u => u.RevokedTokens).WithOne(rt => rt.User).HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Restrict);
                user.HasMany(u => u.MessagesSent).WithOne(m => m.User).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);

                user.HasQueryFilter(u => !u.IsDeleted);
            });

            // ================= FRIENDSHIPS =================
            modelBuilder.Entity<Friendship>(f =>
            {
                f.HasOne(fr => fr.User).WithMany(u => u.Friendships).HasForeignKey(fr => fr.UserId).OnDelete(DeleteBehavior.Restrict);
                f.HasOne(fr => fr.Friend).WithMany().HasForeignKey(fr => fr.FriendId).OnDelete(DeleteBehavior.Restrict);
                f.HasIndex(fr => new { fr.UserId, fr.FriendId }).IsUnique();
                f.HasQueryFilter(fr => !fr.IsDeleted);
            });

            // ================= CHATS =================
            modelBuilder.Entity<Chat>(chat =>
            {
                // Link Users <-> Chats
                chat.HasMany(c => c.Users)
                    .WithMany(u => u.Chats)
                    .UsingEntity(j => j.ToTable("UserChats"));

                // REMOVED: ActiveStream relation

                chat.HasQueryFilter(c => !c.IsDeleted);
            });

            // ================= MESSAGES (CORE) =================
            modelBuilder.Entity<Message>(msg =>
            {
                msg.HasOne(m => m.Chat)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ChatId)
                   .OnDelete(DeleteBehavior.Cascade);

                // Self-Referencing Reply
                msg.HasOne(m => m.ReplyTo)
                   .WithMany()
                   .HasForeignKey(m => m.ReplyToId)
                   .OnDelete(DeleteBehavior.Restrict);

                msg.HasQueryFilter(m => !m.IsDeleted);
            });

            // ================= MESSAGE RECEIPTS =================
            modelBuilder.Entity<MessageReceipt>(mr =>
            {
                mr.HasIndex(r => new { r.MessageId, r.UserId }).IsUnique();

                mr.HasOne(r => r.Message)
                  .WithMany(m => m.ReadReceipts)
                  .HasForeignKey(r => r.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

                mr.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            });

            // ================= MESSAGE REACTIONS =================
            modelBuilder.Entity<MessageReaction>(mr =>
            {
                mr.HasIndex(r => new { r.MessageId, r.UserId, r.Content }).IsUnique();

                mr.HasOne(r => r.Message)
                  .WithMany(m => m.Reactions)
                  .HasForeignKey(r => r.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

                mr.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}