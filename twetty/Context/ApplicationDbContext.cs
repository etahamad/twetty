using Microsoft.EntityFrameworkCore;
using twetty.Models;

namespace twetty.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Retweet> Retweets { get; set; }
        public DbSet<Reply> Replies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // One-to-Many: User to Tweets
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tweets)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.Username);

            // Many-to-Many: User to Likes (through Like entity)
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.Username, l.TweetId });

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.Username);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Tweet)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.TweetId);

            // Many-to-Many: User to Follows (through Follow entity)
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerUsername, f.TargetUsername });

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.FollowerUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowerUsername);

            modelBuilder.Entity<Follow>()
                .HasOne(f => f.TargetUser)
                .WithMany(u => u.Followings)
                .HasForeignKey(f => f.TargetUsername);

            // One-to-Many: User to Retweets
            modelBuilder.Entity<Retweet>()
                .HasOne(r => r.User)
                .WithMany(u => u.Retweets)
                .HasForeignKey(r => r.Username);

            // One-to-Many: Tweet to Retweets
            modelBuilder.Entity<Retweet>()
                .HasOne(r => r.Tweet)
                .WithMany(t => t.Retweets)
                .HasForeignKey(r => r.TweetId);

            // One-to-Many: User to Replies
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany(u => u.Replies)
                .HasForeignKey(r => r.Username);

            // One-to-Many: Tweet to Replies
            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Tweet)
                .WithMany(t => t.Replies)
                .HasForeignKey(r => r.TweetId);
        }
    }
}
