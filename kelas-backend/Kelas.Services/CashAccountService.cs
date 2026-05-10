using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Services;

public class CashAccountService : ICashAccountService
{
    private readonly ICashAccountRepository _repository;

    public CashAccountService(ICashAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CashAccountResponse>> GetAllAsync()
    {
        var entities = await _repository.GetActiveAsync();
        return entities.Select(MapToResponse).ToList();
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
