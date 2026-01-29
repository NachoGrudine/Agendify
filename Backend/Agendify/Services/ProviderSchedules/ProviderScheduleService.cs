using Agendify.Models.Entities;
using Agendify.DTOs.ProviderSchedule;
using Agendify.Repositories;
using Agendify.Services.Providers;
using FluentResults;

namespace Agendify.Services.ProviderSchedules;

public class ProviderScheduleService : IProviderScheduleService
{
    private readonly IRepository<ProviderSchedule> _scheduleRepository;
    private readonly IProviderService _providerService;

    public ProviderScheduleService(
        IRepository<ProviderSchedule> scheduleRepository,
        IProviderService providerService)
    {
        _scheduleRepository = scheduleRepository;
        _providerService = providerService;
    }
    
    /// <summary>
    /// Obtiene los schedules vigentes (actuales) de un provider específico
    /// </summary>
    /// <returns>Lista de schedules con ValidUntil = NULL (solo los que están activos actualmente)</returns>
    public async Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByProviderAsync(int businessId, int providerId)
    {
        var providerResult = await _providerService.GetByIdAsync(businessId, providerId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        var schedules = await _scheduleRepository.FindAsync(s => 
            s.ProviderId == providerId && s.ValidUntil == null);
        
        return Result.Ok(schedules.Select(MapToResponseDto));
    }

    /// <summary>
    /// Obtiene minutos programados por día de semana para UNA FECHA ESPECÍFICA usando versionado temporal
    /// </summary>
    /// <param name="providerIds">Lista de IDs de providers</param>
    /// <param name="date">Fecha específica para consultar schedules históricos</param>
    /// <returns>Dictionary con minutos por día de semana (ej: {Monday: 540, Tuesday: 540})</returns>
    /// <remarks>Usa ValidFrom/ValidUntil para obtener schedules que eran válidos en esa fecha</remarks>
    public async Task<Dictionary<DayOfWeek, int>> GetScheduledMinutesByProviderIdsForDateAsync(
        List<int> providerIds, 
        DateTime date)
    {
        // Normalizar la fecha a medianoche para comparaciones correctas
        var dateOnly = date.Date;

        var schedules = await _scheduleRepository.FindAsync(s =>
            providerIds.Contains(s.ProviderId) &&
            s.ValidFrom.Date <= dateOnly &&
            (s.ValidUntil == null || s.ValidUntil.Value.Date >= dateOnly));

        return CalculateMinutesByDayOfWeek(schedules);
    }

    /// <summary>
    /// Obtiene TODOS los schedules que intersectan con un rango de fechas (1 query optimizada)
    /// </summary>
    /// <param name="providerIds">Lista de IDs de providers</param>
    /// <param name="startDate">Fecha inicio del rango</param>
    /// <param name="endDate">Fecha fin del rango</param>
    /// <returns>Lista completa de schedules (incluye históricos y vigentes) para filtrar en memoria</returns>
    /// <remarks>Retorna schedules donde ValidFrom menor igual endDate AND (ValidUntil IS NULL OR ValidUntil mayor igual startDate)</remarks>
    public async Task<List<ProviderSchedule>> GetSchedulesForDateRangeAsync(
        List<int> providerIds,
        DateTime startDate,
        DateTime endDate)
    {
        // Normalizar las fechas a medianoche para comparaciones correctas
        var startDateOnly = startDate.Date;
        var endDateOnly = endDate.Date;

        var schedules = await _scheduleRepository.FindAsync(s =>
            providerIds.Contains(s.ProviderId) &&
            s.ValidFrom.Date <= endDateOnly &&
            (s.ValidUntil == null || s.ValidUntil.Value.Date >= startDateOnly));

        return schedules.ToList();
    }
    
    /// <summary>
    /// Crea schedules por defecto para un provider nuevo (Lun-Vie 9-18)
    /// </summary>
    /// <param name="providerId">ID del provider</param>
    /// <returns>Result Ok si se crearon correctamente</returns>
    /// <remarks>Inserta 5 schedules en bulk con ValidFrom = hoy, ValidUntil = NULL</remarks>
    public async Task<Result> CreateDefaultSchedulesAsync(int providerId)
    {
        var defaultSchedules = CreateDefaultSchedulesList(providerId);
        await _scheduleRepository.AddRangeAsync(defaultSchedules);
        return Result.Ok();
    }

    /// <summary>
    /// Actualiza schedules de un provider usando estrategia granular (cierra viejos, inserta nuevos)
    /// </summary>
    /// <param name="businessId">ID del business</param>
    /// <param name="providerId">ID del provider</param>
    /// <param name="dto">Lista de schedules nuevos a aplicar</param>
    /// <returns>Lista de schedules vigentes después del update</returns>
    /// <remarks>
    /// NO elimina schedules, los cierra (ValidUntil = ayer) para mantener historial.
    /// Usa comparación granular: solo modifica lo que cambió.
    /// Operaciones bulk para mejor performance.
    /// </remarks>
    public async Task<Result<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateAsync(
        int businessId, 
        int providerId, 
        BulkUpdateProviderSchedulesDto dto)
    {
        var providerResult = await _providerService.GetByIdAsync(businessId, providerId);
        if (providerResult.IsFailed)
        {
            return Result.Fail(providerResult.Errors);
        }

        var existingSchedules = await _scheduleRepository.FindAsync(s => 
            s.ProviderId == providerId && s.ValidUntil == null);
        
        var (toClose, toInsert) = IdentifyChanges(existingSchedules.ToList(), dto.Schedules, providerId);

        await CloseOldSchedules(toClose);
        await InsertNewSchedules(toInsert);

        var finalSchedules = await _scheduleRepository.FindAsync(s => 
            s.ProviderId == providerId && s.ValidUntil == null);
        
        return Result.Ok(finalSchedules.Select(MapToResponseDto));
    }
    
    /// <summary>
    /// Calcula minutos programados por día de semana
    /// </summary>
    private static Dictionary<DayOfWeek, int> CalculateMinutesByDayOfWeek(IEnumerable<ProviderSchedule> schedules)
    {
        var minutesByDayOfWeek = new Dictionary<DayOfWeek, int>();

        foreach (var schedule in schedules)
        {
            var minutes = (int)(schedule.EndTime - schedule.StartTime).TotalMinutes;

            if (!minutesByDayOfWeek.ContainsKey(schedule.DayOfWeek))
            {
                minutesByDayOfWeek[schedule.DayOfWeek] = 0;
            }

            minutesByDayOfWeek[schedule.DayOfWeek] += minutes;
        }

        return minutesByDayOfWeek;
    }

    /// <summary>
    /// Crea lista de schedules por defecto (Lun-Vie 9-18)
    /// </summary>
    private static List<ProviderSchedule> CreateDefaultSchedulesList(int providerId)
    {
        var defaultSchedules = new List<ProviderSchedule>();
        var now = DateTime.Now; // Usar hora local para consistencia

        for (int day = (int)DayOfWeek.Monday; day <= (int)DayOfWeek.Friday; day++)
        {
            defaultSchedules.Add(new ProviderSchedule
            {
                ProviderId = providerId,
                DayOfWeek = (DayOfWeek)day,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(18, 0, 0),
                ValidFrom = now,
                ValidUntil = null
            });
        }

        return defaultSchedules;
    }

    /// <summary>
    /// Cierra schedules viejos (setea ValidUntil a ayer)
    /// </summary>
    private async Task CloseOldSchedules(List<ProviderSchedule> schedulesToClose)
    {
        if (!schedulesToClose.Any())
            return;

        var yesterday = DateTime.Now.Date.AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59); // Usar hora local

        foreach (var schedule in schedulesToClose)
        {
            schedule.ValidUntil = yesterday;
            await _scheduleRepository.UpdateAsync(schedule);
        }
    }

    /// <summary>
    /// Inserta nuevos schedules con validez desde hoy
    /// </summary>
    private async Task InsertNewSchedules(List<ProviderSchedule> schedulesToInsert)
    {
        if (!schedulesToInsert.Any())
            return;

        var now = DateTime.Now; // Usar hora local

        foreach (var schedule in schedulesToInsert)
        {
            schedule.ValidFrom = now.Date;
            schedule.ValidUntil = null;
        }

        await _scheduleRepository.AddRangeAsync(schedulesToInsert);
    }

    /// <summary>
    /// Identifica qué schedules cerrar y cuáles insertar (comparación granular)
    /// </summary>
    private static (List<ProviderSchedule> ToClose, List<ProviderSchedule> ToInsert) IdentifyChanges(
        List<ProviderSchedule> existing,
        List<ProviderScheduleItemDto> incoming,
        int providerId)
    {
        var incomingSet = incoming
            .Select(s => new ScheduleKey(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToHashSet();

        var existingSet = existing
            .Select(s => new ScheduleKey(s.DayOfWeek, s.StartTime, s.EndTime))
            .ToHashSet();

        var toClose = existing
            .Where(e => !incomingSet.Contains(new ScheduleKey(e.DayOfWeek, e.StartTime, e.EndTime)))
            .ToList();

        var toInsert = incoming
            .Where(i => !existingSet.Contains(new ScheduleKey(i.DayOfWeek, i.StartTime, i.EndTime)))
            .Select(s => new ProviderSchedule
            {
                ProviderId = providerId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            })
            .ToList();

        return (toClose, toInsert);
    }

    private static ProviderScheduleResponseDto MapToResponseDto(ProviderSchedule schedule)
    {
        return new ProviderScheduleResponseDto
        {
            Id = schedule.Id,
            ProviderId = schedule.ProviderId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime
        };
    }

    private record ScheduleKey(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime);
}

