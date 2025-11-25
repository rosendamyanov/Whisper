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
        public DbSet<LiveStream> Streams { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<VoiceSession> VoiceSessions { get; set; }
        public DbSet<VoiceParticipant> VoiceParticipants { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER CONFIG
            modelBuilder.Entity<User>(user =>
            {
                user.HasIndex(u => u.Username).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();

                user.HasOne(u => u.Credentials)
                    .WithOne(c => c.User)
                    .HasForeignKey<UserCredentials>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                user.HasMany(u => u.RefreshTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                user.HasMany(u => u.RevokedTokens)
                    .WithOne(rt => rt.User)
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                user.HasMany(u => u.MessagesSent)
                    .WithOne(m => m.User)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // FRIENDSHIPS
            modelBuilder.Entity<Friendship>(f =>
            {
                f.HasOne(fr => fr.User)
                 .WithMany(u => u.Friendships)
                 .HasForeignKey(fr => fr.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                f.HasOne(fr => fr.Friend)
                 .WithMany()
                 .HasForeignKey(fr => fr.FriendId)
                 .OnDelete(DeleteBehavior.Restrict);

                f.HasIndex(fr => new { fr.UserId, fr.FriendId }).IsUnique();
            });

            // MESSAGES
            modelBuilder.Entity<Message>(msg =>
            {
                msg.HasOne(m => m.Chat)
                   .WithMany(c => c.Messages)
                   .HasForeignKey(m => m.ChatId)
                   .OnDelete(DeleteBehavior.Cascade);
            });

            // CHATS
            modelBuilder.Entity<Chat>(chat =>
            {
                chat.HasMany(c => c.Users)
                    .WithMany(u => u.Chats)
                    .UsingEntity(j => j.ToTable("UserChats"));

                chat.HasOne(c => c.ActiveStream)
                    .WithOne()
                    .HasForeignKey<Chat>(c => c.ActiveStreamId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // STREAMS
            modelBuilder.Entity<LiveStream>(stream =>
            {
                stream.HasOne(s => s.HostUser)
                    .WithMany()
                    .HasForeignKey(s => s.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // VOICE SESSIONS
            modelBuilder.Entity<VoiceSession>(vs =>
            {
                vs.HasOne(v => v.HostUser)
                    .WithMany()
                    .HasForeignKey(v => v.HostUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                vs.HasOne(v => v.Chat)
                    .WithMany()
                    .HasForeignKey(v => v.ChatId)
                    .OnDelete(DeleteBehavior.Cascade);

                vs.HasMany(v => v.Participants)
                    .WithOne(p => p.VoiceSession)
                    .HasForeignKey(p => p.VoiceSessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // VOICE PARTICIPANTS
            modelBuilder.Entity<VoiceParticipant>(vp =>
            {
                vp.HasOne(p => p.User)
                    .WithMany(u => u.VoiceParticipations)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                vp.HasIndex(p => new { p.VoiceSessionId, p.UserId }).IsUnique();
            });
        }
    }
}
