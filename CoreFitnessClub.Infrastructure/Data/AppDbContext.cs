using CoreFitnessClub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreFitnessClub.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MembershipPlan>    MembershipPlans    => Set<MembershipPlan>();
    public DbSet<PlanFeature>       PlanFeatures       => Set<PlanFeature>();
    public DbSet<UserMembership>    UserMemberships    => Set<UserMembership>();
    public DbSet<FitnessClass>      FitnessClasses     => Set<FitnessClass>();
    public DbSet<Booking>           Bookings           => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── MembershipPlan ────────────────────────────────────────────────────
        builder.Entity<MembershipPlan>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            e.HasMany(p => p.Features)
             .WithOne(f => f.MembershipPlan)
             .HasForeignKey(f => f.MembershipPlanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── UserMembership ────────────────────────────────────────────────────
        builder.Entity<UserMembership>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.ApplicationUser)
             .WithOne(u => u.UserMembership)
             .HasForeignKey<UserMembership>(m => m.ApplicationUserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.MembershipPlan)
             .WithMany(p => p.UserMemberships)
             .HasForeignKey(m => m.MembershipPlanId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── FitnessClass ──────────────────────────────────────────────────────
        builder.Entity<FitnessClass>(e =>
        {
            e.HasKey(c => c.Id);
        });

        // ── Booking ───────────────────────────────────────────────────────────
        builder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.HasOne(b => b.ApplicationUser)
             .WithMany(u => u.Bookings)
             .HasForeignKey(b => b.ApplicationUserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(b => b.FitnessClass)
             .WithMany(c => c.Bookings)
             .HasForeignKey(b => b.FitnessClassId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed data ─────────────────────────────────────────────────────────
        builder.Entity<MembershipPlan>().HasData(
            new MembershipPlan
            {
                Id = 1, Name = "Standard Membership",
                Description = "Full access to all gym facilities.",
                Price = 495, MaxClasses = 20, IsActive = true
            },
            new MembershipPlan
            {
                Id = 2, Name = "Premium Membership",
                Description = "Priority support and premium facilities.",
                Price = 595, MaxClasses = 20, IsActive = true
            }
        );

        builder.Entity<PlanFeature>().HasData(
            new PlanFeature { Id = 1, MembershipPlanId = 1, Description = "Standard Locker" },
            new PlanFeature { Id = 2, MembershipPlanId = 1, Description = "High-energy group fitness classes" },
            new PlanFeature { Id = 3, MembershipPlanId = 1, Description = "Motivating & supportive environment" },
            new PlanFeature { Id = 4, MembershipPlanId = 2, Description = "Priority Support & Premium Locker" },
            new PlanFeature { Id = 5, MembershipPlanId = 2, Description = "High-energy group fitness classes" },
            new PlanFeature { Id = 6, MembershipPlanId = 2, Description = "Motivating & supportive environment" }
        );

        builder.Entity<FitnessClass>().HasData(
            new FitnessClass { Id = 1, Name = "CrossFit",        Category = "Strength",   DurationMin = 60, Level = "All Levels",   IsActive = true, Description = "High-intensity functional movements." },
            new FitnessClass { Id = 2, Name = "Yoga Flow",       Category = "Flexibility", DurationMin = 75, Level = "Beginner",     IsActive = true, Description = "Dynamic breath-to-movement sequences." },
            new FitnessClass { Id = 3, Name = "HIIT",            Category = "Cardio",     DurationMin = 45, Level = "Intermediate", IsActive = true, Description = "Explosive cardio intervals." },
            new FitnessClass { Id = 4, Name = "Indoor Cycling",  Category = "Cardio",     DurationMin = 50, Level = "All Levels",   IsActive = true, Description = "High-energy indoor rides." },
            new FitnessClass { Id = 5, Name = "Boxing",          Category = "Combat",     DurationMin = 60, Level = "Intermediate", IsActive = true, Description = "Technical striking with conditioning." },
            new FitnessClass { Id = 6, Name = "Pilates",         Category = "Core",       DurationMin = 60, Level = "Beginner",     IsActive = true, Description = "Core-focused control and stability." }
        );
    }
}
