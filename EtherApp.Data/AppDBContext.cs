using EtherApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EtherApp.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }
        public DbSet<Post> Posts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Like>()
                .HasKey(l => new {l.PostId, l.UserId});

            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Like)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Like)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comments
            modelBuilder.Entity<Comment>()
               .HasOne(l => l.Post)
               .WithMany(p => p.Comment)
               .HasForeignKey(l => l.PostId)
               .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Comment>()
                .HasOne(l => l.User)
                .WithMany(u => u.Comment)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
