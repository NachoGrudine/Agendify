using Agendify.Data;
using Agendify.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agendify.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AgendifyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetByBusinessIdAsync(int businessId)
    {
        return await _dbSet
            .Where(a => a.BusinessId == businessId && !a.IsDeleted)
            .Include(a => a.Provider)
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByProviderIdAsync(int providerId)
    {
        return await _dbSet
            .Where(a => a.ProviderId == providerId && !a.IsDeleted)
            .Include(a => a.Provider)
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(a => a.BusinessId == businessId 
                && !a.IsDeleted 
                && a.StartTime >= startDate 
                && a.StartTime <= endDate)
            .Include(a => a.Provider)
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Appointment> Items, int TotalCount)> GetPagedByDateAsync(
        int businessId, 
        DateTime date, 
        int page, 
        int pageSize)
    {
        var query = _dbSet
            .Where(a => a.BusinessId == businessId 
                && !a.IsDeleted 
                && a.StartTime.Date == date.Date)
            .Include(a => a.Provider)
            .Include(a => a.Customer)
            .Include(a => a.Service);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderBy(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasConflictAsync(
        int providerId, 
        DateTime startTime, 
        DateTime endTime, 
        int? excludeAppointmentId = null)
    {
        var query = _dbSet
            .Where(a => a.ProviderId == providerId 
                && !a.IsDeleted
                && ((a.StartTime < endTime && a.EndTime > startTime)));

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return await query.AnyAsync();
    }
}

