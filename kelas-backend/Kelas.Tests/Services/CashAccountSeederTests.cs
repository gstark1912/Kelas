using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kelas.Tests.Services;

public class CashAccountSeederTests
{
    private readonly Mock<ICashAccountRepository> _repositoryMock;
    private readonly Mock<ILogger<CashAccountSeeder>> _loggerMock;
    private readonly CashAccountSeeder _seeder;

    public CashAccountSeederTests()
    {
        _repositoryMock = new Mock<ICashAccountRepository>();
        _loggerMock = new Mock<ILogger<CashAccountSeeder>>();
        _seeder = new CashAccountSeeder(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SeedAsync_CreatesMissingDefaultAccounts()
    {
        // Arrange - all accounts are missing
        _repositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((CashAccount?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CashAccount>()))
            .ReturnsAsync((CashAccount entity) => entity);
        _repositoryMock.Setup(r => r.EnsureIndexesAsync()).Returns(Task.CompletedTask);

        // Act
        await _seeder.SeedAsync();

        // Assert - all 3 default accounts should be created
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<CashAccount>(e =>
            e.Name == "Efectivo" && e.CurrentBalance == 0 && e.IsActive == true
        )), Times.Once);

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<CashAccount>(e =>
            e.Name == "Banco" && e.CurrentBalance == 0 && e.IsActive == true
        )), Times.Once);

        _repositoryMock.Verify(r => r.CreateAsync(It.Is<CashAccount>(e =>
            e.Name == "Mercado Pago" && e.CurrentBalance == 0 && e.IsActive == true
        )), Times.Once);

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CashAccount>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SeedAsync_SkipsExistingAccountsWithoutError()
    {
        // Arrange - all accounts already exist
        _repositoryMock.Setup(r => r.GetByNameAsync("Efectivo"))
            .ReturnsAsync(new CashAccount { Name = "Efectivo", IsActive = true });
        _repositoryMock.Setup(r => r.GetByNameAsync("Banco"))
            .ReturnsAsync(new CashAccount { Name = "Banco", IsActive = true });
        _repositoryMock.Setup(r => r.GetByNameAsync("Mercado Pago"))
            .ReturnsAsync(new CashAccount { Name = "Mercado Pago", IsActive = true });
        _repositoryMock.Setup(r => r.EnsureIndexesAsync()).Returns(Task.CompletedTask);

        // Act
        await _seeder.SeedAsync();

        // Assert - no accounts should be created
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CashAccount>()), Times.Never);
    }

    [Fact]
    public async Task SeedAsync_CallsEnsureIndexesAsync()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>())).ReturnsAsync((CashAccount?)null);
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<CashAccount>()))
            .ReturnsAsync((CashAccount entity) => entity);
        _repositoryMock.Setup(r => r.EnsureIndexesAsync()).Returns(Task.CompletedTask);

        // Act
        await _seeder.SeedAsync();

        // Assert
        _repositoryMock.Verify(r => r.EnsureIndexesAsync(), Times.Once);
    }

    [Fact]
    public async Task SeedAsync_CatchesExceptionsAndDoesNotRethrow()
    {
        // Arrange - EnsureIndexesAsync throws an exception
        _repositoryMock.Setup(r => r.EnsureIndexesAsync())
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() => _seeder.SeedAsync());
        Assert.Null(exception);
    }
}
