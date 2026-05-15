using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class CashMovementService : ICashMovementService
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Ingreso",
        "Egreso"
    };

    private static readonly HashSet<string> ValidConcepts = new(StringComparer.OrdinalIgnoreCase)
    {
        "Venta",
        "Compra MP",
        "Gasto",
        "Aporte de capital",
        "Retiro de efectivo",
        "Transferencia",
        "Ajuste de saldo",
        "Otro"
    };

    private readonly IMongoClient _client;
    private readonly ICashMovementRepository _cashMovementRepository;
    private readonly ICashAccountService _cashAccountService;

    public CashMovementService(
        IMongoClient client,
        ICashMovementRepository cashMovementRepository,
        ICashAccountService cashAccountService)
    {
        _client = client;
        _cashMovementRepository = cashMovementRepository;
        _cashAccountService = cashAccountService;
    }

    public async Task<CashMovementResponse> CreateManualAsync(CreateManualCashMovementRequest request)
    {
        ValidateAmount(request.Amount);
        ValidateRequiredDate(request.Date);
        var type = NormalizeType(request.Type);
        var concept = ValidateConcept(request.Concept);

        if (string.IsNullOrWhiteSpace(request.CashAccountId))
            throw new BusinessException("La cuenta de caja es obligatoria.");

        var account = await _cashAccountService.GetByIdAsync(request.CashAccountId);

        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var movement = new CashMovement
            {
                CashAccountId = ObjectId.Parse(account.Id),
                Type = type,
                Concept = concept,
                Amount = request.Amount,
                Description = request.Description?.Trim() ?? string.Empty,
                Date = request.Date,
                Origin = "Manual",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _cashMovementRepository.CreateAsync(movement, session);

            if (type == "Ingreso")
                await _cashAccountService.IncrementBalanceAsync(account.Id, request.Amount, session);
            else
                await _cashAccountService.DecrementBalanceAsync(account.Id, request.Amount, session);

            await session.CommitTransactionAsync();
            return MapToResponse(created, account.Name);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<CashTransferResponse> CreateTransferAsync(CreateCashTransferRequest request)
    {
        ValidateAmount(request.Amount);
        ValidateRequiredDate(request.Date);

        if (string.IsNullOrWhiteSpace(request.OriginCashAccountId))
            throw new BusinessException("La cuenta origen es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.DestinationCashAccountId))
            throw new BusinessException("La cuenta destino es obligatoria.");

        if (request.OriginCashAccountId == request.DestinationCashAccountId)
            throw new BusinessException("La cuenta origen y destino no pueden ser la misma.");

        var originAccount = await _cashAccountService.GetByIdAsync(request.OriginCashAccountId);
        var destinationAccount = await _cashAccountService.GetByIdAsync(request.DestinationCashAccountId);
        var note = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : $" - {request.Description.Trim()}";

        using var session = await _client.StartSessionAsync();
        session.StartTransaction();
        try
        {
            var originMovement = new CashMovement
            {
                CashAccountId = ObjectId.Parse(originAccount.Id),
                Type = "Egreso",
                Concept = "Transferencia",
                Amount = request.Amount,
                Description = $"Transferencia a {destinationAccount.Name}{note}",
                Date = request.Date,
                Origin = "Manual",
                CreatedAt = DateTime.UtcNow
            };

            var destinationMovement = new CashMovement
            {
                CashAccountId = ObjectId.Parse(destinationAccount.Id),
                Type = "Ingreso",
                Concept = "Transferencia",
                Amount = request.Amount,
                Description = $"Transferencia desde {originAccount.Name}{note}",
                Date = request.Date,
                Origin = "Manual",
                CreatedAt = DateTime.UtcNow
            };

            var createdOrigin = await _cashMovementRepository.CreateAsync(originMovement, session);
            var createdDestination = await _cashMovementRepository.CreateAsync(destinationMovement, session);

            await _cashAccountService.DecrementBalanceAsync(originAccount.Id, request.Amount, session);
            await _cashAccountService.IncrementBalanceAsync(destinationAccount.Id, request.Amount, session);

            await session.CommitTransactionAsync();

            return new CashTransferResponse
            {
                OriginMovement = MapToResponse(createdOrigin, originAccount.Name),
                DestinationMovement = MapToResponse(createdDestination, destinationAccount.Name)
            };
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<CashMovementListResponse> GetByFiltersAsync(CashMovementFilterRequest filter)
    {
        ValidateFilters(filter);

        var movements = await _cashMovementRepository.GetByFiltersAsync(filter);
        var accountIds = movements.Select(x => x.CashAccountId).Distinct().ToList();
        var accounts = await _cashAccountService.GetByIdsAsync(accountIds);

        var items = movements.Select(m =>
        {
            accounts.TryGetValue(m.CashAccountId.ToString(), out var account);
            return MapToResponse(m, account?.Name ?? "Desconocida");
        }).ToList();

        var totalIncome = items
            .Where(x => IsIncome(x.Type))
            .Sum(x => x.Amount);
        var totalExpense = items
            .Where(x => IsExpense(x.Type))
            .Sum(x => x.Amount);

        return new CashMovementListResponse
        {
            Items = items,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetTotal = totalIncome - totalExpense
        };
    }

    private static void ValidateFilters(CashMovementFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Type) && !ValidTypes.Contains(filter.Type.Trim()))
            throw new BusinessException("El tipo debe ser Ingreso o Egreso.");

        if (!string.IsNullOrWhiteSpace(filter.Concept) && !ValidConcepts.Contains(filter.Concept.Trim()))
            throw new BusinessException("El concepto no es válido.");

        if (!string.IsNullOrWhiteSpace(filter.CashAccountId) && !ObjectId.TryParse(filter.CashAccountId, out _))
            throw new BusinessException("La cuenta de caja no es válida.");

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom.Value > filter.DateTo.Value)
            throw new BusinessException("La fecha desde no puede ser posterior a la fecha hasta.");
    }

    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessException("El monto debe ser mayor a 0.");
    }

    private static void ValidateRequiredDate(DateTime date)
    {
        if (date == default)
            throw new BusinessException("La fecha es obligatoria.");
    }

    private static string NormalizeType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new BusinessException("El tipo es obligatorio.");

        var trimmed = type.Trim();
        if (!ValidTypes.Contains(trimmed))
            throw new BusinessException("El tipo debe ser Ingreso o Egreso.");

        return trimmed.Equals("Ingreso", StringComparison.OrdinalIgnoreCase) ? "Ingreso" : "Egreso";
    }

    private static string ValidateConcept(string concept)
    {
        if (string.IsNullOrWhiteSpace(concept))
            throw new BusinessException("El concepto es obligatorio.");

        var trimmed = concept.Trim();
        if (!ValidConcepts.Contains(trimmed))
            throw new BusinessException("El concepto no es válido.");

        return ValidConcepts.First(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsIncome(string type)
    {
        return type.Equals("Ingreso", StringComparison.OrdinalIgnoreCase)
            || type.Equals("income", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExpense(string type)
    {
        return type.Equals("Egreso", StringComparison.OrdinalIgnoreCase)
            || type.Equals("expense", StringComparison.OrdinalIgnoreCase);
    }

    private static CashMovementResponse MapToResponse(CashMovement movement, string accountName)
    {
        return new CashMovementResponse
        {
            Id = movement.Id.ToString(),
            CashAccountId = movement.CashAccountId.ToString(),
            CashAccountName = accountName,
            Type = movement.Type,
            Concept = movement.Concept,
            Amount = movement.Amount,
            Description = movement.Description,
            Date = movement.Date,
            Origin = movement.Origin,
            ReferenceType = movement.ReferenceType,
            ReferenceId = movement.ReferenceId?.ToString(),
            LinkedMovementId = movement.LinkedMovementId?.ToString(),
            CreatedAt = movement.CreatedAt
        };
    }
}
