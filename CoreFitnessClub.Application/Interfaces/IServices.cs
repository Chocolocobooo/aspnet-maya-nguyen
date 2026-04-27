using CoreFitnessClub.Application.DTOs;
using CoreFitnessClub.Domain.Entities;

namespace CoreFitnessClub.Application.Interfaces;

public interface IMembershipService
{
    Task<IEnumerable<MembershipPlanDto>> GetActivePlansAsync();
    Task<MembershipPlanDto?> GetPlanByIdAsync(int id);
    Task JoinPlanAsync(string userId, int planId);
    Task CancelMembershipAsync(string userId);
    Task<MembershipPlanDto?> GetUserCurrentPlanAsync(string userId);
}

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(string userId);
    Task<bool> CreateBookingAsync(string userId, CreateBookingDto dto);
    Task<bool> CancelBookingAsync(string userId, int bookingId);
}

public interface IUserService
{
    Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<bool> DeleteAccountAsync(string userId);
    Task<ApplicationUser?> GetUserAsync(string userId);
}
