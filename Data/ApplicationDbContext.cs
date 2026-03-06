using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TripTracker.Models;

namespace TripTracker.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<ItineraryItem> ItineraryItems => Set<ItineraryItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<PackingTemplate> PackingTemplates => Set<PackingTemplate>();
    public DbSet<PackingTemplateItem> PackingTemplateItems => Set<PackingTemplateItem>();
    public DbSet<PackingItem> PackingItems => Set<PackingItem>();
    public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Trip>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ItineraryItem>()
            .HasOne(i => i.Trip)
            .WithMany(t => t.ItineraryItems)
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Expense>()
            .HasOne(e => e.Trip)
            .WithMany(t => t.Expenses)
            .HasForeignKey(e => e.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PackingItem>()
            .HasOne(p => p.Trip)
            .WithMany(t => t.PackingItems)
            .HasForeignKey(p => p.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DocumentLink>()
            .HasOne(d => d.Trip)
            .WithMany(t => t.DocumentLinks)
            .HasForeignKey(d => d.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PackingTemplateItem>()
            .HasOne(i => i.PackingTemplate)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.PackingTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
