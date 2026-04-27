using CoreFitnessClub.Domain.Entities;

namespace CoreFitnessClub.Domain.Interfaces;

/// <summary>
/// Generic repository contract used by all repositories.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public interface IMembershipPlanRepository : IRepository<MembershipPlan>
{
    Task<IEnumerable<MembershipPlan>> GetActiveWithFeaturesAsync();
}

public interface IUserMembershipRepository : IRepository<UserMembership>
{
    Task<UserMembership?> GetByUserIdAsync(string userId);
}

public interface IFitnessClassRepository : IRepository<FitnessClass>
{
    Task<IEnumerable<FitnessClass>> GetActiveAsync();
}

public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByUserIdAsync(string userId);
    Task<bool> UserHasBookingAsync(string userId, int fitnessClassId, DateTime classDateTime);
}
