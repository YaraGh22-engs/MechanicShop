using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Domain.Common.Results;

namespace MechanicShop.Application.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : ICachedQuery<Result<List<CustomerDto>>>
{
    public string CacheKey => "customers";
    public string[] Tags => ["customer"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}

// Query لجلب كل العملاء مع تخزين مؤقت لمدة 10 دقائق
//Cache Hit:
//1. Client → GET /api/customers  
//2. Controller → Send(new GetCustomersQuery())
//3. CachingBehavior → Check Cache("customers") → Found!
//4. CachingBehavior → Return Cached Result
//5. Controller → Return HTTP 200 + CustomerDtos

//⏱️ Time: ~5ms ⚡