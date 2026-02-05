using Agendify.Data;
using Agendify.Models.Entities;
using Agendify.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TestAgendify.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly AgendifyDbContext _context;
    private readonly Repository<Provider> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AgendifyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AgendifyDbContext(options);
        _repository = new Repository<Provider>(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "Test Provider",
            Specialty = "Test Specialty",
            BusinessId = 1,
            IsActive = true
        };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
        result.Name.Should().Be(provider.Name);
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

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Entities()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "Provider 1", Specialty = "Specialty 1", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 2", Specialty = "Specialty 2", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 3", Specialty = "Specialty 3", BusinessId = 1, IsActive = true }
        };
        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Name == "Provider 1");
        result.Should().Contain(p => p.Name == "Provider 2");
        result.Should().Contain(p => p.Name == "Provider 3");
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_List_When_No_Entities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region FindAsync Tests

    [Fact]
    public async Task FindAsync_Should_Return_Matching_Entities()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "John", Specialty = "Barber", BusinessId = 1, IsActive = true },
            new Provider { Name = "Jane", Specialty = "Stylist", BusinessId = 1, IsActive = true },
            new Provider { Name = "Bob", Specialty = "Barber", BusinessId = 1, IsActive = false }
        };
        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(p => p.Specialty == "Barber");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "John");
        result.Should().Contain(p => p.Name == "Bob");
    }

    [Fact]
    public async Task FindAsync_Should_Return_Empty_When_No_Match()
    {
        // Arrange
        var provider = new Provider { Name = "Test", Specialty = "Barber", BusinessId = 1, IsActive = true };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(p => p.Specialty == "NonExistent");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_Should_Add_Entity_To_Database()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "New Provider",
            Specialty = "New Specialty",
            BusinessId = 1,
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(provider);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        var savedProvider = await _context.Providers.FindAsync(result.Id);
        savedProvider.Should().NotBeNull();
        savedProvider!.Name.Should().Be(provider.Name);
    }

    [Fact]
    public async Task AddAsync_Should_Return_Entity_With_Generated_Id()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "Test Provider",
            Specialty = "Test",
            BusinessId = 1,
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(provider);

        // Assert
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region AddRangeAsync Tests

    [Fact]
    public async Task AddRangeAsync_Should_Add_Multiple_Entities()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "Provider 1", Specialty = "Spec 1", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 2", Specialty = "Spec 2", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 3", Specialty = "Spec 3", BusinessId = 1, IsActive = true }
        };

        // Act
        var result = await _repository.AddRangeAsync(providers);

        // Assert
        result.Should().HaveCount(3);
        var allProviders = await _context.Providers.ToListAsync();
        allProviders.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddRangeAsync_Should_Assign_Ids_To_All_Entities()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "Provider 1", Specialty = "Spec 1", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 2", Specialty = "Spec 2", BusinessId = 1, IsActive = true }
        };

        // Act
        var result = await _repository.AddRangeAsync(providers);

        // Assert
        result.Should().OnlyContain(p => p.Id > 0);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_Should_Update_Entity()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "Original Name",
            Specialty = "Original Specialty",
            BusinessId = 1,
            IsActive = true
        };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        provider.Name = "Updated Name";
        provider.Specialty = "Updated Specialty";
        var result = await _repository.UpdateAsync(provider);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Specialty.Should().Be("Updated Specialty");
        
        var updatedProvider = await _context.Providers.FindAsync(provider.Id);
        updatedProvider!.Name.Should().Be("Updated Name");
        updatedProvider.Specialty.Should().Be("Updated Specialty");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_Should_Remove_Entity()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "To Delete",
            Specialty = "Test",
            BusinessId = 1,
            IsActive = true
        };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        var providerId = provider.Id;

        // Act
        await _repository.DeleteAsync(provider);

        // Assert
        var deletedProvider = await _context.Providers.FindAsync(providerId);
        deletedProvider.Should().BeNull();
    }

    #endregion

    #region DeleteRangeAsync Tests

    [Fact]
    public async Task DeleteRangeAsync_Should_Remove_Multiple_Entities()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "Provider 1", Specialty = "Spec 1", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 2", Specialty = "Spec 2", BusinessId = 1, IsActive = true }
        };
        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteRangeAsync(providers);

        // Assert
        var remainingProviders = await _context.Providers.ToListAsync();
        remainingProviders.Should().BeEmpty();
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Entity_Exists()
    {
        // Arrange
        var provider = new Provider
        {
            Name = "Test Provider",
            Specialty = "Test",
            BusinessId = 1,
            IsActive = true
        };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(p => p.Name == "Test Provider");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_False_When_Entity_Does_Not_Exist()
    {
        // Act
        var result = await _repository.ExistsAsync(p => p.Name == "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CountAsync Tests

    [Fact]
    public async Task CountAsync_Should_Return_Correct_Count()
    {
        // Arrange
        var providers = new List<Provider>
        {
            new Provider { Name = "Provider 1", Specialty = "Barber", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 2", Specialty = "Stylist", BusinessId = 1, IsActive = true },
            new Provider { Name = "Provider 3", Specialty = "Barber", BusinessId = 1, IsActive = true }
        };
        _context.Providers.AddRange(providers);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(p => p.Specialty == "Barber");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_Should_Return_Zero_When_No_Match()
    {
        // Arrange
        var provider = new Provider { Name = "Test", Specialty = "Barber", BusinessId = 1, IsActive = true };
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(p => p.Specialty == "NonExistent");

        // Assert
        result.Should().Be(0);
    }

    #endregion
}

