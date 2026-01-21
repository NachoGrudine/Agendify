using Agendify.API.Domain.Entities;

namespace Agendify.API.Infrastructure.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByBusinessIdAsync(int businessId);
    Task<IEnumerable<Appointment>> GetByProviderIdAsync(int providerId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(int businessId, DateTime startDate, DateTime endDate);
    Task<(IEnumerable<Appointment> Items, int TotalCount)> GetPagedByDateAsync(int businessId, DateTime date, int page, int pageSize);
    Task<bool> HasConflictAsync(int providerId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
}

