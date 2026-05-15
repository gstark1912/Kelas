using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;

namespace Kelas.Services;

public class CashAccountService : ICashAccountService
{
    private readonly ICashAccountRepository _repository;
    private readonly ICashMovementRepository _movementRepository;

    public CashAccountService(ICashAccountRepository repository, ICashMovementRepository movementRepository)
    {
        _repository = repository;
        _movementRepository = movementRepository;
    }

    public async Task<List<CashAccountResponse>> GetAllAsync()
    {
        var entities = await _repository.GetActiveAsync();
        return entities.Select(MapToResponse).ToList();
    }

    public async Task<Dictionary<string, CashAccountResponse>> GetByIdsAsync(IEnumerable<ObjectId> ids)
    {
        var entities = await _repository.GetByIdsAsync(ids);
        return entities.ToDictionary(x => x.Id.ToString(), MapToResponse);
    }

    public async Task<CashAccountSummaryResponse> GetSummaryAsync()
    {
        var accounts = await GetAllAsync();
        return new CashAccountSummaryResponse
        {
            Accounts = accounts,
            TotalBalance = accounts.Sum(x => x.CurrentBalance)
        };
    }

    public async Task<CashAccountResponse> GetByIdAsync(string id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("CashAccount", id);

        return MapToResponse(entity);
    }

    public async Task<CashAccountResponse> CreateAsync(CreateCashAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new BusinessException("El nombre es obligatorio.");

        var trimmedName = request.Name.Trim();

        var existing = await _repository.GetByNameAsync(trimmedName);
        if (existing is not null)
            throw new BusinessException($"Ya existe una cuenta con el nombre '{existing.Name}'.");

        var entity = new CashAccount
        {
            Name = trimmedName,
            CurrentBalance = 0,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return MapToResponse(created);
    }

    public async Task IncrementBalanceAsync(string accountId, decimal amount, object? session = null)
    {
        var account = await _repository.GetByIdAsync(accountId)
            ?? throw new NotFoundException("CashAccount", accountId);

        await _repository.IncrementBalanceAsync(account.Id.ToString(), amount, session);
    }

    public async Task DecrementBalanceAsync(string accountId, decimal amount, object? session = null)
    {
        var account = await _repository.GetByIdAsync(accountId)
            ?? throw new NotFoundException("CashAccount", accountId);

        await _repository.DecrementBalanceAsync(account.Id.ToString(), amount, session);
    }

    public async Task RegisterPaymentAsync(string accountId, decimal amount, string concept, string description, string referenceType, string referenceId, DateTime date, object? session = null)
    {
        var account = await _repository.GetByIdAsync(accountId)
            ?? throw new NotFoundException("CashAccount", accountId);

        var movement = new CashMovement
        {
            CashAccountId = ObjectId.Parse(accountId),
            Type = "Egreso",
            Concept = concept,
            Amount = amount,
            Description = description,
            Date = date,
            Origin = "Automático",
            ReferenceType = referenceType,
            ReferenceId = ObjectId.Parse(referenceId),
            CreatedAt = DateTime.UtcNow
        };

        await _movementRepository.CreateAsync(movement, session);
        await _repository.DecrementBalanceAsync(accountId, amount, session);
    }

    private static CashAccountResponse MapToResponse(CashAccount entity) => new()
    {
        Id = entity.Id.ToString(),
        Name = entity.Name,
        CurrentBalance = entity.CurrentBalance,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt
    };
}
