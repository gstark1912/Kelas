using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Models.Requests;
using Kelas.Services;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace Kelas.Tests.Services;

public class CashAccountServiceTests
{
    private readonly Mock<ICashAccountRepository> _repositoryMock;
    private readonly CashAccountService _service;

    public CashAccountServiceTests()
    {
        _repositoryMock = new Mock<ICashAccountRepository>();
        _service = new CashAccountService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveAccounts()
    {
        // Arrange
        var activeAccounts = new List<CashAccount>
        {
            new() { Id = ObjectId.GenerateNewId(), Name = "Efectivo", CurrentBalance = 100, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = ObjectId.GenerateNewId(), Name = "Banco", CurrentBalance = 500, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _repositoryMock.Setup(r => r.GetActiveAsync()).ReturnsAsync(activeAccounts);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Efectivo", result[0].Name);
        Assert.Equal("Banco", result[1].Name);
        _repositoryMock.Verify(r => r.GetActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsMappedResponse()
    {
        // Arrange
        var id = ObjectId.GenerateNewId();
        var entity = new CashAccount
        {
            Id = id,
            Name = "Efectivo",
            CurrentBalance = 250.50m,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 2, 1, 8, 0, 0, DateTimeKind.Utc)
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id.ToString())).ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(id.ToString());

        // Assert
        Assert.Equal(id.ToString(), result.Id);
        Assert.Equal("Efectivo", result.Name);
        Assert.Equal(250.50m, result.CurrentBalance);
        Assert.True(result.IsActive);
        Assert.Equal(entity.CreatedAt, result.CreatedAt);
        Assert.Equal(entity.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        var id = ObjectId.GenerateNewId().ToString();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((CashAccount?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));
        Assert.Contains(id, exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_CreatesAccountWithBalanceZeroAndIsActiveTrue()
    {
        // Arrange
        var request = new CreateCashAccountRequest { Name = "Nueva Cuenta" };

        _repositoryMock.Setup(r => r.GetByNameAsync("Nueva Cuenta")).ReturnsAsync((CashAccount?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CashAccount>()))
            .ReturnsAsync((CashAccount entity) =>
            {
                entity.Id = ObjectId.GenerateNewId();
                entity.CreatedAt = DateTime.UtcNow;
                return entity;
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.Equal("Nueva Cuenta", result.Name);
        Assert.Equal(0, result.CurrentBalance);
        Assert.True(result.IsActive);
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<CashAccount>(e =>
            e.Name == "Nueva Cuenta" &&
            e.CurrentBalance == 0 &&
            e.IsActive == true
        )), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsBusinessException()
    {
        // Arrange
        var request = new CreateCashAccountRequest { Name = "" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));
        Assert.Equal("El nombre es obligatorio.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithWhitespaceOnlyName_ThrowsBusinessException()
    {
        // Arrange
        var request = new CreateCashAccountRequest { Name = "   " };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));
        Assert.Equal("El nombre es obligatorio.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsBusinessException()
    {
        // Arrange
        var request = new CreateCashAccountRequest { Name = "Efectivo" };
        var existingAccount = new CashAccount
        {
            Id = ObjectId.GenerateNewId(),
            Name = "Efectivo",
            CurrentBalance = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock.Setup(r => r.GetByNameAsync("Efectivo")).ReturnsAsync(existingAccount);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _service.CreateAsync(request));
        Assert.Equal("Ya existe una cuenta con el nombre 'Efectivo'.", exception.Message);
    }
}
