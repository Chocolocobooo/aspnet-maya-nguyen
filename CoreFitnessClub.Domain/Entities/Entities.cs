using Microsoft.AspNetCore.Identity;

namespace CoreFitnessClub.Domain.Entities;

/// <summary>
/// Extended Identity user with fitness-club profile data.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;

    // Navigation
    public UserMembership? UserMembership { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
}

/// <summary>
/// A membership plan offered by the club (Standard / Premium).
/// </summary>
public class MembershipPlan
{
    public int    Id          { get; set; }
    public string Name        { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price      { get; set; }
    public int    MaxClasses  { get; set; }   // per month; 0 = unlimited
    public bool   IsActive    { get; set; } = true;

    // Navigation
    public ICollection<PlanFeature>    Features        { get; set; } = [];
    public ICollection<UserMembership> UserMemberships { get; set; } = [];
}

/// <summary>
/// A bullet-point feature belonging to a membership plan.
/// </summary>
public class PlanFeature
{
    public int    Id             { get; set; }
    public int    MembershipPlanId { get; set; }
    public string Description    { get; set; } = string.Empty;
    public MembershipPlan? MembershipPlan { get; set; }
}

/// <summary>
/// Links a user to a plan — their active subscription.
/// </summary>
public class UserMembership
{
    public int    Id               { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public int    MembershipPlanId  { get; set; }
    public DateTime StartDate       { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate        { get; set; }
    public bool   IsActive          { get; set; } = true;

    public ApplicationUser? ApplicationUser { get; set; }
    public MembershipPlan?  MembershipPlan  { get; set; }
}

/// <summary>
/// A fitness class (e.g. CrossFit, Yoga, HIIT).
/// </summary>
public class FitnessClass
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    public string   Category    { get; set; } = string.Empty;
    public int      DurationMin { get; set; }
    public string   Level       { get; set; } = string.Empty;
    public bool     IsActive    { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = [];
}

/// <summary>
/// A single booking of a fitness class by a user.
/// </summary>
public class Booking
{
    public int      Id                { get; set; }
    public string   ApplicationUserId { get; set; } = string.Empty;
    public int      FitnessClassId    { get; set; }
    public DateTime BookedAt          { get; set; } = DateTime.UtcNow;
    public DateTime ClassDateTime     { get; set; }
    public BookingStatus Status       { get; set; } = BookingStatus.Confirmed;

    public ApplicationUser? ApplicationUser { get; set; }
    public FitnessClass?    FitnessClass    { get; set; }
}

public enum BookingStatus { Confirmed, Cancelled, Attended }
