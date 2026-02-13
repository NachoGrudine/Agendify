using Agendify.Data;
using Agendify.Models.Entities;
using Agendify.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TestAgendify.Repositories;

public class AppointmentRepositoryTests : IDisposable
{
    private readonly AgendifyDbContext _context;
    private readonly AppointmentRepository _repository;

    public AppointmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AgendifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AgendifyDbContext(options);
        _repository = new AppointmentRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_Should_Return_Appointment_With_Navigation_Properties()
    {
        // Arrange
        var provider = new Provider { Name = "John", Specialty = "Barber", BusinessId = 1 };
        var customer = new Customer { Name = "Jane Doe", Email = "jane@test.com", Phone = "123456", BusinessId = 1 };
        var service = new Service { Name = "Haircut", DefaultDuration = 30, Price = 20, BusinessId = 1 };
        
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        var appointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            CustomerId = customer.Id,
            ServiceId = service.Id,
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(1.5)
        };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(appointment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.Provider.Should().NotBeNull();
        result.Provider!.Name.Should().Be("John");
        result.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Jane Doe");
        result.Service.Should().NotBeNull();
        result.Service!.Name.Should().Be("Haircut");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByBusinessIdAsync Tests

    [Fact]
    public async Task GetByBusinessIdAsync_Should_Return_All_Appointments_For_Business()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now.AddHours(1), EndTime = DateTime.Now.AddHours(1.5) },
            new Appointment { BusinessId = 2, ProviderId = provider.Id, StartTime = DateTime.Now.AddHours(2), EndTime = DateTime.Now.AddHours(2.5) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBusinessIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.BusinessId == 1);
    }

    [Fact]
    public async Task GetByBusinessIdAsync_Should_Exclude_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now.AddHours(1), EndTime = DateTime.Now.AddHours(1.5), IsDeleted = true }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBusinessIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(a => !a.IsDeleted);
    }

    [Fact]
    public async Task GetByBusinessIdAsync_Should_Include_Navigation_Properties()
    {
        // Arrange
        var provider = new Provider { Name = "John", Specialty = "Barber", BusinessId = 1 };
        var customer = new Customer { Name = "Jane", Email = "jane@test.com", Phone = "123", BusinessId = 1 };
        var service = new Service { Name = "Haircut", DefaultDuration = 30, Price = 20, BusinessId = 1 };
        
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        var appointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            CustomerId = customer.Id,
            ServiceId = service.Id,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddMinutes(30) };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBusinessIdAsync(1);

        // Assert
        result.Should().HaveCount(1);
        var first = result.First();
        first.Provider.Should().NotBeNull();
        first.Customer.Should().NotBeNull();
        first.Service.Should().NotBeNull();
    }

    #endregion

    #region GetByProviderIdAsync Tests

    [Fact]
    public async Task GetByProviderIdAsync_Should_Return_All_Appointments_For_Provider()
    {
        // Arrange
        var provider1 = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var provider2 = new Provider { Name = "Provider 2", Specialty = "Test", BusinessId = 1 };
        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider1.Id, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider1.Id, StartTime = DateTime.Now.AddHours(1), EndTime = DateTime.Now.AddHours(1.5) },
            new Appointment { BusinessId = 1, ProviderId = provider2.Id, StartTime = DateTime.Now.AddHours(2), EndTime = DateTime.Now.AddHours(2.5) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderIdAsync(provider1.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.ProviderId == provider1.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_Should_Exclude_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = DateTime.Now.AddHours(1), EndTime = DateTime.Now.AddHours(1.5), IsDeleted = true }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProviderIdAsync(provider.Id);

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(a => !a.IsDeleted);
    }

    #endregion

    #region GetByDateRangeAsync Tests

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_Appointments_Within_Date_Range()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseDate = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate, EndTime = baseDate.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddDays(1), EndTime = baseDate.AddDays(1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddDays(5), EndTime = baseDate.AddDays(5).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(1, baseDate, baseDate.AddDays(2));

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.StartTime >= baseDate && a.StartTime <= baseDate.AddDays(2));
    }

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_Appointments_Ordered_By_StartTime()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseDate = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddHours(3), EndTime = baseDate.AddHours(3).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddHours(1), EndTime = baseDate.AddHours(1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddHours(2), EndTime = baseDate.AddHours(2).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetByDateRangeAsync(1, baseDate, baseDate.AddDays(1))).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].StartTime.Should().Be(baseDate.AddHours(1));
        result[1].StartTime.Should().Be(baseDate.AddHours(2));
        result[2].StartTime.Should().Be(baseDate.AddHours(3));
    }

    [Fact]
    public async Task GetByDateRangeAsync_Should_Exclude_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseDate = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate, EndTime = baseDate.AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = baseDate.AddHours(1), EndTime = baseDate.AddHours(1).AddMinutes(30), IsDeleted = true }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDateRangeAsync(1, baseDate, baseDate.AddDays(1));

        // Assert
        result.Should().HaveCount(1);
        result.Should().OnlyContain(a => !a.IsDeleted);
    }

    #endregion

    #region GetPagedByDateAsync Tests

    [Fact]
    public async Task GetPagedByDateAsync_Should_Return_Correct_Page_And_Total_Count()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2026, 2, 15);
        var appointments = new List<Appointment>();
        for (int i = 0; i < 10; i++)
        {
            appointments.Add(new Appointment
            {
                BusinessId = 1,
                ProviderId = provider.Id,
                StartTime = targetDate.AddHours(i),
                EndTime = targetDate.AddHours(i).AddMinutes(30) });
        }
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedByDateAsync(1, targetDate, page: 1, pageSize: 3);

        // Assert
        result.TotalCount.Should().Be(10);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetPagedByDateAsync_Should_Return_Correct_Second_Page()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2026, 2, 15);
        var appointments = new List<Appointment>();
        for (int i = 0; i < 10; i++)
        {
            appointments.Add(new Appointment
            {
                BusinessId = 1,
                ProviderId = provider.Id,
                StartTime = targetDate.AddHours(i),
                EndTime = targetDate.AddHours(i).AddMinutes(30) });
        }
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedByDateAsync(1, targetDate, page: 2, pageSize: 3);

        // Assert
        result.Items.Should().HaveCount(3);
        var items = result.Items.ToList();
        items[0].StartTime.Hour.Should().Be(3); // Skip 0,1,2 -> get 3,4,5
    }

    [Fact]
    public async Task GetPagedByDateAsync_Should_Only_Return_Appointments_For_Specific_Date()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2026, 2, 15);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = targetDate.AddHours(10), EndTime = targetDate.AddHours(10).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = targetDate.AddDays(1), EndTime = targetDate.AddDays(1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = targetDate.AddDays(-1), EndTime = targetDate.AddDays(-1).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedByDateAsync(1, targetDate, page: 1, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items.First().StartTime.Date.Should().Be(targetDate.Date);
    }

    [Fact]
    public async Task GetPagedByDateAsync_Should_Exclude_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2026, 2, 15);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = targetDate.AddHours(10), EndTime = targetDate.AddHours(10).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, StartTime = targetDate.AddHours(11), EndTime = targetDate.AddHours(11).AddMinutes(30), IsDeleted = true }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedByDateAsync(1, targetDate, page: 1, pageSize: 10);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(a => !a.IsDeleted);
    }

    #endregion

    #region HasConflictAsync Tests

    [Fact]
    public async Task HasConflictAsync_Should_Return_True_When_Appointments_Overlap()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var existingAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30) };
        _context.Appointments.Add(existingAppointment);
        await _context.SaveChangesAsync();

        // Act - New appointment overlaps with existing one
        var result = await _repository.HasConflictAsync(
            provider.Id, 
            baseTime.AddMinutes(15), 
            baseTime.AddMinutes(45));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_False_When_No_Overlap()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var existingAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30) };
        _context.Appointments.Add(existingAppointment);
        await _context.SaveChangesAsync();

        // Act - New appointment starts after existing one ends
        var result = await _repository.HasConflictAsync(
            provider.Id, 
            baseTime.AddMinutes(30), 
            baseTime.AddMinutes(60));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_Should_Exclude_Specific_Appointment_When_Updating()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var existingAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30) };
        _context.Appointments.Add(existingAppointment);
        await _context.SaveChangesAsync();

        // Act - Check same time slot but excluding the existing appointment (simulate update)
        var result = await _repository.HasConflictAsync(
            provider.Id, 
            baseTime, 
            baseTime.AddMinutes(30),
            excludeAppointmentId: existingAppointment.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_Should_Ignore_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var deletedAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30),
                        IsDeleted = true
        };
        _context.Appointments.Add(deletedAppointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HasConflictAsync(
            provider.Id, 
            baseTime, 
            baseTime.AddMinutes(30));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasConflictAsync_Should_Return_True_When_New_Appointment_Contains_Existing()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var existingAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = baseTime.AddMinutes(15),
            EndTime = baseTime.AddMinutes(30) };
        _context.Appointments.Add(existingAppointment);
        await _context.SaveChangesAsync();

        // Act - New appointment completely contains existing one
        var result = await _repository.HasConflictAsync(
            provider.Id, 
            baseTime, 
            baseTime.AddMinutes(45));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasConflictAsync_Should_Not_Conflict_With_Different_Provider()
    {
        // Arrange
        var provider1 = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var provider2 = new Provider { Name = "Provider 2", Specialty = "Test", BusinessId = 1 };
        _context.Providers.AddRange(provider1, provider2);
        await _context.SaveChangesAsync();

        var baseTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var existingAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider1.Id,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30) };
        _context.Appointments.Add(existingAppointment);
        await _context.SaveChangesAsync();

        // Act - Same time but different provider
        var result = await _repository.HasConflictAsync(
            provider2.Id, 
            baseTime, 
            baseTime.AddMinutes(30));

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetNextAppointmentAsync Tests

    [Fact]
    public async Task GetNextAppointmentAsync_Should_Return_Next_Appointment_After_Current_Time()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var customer = new Customer { Name = "John Doe", Email = "john@test.com", Phone = "123", BusinessId = 1 };
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var currentTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(-1), EndTime = currentTime.AddHours(-1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(1), EndTime = currentTime.AddHours(1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(2), EndTime = currentTime.AddHours(2).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNextAppointmentAsync(1, currentTime);

        // Assert
        result.Should().NotBeNull();
        result!.StartTime.Should().Be(currentTime.AddHours(1));
    }

    [Fact]
    public async Task GetNextAppointmentAsync_Should_Return_Null_When_No_Future_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var currentTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var pastAppointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            StartTime = currentTime.AddHours(-1),
            EndTime = currentTime.AddHours(-1).AddMinutes(30) };
        _context.Appointments.Add(pastAppointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNextAppointmentAsync(1, currentTime);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNextAppointmentAsync_Should_Include_Customer_Navigation_Property()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var customer = new Customer { Name = "Jane Doe", Email = "jane@test.com", Phone = "123", BusinessId = 1 };
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var currentTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointment = new Appointment
        {
            BusinessId = 1,
            ProviderId = provider.Id,
            CustomerId = customer.Id,
            StartTime = currentTime.AddHours(1),
            EndTime = currentTime.AddHours(1).AddMinutes(30) };
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNextAppointmentAsync(1, currentTime);

        // Assert
        result.Should().NotBeNull();
        result!.Customer.Should().NotBeNull();
        result.Customer!.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetNextAppointmentAsync_Should_Exclude_Deleted_Appointments()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var customer = new Customer { Name = "John", Email = "john@test.com", Phone = "123", BusinessId = 1 };
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var currentTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(1), EndTime = currentTime.AddHours(1).AddMinutes(30), IsDeleted = true },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(2), EndTime = currentTime.AddHours(2).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNextAppointmentAsync(1, currentTime);

        // Assert
        result.Should().NotBeNull();
        result!.StartTime.Should().Be(currentTime.AddHours(2)); // Should skip the deleted one
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetNextAppointmentAsync_Should_Only_Return_Appointments_For_Specific_Business()
    {
        // Arrange
        var provider = new Provider { Name = "Provider 1", Specialty = "Test", BusinessId = 1 };
        var customer = new Customer { Name = "John", Email = "john@test.com", Phone = "123", BusinessId = 1 };
        _context.Providers.Add(provider);
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var currentTime = new DateTime(2026, 2, 15, 10, 0, 0);
        var appointments = new List<Appointment>
        {
            new Appointment { BusinessId = 2, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(1), EndTime = currentTime.AddHours(1).AddMinutes(30) },
            new Appointment { BusinessId = 1, ProviderId = provider.Id, CustomerId = customer.Id, StartTime = currentTime.AddHours(2), EndTime = currentTime.AddHours(2).AddMinutes(30) }
        };
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNextAppointmentAsync(1, currentTime);

        // Assert
        result.Should().NotBeNull();
        result!.BusinessId.Should().Be(1);
        result.StartTime.Should().Be(currentTime.AddHours(2));
    }

    #endregion
}

