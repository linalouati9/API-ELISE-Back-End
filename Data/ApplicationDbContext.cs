using api_elise.Models;
using Microsoft.EntityFrameworkCore;

namespace api_elise.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Model> Models { get; set; }

        public DbSet<QRCode> QRCodes { get; set; }

        // customize data model configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Calls the base class method to ensure all default configuration is applied before adding additional configurations.
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Model>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Template); // Not required

                // One-to-many relationship
                entity.HasMany(e => e.QRCodes)
                      .WithOne() // No navigation property
                      .HasForeignKey(q => q.ModelId)
                      .OnDelete(DeleteBehavior.Cascade) // Cascading deletion
                      .IsRequired(true); // Every QR code is associated to a Model ==> ModelId is required in QRCode instance
            });

            // Configuration of the QRCode entity
            modelBuilder.Entity<QRCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Xslt).IsRequired().HasMaxLength(10000);
            });

            base.OnModelCreating(modelBuilder);
        }

        }

    }
