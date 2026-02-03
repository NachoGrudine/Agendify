using Agendify.DTOs.ProviderSchedule;
using Agendify.Models.Entities;
using Agendify.Repositories;
using Agendify.Services.ProviderSchedules;
using Agendify.Services.Providers;
using FluentAssertions;
using FluentResults;
using Moq;

namespace TestAgendify.Services.ProviderSchedules;

public class ProviderScheduleServiceTests
{
    private readonly Mock<IRepository<ProviderSchedule>> _mockScheduleRepository;
    private readonly Mock<IProviderService> _mockProviderService;
    private readonly ProviderScheduleService _sut;

    public ProviderScheduleServiceTests()
    {
        _mockScheduleRepository = new Mock<IRepository<ProviderSchedule>>();
        _mockProviderService = new Mock<IProviderService>();
        _sut = new ProviderScheduleService(_mockScheduleRepository.Object, _mockProviderService.Object);
    }

    #region GetByProviderAsync Tests

    [Fact]
    public async Task GetByProviderAsync_WhenProviderExists_ShouldReturnActiveSchedules()
    {
        // Arrange
        var businessId = 1;
        var providerId = 10;

        _mockProviderService
            .Setup(x => x.GetByIdAsync(businessId, providerId))
            .ReturnsAsync(Result.Ok(new Agendify.DTOs.Provider.ProviderResponseDto()));

        var schedules = new List<ProviderSchedule>
        {
            new()
            {
                Id = 1,
                ProviderId = providerId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                ValidFrom = DateTime.Now.AddMonths(-1),
                ValidUntil = null
            },
            new()
            {
                Id = 2,
                ProviderId = providerId,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                ValidFrom = DateTime.Now.AddMonths(-1),
                ValidUntil = null
            }
        };

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(schedules);

        // Act
        var result = await _sut.GetByProviderAsync(businessId, providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByProviderAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 999;

        _mockProviderService
            .Setup(x => x.GetByIdAsync(businessId, providerId))
            .ReturnsAsync(Result.Fail("Provider not found"));

        // Act
        var result = await _sut.GetByProviderAsync(businessId, providerId);

        // Assert
        result.IsFailed.Should().BeTrue();

        _mockScheduleRepository.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()), 
            Times.Never);
    }

    #endregion

    #region GetScheduledMinutesByProviderIdsForDateAsync Tests

    [Fact]
    public async Task GetScheduledMinutesByProviderIdsForDateAsync_ShouldReturnMinutesByDayOfWeek()
    {
        // Arrange
        var providerIds = new List<int> { 1, 2 };
        var date = new DateTime(2026, 2, 3);

        var schedules = new List<ProviderSchedule>
        {
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17), // 480 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            },
            new()
            {
                ProviderId = 2,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(14), // 240 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            },
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(13), // 240 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            }
        };

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(schedules);

        // Act
        var result = await _sut.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date);

        // Assert
        result.Should().ContainKey(DayOfWeek.Monday);
        result[DayOfWeek.Monday].Should().Be(720); // 480 + 240
        result.Should().ContainKey(DayOfWeek.Tuesday);
        result[DayOfWeek.Tuesday].Should().Be(240);
    }

    [Fact]
    public async Task GetScheduledMinutesByProviderIdsForDateAsync_WhenNoSchedules_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var providerIds = new List<int> { 1 };
        var date = new DateTime(2026, 2, 3);

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(new List<ProviderSchedule>());

        // Act
        var result = await _sut.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetScheduledMinutesByProviderIdsForDateAsync_ShouldOnlyIncludeValidSchedulesForDate()
    {
        // Arrange
        var providerIds = new List<int> { 1 };
        var date = new DateTime(2026, 2, 3);

        var schedules = new List<ProviderSchedule>
        {
            // Schedule válido en la fecha
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17), // 480 min
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            },
            // Schedule expirado antes de la fecha - NO debe incluirse
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(14), // 240 min
                ValidFrom = new DateTime(2025, 1, 1),
                ValidUntil = new DateTime(2026, 1, 31)
            }
        };

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(schedules.Where(s => 
                s.ValidFrom.Date <= date.Date && 
                (s.ValidUntil == null || s.ValidUntil.Value.Date >= date.Date)));

        // Act
        var result = await _sut.GetScheduledMinutesByProviderIdsForDateAsync(providerIds, date);

        // Assert
        result[DayOfWeek.Monday].Should().Be(480); // Solo el válido
    }

    #endregion

    #region GetSchedulesForDateRangeAsync Tests

    [Fact]
    public async Task GetSchedulesForDateRangeAsync_ShouldReturnAllSchedulesInRange()
    {
        // Arrange
        var providerIds = new List<int> { 1, 2 };
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 28);

        var schedules = new List<ProviderSchedule>
        {
            new()
            {
                ProviderId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                ValidFrom = new DateTime(2026, 1, 1),
                ValidUntil = null
            },
            new()
            {
                ProviderId = 2,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(16),
                ValidFrom = new DateTime(2026, 1, 15),
                ValidUntil = new DateTime(2026, 2, 15)
            }
        };

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(schedules);

        // Act
        var result = await _sut.GetSchedulesForDateRangeAsync(providerIds, startDate, endDate);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSchedulesForDateRangeAsync_WhenNoSchedules_ShouldReturnEmptyList()
    {
        // Arrange
        var providerIds = new List<int> { 1 };
        var startDate = new DateTime(2026, 2, 1);
        var endDate = new DateTime(2026, 2, 28);

        _mockScheduleRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(new List<ProviderSchedule>());

        // Act
        var result = await _sut.GetSchedulesForDateRangeAsync(providerIds, startDate, endDate);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateDefaultSchedulesAsync Tests

    [Fact]
    public async Task CreateDefaultSchedulesAsync_ShouldCreateFiveDefaultSchedules()
    {
        // Arrange
        var providerId = 10;
        List<ProviderSchedule>? capturedSchedules = null;

        _mockScheduleRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProviderSchedule>>()))
            .Callback<IEnumerable<ProviderSchedule>>(schedules => capturedSchedules = schedules.ToList())
            .ReturnsAsync((IEnumerable<ProviderSchedule> schedules) => schedules);

        // Act
        var result = await _sut.CreateDefaultSchedulesAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockScheduleRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProviderSchedule>>()), Times.Once);

        capturedSchedules.Should().NotBeNull();
        capturedSchedules.Should().HaveCount(5); // Lun-Vie
        capturedSchedules.Should().AllSatisfy(s =>
        {
            s.ProviderId.Should().Be(providerId);
            s.StartTime.Should().Be(TimeSpan.FromHours(9));
            s.EndTime.Should().Be(TimeSpan.FromHours(18));
            s.ValidUntil.Should().BeNull();
        });

        // Verificar que sean Lun-Vie
        capturedSchedules!.Select(s => s.DayOfWeek).Should().BeEquivalentTo(new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        });
    }

    #endregion

    #region BulkUpdateAsync Tests

    [Fact]
    public async Task BulkUpdateAsync_WhenProviderExists_ShouldUpdateSchedules()
    {
        // Arrange
        var businessId = 1;
        var providerId = 10;

        _mockProviderService
            .Setup(x => x.GetByIdAsync(businessId, providerId))
            .ReturnsAsync(Result.Ok(new Agendify.DTOs.Provider.ProviderResponseDto()));

        var existingSchedules = new List<ProviderSchedule>
        {
            new()
            {
                Id = 1,
                ProviderId = providerId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
                ValidUntil = null
            }
        };

        var newSchedulesDto = new BulkUpdateProviderSchedulesDto
        {
            Schedules = new List<ProviderScheduleItemDto>
            {
                new()
                {
                    DayOfWeek = DayOfWeek.Monday,
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(18)
                }
            }
        };

        var updatedSchedules = new List<ProviderSchedule>
        {
            new()
            {
                Id = 2,
                ProviderId = providerId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(18),
                ValidUntil = null
            }
        };

        _mockScheduleRepository
            .SetupSequence(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()))
            .ReturnsAsync(existingSchedules)
            .ReturnsAsync(updatedSchedules);

        _mockScheduleRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProviderSchedule>()))
            .ReturnsAsync((ProviderSchedule schedule) => schedule);

        _mockScheduleRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProviderSchedule>>()))
            .ReturnsAsync((IEnumerable<ProviderSchedule> schedules) => schedules);

        // Act
        var result = await _sut.BulkUpdateAsync(businessId, providerId, newSchedulesDto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task BulkUpdateAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var businessId = 1;
        var providerId = 999;
        var dto = new BulkUpdateProviderSchedulesDto { Schedules = new List<ProviderScheduleItemDto>() };

        _mockProviderService
            .Setup(x => x.GetByIdAsync(businessId, providerId))
            .ReturnsAsync(Result.Fail("Provider not found"));

        // Act
        var result = await _sut.BulkUpdateAsync(businessId, providerId, dto);

        // Assert
        result.IsFailed.Should().BeTrue();

        _mockScheduleRepository.Verify(
            x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProviderSchedule, bool>>>()), 
            Times.Never);
    }

    #endregion
}

