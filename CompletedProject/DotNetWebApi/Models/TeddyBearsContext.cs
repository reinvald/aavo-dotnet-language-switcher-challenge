using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DotNetWebApi.Models;

public partial class TeddyBearsContext : DbContext
{
    public TeddyBearsContext()
    {
    }

    public TeddyBearsContext(DbContextOptions<TeddyBearsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Picnic> Picnics { get; set; }

    public virtual DbSet<PicnicLocation> PicnicLocations { get; set; }

    public virtual DbSet<TeddyBear> TeddyBears { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=SL-011179202657\\BMACKAY;Initial Catalog=TeddyBears;Integrated Security=True;TrustServerCertificate=false;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Picnic>(entity =>
        {
            entity.ToTable("Picnic", tb => tb.HasComment("Teddy bears have picnics.  Picnics have locations and participants (teddy bears)"));

            entity.Property(e => e.Id).HasComment("The primary key");
            entity.Property(e => e.HasFood)
                .HasDefaultValueSql("((1))")
                .HasComment("Will the picnic have food (default = true)");
            entity.Property(e => e.HasMusic)
                .HasDefaultValueSql("((1))")
                .HasComment("Will the picnic have music (default = true)");
            entity.Property(e => e.LocationId).HasComment("A reference to where the picnic will be held (a foreign key into the PicnicLocation table)");
            entity.Property(e => e.PicnicName).HasComment("The name of the picnic.  All picnics have unique names");
            entity.Property(e => e.StartTime).HasComment("Picnics have a start time.  They always end at 6:00pm, when the mommies and daddies take them home to bed because they are tired little teddy bears");

            entity.HasOne(d => d.Location).WithMany(p => p.Picnics).HasConstraintName("FK_Picnic_PicnicLocation");

            entity.HasMany(d => d.TeddyBears).WithMany(p => p.Picnics)
                .UsingEntity<Dictionary<string, object>>(
                    "PicnicParticipant",
                    r => r.HasOne<TeddyBear>().WithMany()
                        .HasForeignKey("TeddyBearId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PicnicParticipants_TeddyBear"),
                    l => l.HasOne<Picnic>().WithMany()
                        .HasForeignKey("PicnicId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PicnicParticipants_Picnic"),
                    j =>
                    {
                        j.HasKey("PicnicId", "TeddyBearId")
                            .HasName("PK_persionId")
                            .IsClustered(false);
                        j.ToTable("PicnicParticipants", tb => tb.HasComment("The \"join table\" that defines the many-to-many relationship between Teddy Bears and Picnics"));
                        j.IndexerProperty<int>("PicnicId").HasComment("A reference to the picnic (via foreign key)");
                        j.IndexerProperty<int>("TeddyBearId").HasComment("A reference to the teddy bear (picnic participant) (via foreign key)");
                    });
        });

        modelBuilder.Entity<PicnicLocation>(entity =>
        {
            entity.ToTable("PicnicLocation", tb => tb.HasComment("The location of one or more picnics.  Every picnic must have a location"));

            entity.Property(e => e.Id).HasComment("The Primary Key");
            entity.Property(e => e.Capacity)
                .HasDefaultValueSql("((25))")
                .HasComment("How many teddy bears can be accommodated at this location");
            entity.Property(e => e.LocationName).HasComment("The name of the location (must be unique)");
            entity.Property(e => e.Municipality).HasComment("In what village, town or city is this location");
        });

        modelBuilder.Entity<TeddyBear>(entity =>
        {
            entity.ToTable("TeddyBear", tb => tb.HasComment("Teddy Bears are soft and cuddly.  Each has a name and a unique personality"));

            entity.Property(e => e.Id).HasComment("The Primary Key");
            entity.Property(e => e.AccentColor).HasComment("Teddy Bears may have a secondary color.  The color is a string (but should be picked from a list)");
            entity.Property(e => e.Characteristic).HasComment("Teddy bears may have a defining characteristic - fluffy, polite, whatever");
            entity.Property(e => e.IsDressed)
                .HasDefaultValueSql("((1))")
                .HasComment("Is the Teddy Bear dressed (true or false)");
            entity.Property(e => e.Name).HasComment("The Teddy Bear's name.  Each is unique");
            entity.Property(e => e.OwnerName).HasComment("Who is the teddy bear's owner");
            entity.Property(e => e.PrimaryColor).HasComment("All teddy bears have a primary color.  The color is a string (but should be picked from a list)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
