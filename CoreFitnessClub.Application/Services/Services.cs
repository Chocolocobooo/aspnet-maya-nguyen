using CoreFitnessClub.Application.DTOs;
using CoreFitnessClub.Application.Interfaces;
using CoreFitnessClub.Domain.Entities;
using CoreFitnessClub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CoreFitnessClub.Application.Services;

// ── MembershipService ─────────────────────────────────────────────────────────

public class MembershipService : IMembershipService
{
    private readonly IMembershipPlanRepository _planRepo;
    private readonly IUserMembershipRepository _userMembershipRepo;

    public MembershipService(
        IMembershipPlanRepository planRepo,
        IUserMembershipRepository userMembershipRepo)
    {
        _planRepo           = planRepo;
        _userMembershipRepo = userMembershipRepo;
    }

    public async Task<IEnumerable<MembershipPlanDto>> GetActivePlansAsync()
    {
        var plans = await _planRepo.GetActiveWithFeaturesAsync();
        return plans.Select(MapToDto);
    }

    public async Task<MembershipPlanDto?> GetPlanByIdAsync(int id)
    {
        var plan = await _planRepo.GetByIdAsync(id);
        return plan is null ? null : MapToDto(plan);
    }

    public async Task JoinPlanAsync(string userId, int planId)
    {
        // Cancel existing membership first
        var existing = await _userMembershipRepo.GetByUserIdAsync(userId);
        if (existing is not null)
        {
            existing.IsActive = false;
            existing.EndDate  = DateTime.UtcNow;
            _userMembershipRepo.Update(existing);
        }

        var membership = new UserMembership
        {
            ApplicationUserId = userId,
            MembershipPlanId  = planId,
            StartDate         = DateTime.UtcNow,
            IsActive          = true
        };
        await _userMembershipRepo.AddAsync(membership);
        await _userMembershipRepo.SaveChangesAsync();
    }

    public async Task CancelMembershipAsync(string userId)
    {
        var membership = await _userMembershipRepo.GetByUserIdAsync(userId);
        if (membership is not null)
        {
            membership.IsActive = false;
            membership.EndDate  = DateTime.UtcNow;
            _userMembershipRepo.Update(membership);
            await _userMembershipRepo.SaveChangesAsync();
        }
    }

    public async Task<MembershipPlanDto?> GetUserCurrentPlanAsync(string userId)
    {
        var membership = await _userMembershipRepo.GetByUserIdAsync(userId);
        if (membership?.MembershipPlan is null) return null;
        return MapToDto(membership.MembershipPlan);
    }

    private static MembershipPlanDto MapToDto(MembershipPlan p) => new()
    {
        Id          = p.Id,
        Name        = p.Name,
        Description = p.Description,
        Price       = p.Price,
        MaxClasses  = p.MaxClasses,
        Features    = p.Features.Select(f => f.Description).ToList()
    };
}

// ── BookingService ────────────────────────────────────────────────────────────

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;

    public BookingService(IBookingRepository bookingRepo)
        => _bookingRepo = bookingRepo;

    public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId)
    {
        var bookings = await _bookingRepo.GetByUserIdAsync(userId);
        return bookings.Select(b => new BookingDto
        {
            Id            = b.Id,
            ClassName     = b.FitnessClass?.Name ?? "Unknown",
            Category      = b.FitnessClass?.Category ?? "",
            ClassDateTime = b.ClassDateTime,
            Status        = b.Status.ToString()
        });
    }

    public async Task<bool> CreateBookingAsync(string userId, CreateBookingDto dto)
    {
        var exists = await _bookingRepo.UserHasBookingAsync(
            userId, dto.FitnessClassId, dto.ClassDateTime);
        if (exists) return false;

        var booking = new Booking
        {
            ApplicationUserId = userId,
            FitnessClassId    = dto.FitnessClassId,
            ClassDateTime     = dto.ClassDateTime,
            BookedAt          = DateTime.UtcNow,
            Status            = BookingStatus.Confirmed
        };
        await _bookingRepo.AddAsync(booking);
        await _bookingRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelBookingAsync(string userId, int bookingId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking is null || booking.ApplicationUserId != userId) return false;

        booking.Status = BookingStatus.Cancelled;
        _bookingRepo.Update(booking);
        await _bookingRepo.SaveChangesAsync();
        return true;
    }
}

// ── UserService ───────────────────────────────────────────────────────────────

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<ApplicationUser?> GetUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId);

    public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        user.FirstName   = dto.FirstName;
        user.LastName    = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteAccountAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
