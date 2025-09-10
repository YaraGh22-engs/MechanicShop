using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviours;


//Cache behavior in the MediatR Pipeline
//Before executing any query, it checks whether the result of this request (response) is stored in the cache.
//If it exists, it returns it immediately and provides a database call. 
//If it does not exist, it passes the request, performs the processing, and then stores the result in the cache.
public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Chech if request is cachedRequest
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct);
        }

        _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);

        // check if request exist in cash based on "CacheKey"
        var result = await _cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            _ => new ValueTask<TResponse>((TResponse)(object)null!),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableUnderlyingData
            },
            cancellationToken: ct);

        // if request does not exist in cash then put the success it in the cash 
        if (result is null)
        {
            result = await next(ct);

            if (result is IResult res && res.IsSuccess)
            {
                _logger.LogInformation("Caching result for {RequestName}", typeof(TRequest).Name);

                await _cache.SetAsync(
                    cachedRequest.CacheKey,
                    result,
                    new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.Expiration
                    },
                    cachedRequest.Tags,
                    ct);
            }
        }
        
        return result;
    }
}