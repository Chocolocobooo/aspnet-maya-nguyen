using CoreFitnessClub.Domain.Entities;
using CoreFitnessClub.Domain.Interfaces;
using CoreFitnessClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreFitnessClub.Infrastructure.Repositories;

// ── Generic base ──────────────────────────────────────────────────────────────

public abstract class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected Repository(AppDbContext db) => _db = db;

    public async Task<T?> GetByIdAsync(int id)        => await _db.Set<T>().FindAsync(id);
    public async Task<IEnumerable<T>> GetAllAsync()   => await _db.Set<T>().ToListAsync();
    public async Task AddAsync(T entity)              => await _db.Set<T>().AddAsync(entity);
    public void Update(T entity)                      => _db.Set<T>().Update(entity);
    public void Delete(T entity)                      => _db.Set<T>().Remove(entity);
    public async Task SaveChangesAsync()              => await _db.SaveChangesAsync();
}

// ── MembershipPlanRepository ──────────────────────────────────────────────────

public class MembershipPlanRepository : Repository<MembershipPlan>, IMembershipPlanRepository
{
    public MembershipPlanRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<MembershipPlan>> GetActiveWithFeaturesAsync()
        => await _db.MembershipPlans
                    .Include(p => p.Features)
                    .Where(p => p.IsActive)
                    .ToListAsync();
}

// ── UserMembershipRepository ──────────────────────────────────────────────────

public class UserMembershipRepository : Repository<UserMembership>, IUserMembershipRepository
{
    public UserMembershipRepository(AppDbContext db) : base(db) { }

    public async Task<UserMembership?> GetByUserIdAsync(string userId)
        => await _db.UserMemberships
                    .Include(m => m.MembershipPlan)
                        .ThenInclude(p => p!.Features)
                    .FirstOrDefaultAsync(m => m.ApplicationUserId == userId && m.IsActive);
}

// ── FitnessClassRepository ────────────────────────────────────────────────────

public class FitnessClassRepository : Repository<FitnessClass>, IFitnessClassRepository
{
    public FitnessClassRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<FitnessClass>> GetActiveAsync()
        => await _db.FitnessClasses.Where(c => c.IsActive).ToListAsync();
}

// ── BookingRepository ─────────────────────────────────────────────────────────

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext db) : base(db) { }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(string userId)
        => await _db.Bookings
                    .Include(b => b.FitnessClass)
                    .Where(b => b.ApplicationUserId == userId)
                    .OrderByDescending(b => b.ClassDateTime)
                    .ToListAsync();

    public async Task<bool> UserHasBookingAsync(string userId, int fitnessClassId, DateTime classDateTime)
        => await _db.Bookings.AnyAsync(b =>
               b.ApplicationUserId == userId &&
               b.FitnessClassId    == fitnessClassId &&
               b.ClassDateTime     == classDateTime &&
               b.Status            != BookingStatus.Cancelled);
}
