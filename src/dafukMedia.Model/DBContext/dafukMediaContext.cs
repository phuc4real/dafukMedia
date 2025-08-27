using dafukMedia.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace dafukMedia.Data.DBContext;

public class dafukMediaContext : DbContext
{
    public dafukMediaContext(DbContextOptions<dafukMediaContext> options) : base(options)
    {
    }

    public DbSet<Artist> Artists { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Metadata> Metadata { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<ScanJob> ScanJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes for better performance
        modelBuilder.Entity<Track>()
            .HasIndex(t => t.FileHash)
            .HasDatabaseName("IX_Track_FileHash");

        modelBuilder.Entity<Track>()
            .HasIndex(t => t.FilePath)
            .IsUnique()
            .HasDatabaseName("IX_Track_FilePath");

        modelBuilder.Entity<Artist>()
            .HasIndex(a => a.Name)
            .HasDatabaseName("IX_Artist_Name");

        modelBuilder.Entity<Album>()
            .HasIndex(a => a.Title)
            .HasDatabaseName("IX_Album_Title");

        modelBuilder.Entity<Genre>()
            .HasIndex(g => g.Name)
            .IsUnique()
            .HasDatabaseName("IX_Genre_Name");

        modelBuilder.Entity<Metadata>()
            .HasIndex(m => new { m.TrackId, m.Key })
            .IsUnique()
            .HasDatabaseName("IX_Metadata_TrackId_Key");

        // Configure relationships
        modelBuilder.Entity<Track>()
            .HasOne(t => t.Artist)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Track>()
            .HasOne(t => t.Album)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.AlbumId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Album>()
            .HasOne(a => a.Artist)
            .WithMany(ar => ar.Albums)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Metadata>()
            .HasOne(m => m.Track)
            .WithMany(t => t.Metadata)
            .HasForeignKey(m => m.TrackId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
