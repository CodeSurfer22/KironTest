using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace DAL.ORM;

public partial class KironDALContext : DbContext
{
    public KironDALContext()
    {
    }

    public KironDALContext(DbContextOptions<KironDALContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<Navigation> Navigations { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<RegionHoliday> RegionHolidays { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Build the configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Or another directory where appsettings.json is located
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Retrieve the connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Configure the DbContext with the connection string
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.HolidayId).HasName("PK__Holidays__2D35D57A39FFC96D");

            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<Navigation>(entity =>
        {
            entity.ToTable("Navigation");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ParentId).HasColumnName("ParentID");
            entity.Property(e => e.Text)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK__Regions__ACD844A3D003EE70");

            entity.Property(e => e.RegionName).HasMaxLength(100);
        });

        modelBuilder.Entity<RegionHoliday>(entity =>
        {
            entity.HasKey(e => e.RegionHolidayId).HasName("PK__RegionHo__9F29F403DC7F5A99");

            entity.HasOne(d => d.Holiday).WithMany(p => p.RegionHolidays)
                .HasForeignKey(d => d.HolidayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RegionHol__Holid__4F7CD00D");

            entity.HasOne(d => d.Region).WithMany(p => p.RegionHolidays)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RegionHol__Regio__5070F446");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
