﻿using Agendify.DTOs.ProviderSchedule;
using Agendify.Models.Entities;
using FluentResults;

namespace Agendify.Services.ProviderSchedules;

public interface IProviderScheduleService
{
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> GetByProviderAsync(int businessId, int providerId);
    Task<Dictionary<DayOfWeek, int>> GetScheduledMinutesByProviderIdsForDateAsync(List<int> providerIds, DateTime date);
    Task<List<ProviderSchedule>> GetSchedulesForDateRangeAsync(List<int> providerIds, DateTime startDate, DateTime endDate);
    Task<Result> CreateDefaultSchedulesAsync(int providerId);
    Task<Result<IEnumerable<ProviderScheduleResponseDto>>> BulkUpdateAsync(int businessId, int providerId, BulkUpdateProviderSchedulesDto dto);
}


