using Kelas.Domain.Entities;
using Kelas.Domain.Exceptions;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Interfaces.Services;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICashAccountService _cashAccountService;
    private readonly ICashAccountRepository _cashAccountRepository;
    private readonly IMongoClient _mongoClient;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        ICashAccountService cashAccountService,
        ICashAccountRepository cashAccountRepository,
        IMongoClient mongoClient)
    {
        _expenseRepository = expenseRepository;
        _cashAccountService = cashAccountService;
        _cashAccountRepository = cashAccountRepository;
        _mongoClient = mongoClient;
    }

    public async Task<ExpenseListResponse> GetByFiltersAsync(ExpenseFilterRequest filter)
    {
        var expenses = await _expenseRepository.GetByFiltersAsync(filter);
        var cashAccounts = await _cashAccountRepository.GetActiveAsync();
        var cashAccountsDict = cashAccounts.ToDictionary(x => x.Id.ToString());

        var items = expenses.Select(e => MapToResponse(e, cashAccountsDict)).ToList();

        var totalAmount = items.Sum(x => x.Amount);
        var totalCount = items.Count;
        
        var topCategory = items.GroupBy(x => x.Category)
            .Select(g => new CategoryTotalResponse
            {
                Category = g.Key,
                Total = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        return new ExpenseListResponse
        {
            Items = items,
            Kpis = new ExpenseKpiResponse
            {
                TotalAmount = totalAmount,
                TotalCount = totalCount,
                TopCategory = topCategory
            }
        };
    }

    public async Task<ExpenseResponse> GetByIdAsync(string id)
    {
        var entity = await _expenseRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Expense", id);

        var cashAccount = await _cashAccountRepository.GetByIdAsync(entity.CashAccountId.ToString());
        
        var response = MapToResponse(entity, new Dictionary<string, CashAccount>());
        if (cashAccount != null)
        {
            response.CashAccountName = cashAccount.Name;
        }

        return response;
    }

    public async Task<List<CategoryTotalResponse>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
    {
        return await _expenseRepository.GetExpensesByCategoryAsync(from, to);
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request)
    {
        // Validation
        if (request.Amount <= 0)
            throw new BusinessException("El monto debe ser mayor a 0.");

        if (string.IsNullOrWhiteSpace(request.Category))
            throw new BusinessException("La categoría es obligatoria.");

        if (request.Date == default)
            throw new BusinessException("La fecha es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new BusinessException("La descripción es obligatoria.");

        if (string.IsNullOrWhiteSpace(request.CashAccountId))
            throw new BusinessException("La cuenta de caja es obligatoria.");

        var entity = new Expense
        {
            Date = request.Date,
            Amount = request.Amount,
            Category = request.Category.Trim(),
            Description = request.Description.Trim(),
            CashAccountId = ObjectId.Parse(request.CashAccountId)
        };

        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();
        try
        {
            // 1. Save expense
            var created = await _expenseRepository.CreateAsync(entity, session);

            // 2. Register payment (creates cash movement and updates account balance)
            await _cashAccountService.RegisterPaymentAsync(
                accountId: request.CashAccountId,
                amount: request.Amount,
                concept: "Gasto",
                description: $"Gasto: {request.Category} - {request.Description}",
                referenceType: "Expense",
                referenceId: created.Id.ToString(),
                date: request.Date,
                session: session
            );

            await session.CommitTransactionAsync();
            
            var cashAccount = await _cashAccountRepository.GetByIdAsync(request.CashAccountId);
            var cashAccountDict = cashAccount != null 
                ? new Dictionary<string, CashAccount> { { cashAccount.Id.ToString(), cashAccount } }
                : new Dictionary<string, CashAccount>();

            return MapToResponse(created, cashAccountDict);
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    private static ExpenseResponse MapToResponse(Expense entity, Dictionary<string, CashAccount> cashAccountsDict)
    {
        var cashAccountId = entity.CashAccountId.ToString();
        var cashAccountName = cashAccountsDict.TryGetValue(cashAccountId, out var account) ? account.Name : "Desconocida";

        return new ExpenseResponse
        {
            Id = entity.Id.ToString(),
            Date = entity.Date,
            Amount = entity.Amount,
            Category = entity.Category,
            Description = entity.Description,
            CashAccountId = cashAccountId,
            CashAccountName = cashAccountName,
            CreatedAt = entity.CreatedAt
        };
    }
}
