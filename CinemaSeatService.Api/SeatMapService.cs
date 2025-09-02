using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;

namespace CinemaSeatService.Api;

public class SeatMapService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SeatMapService> _logger;
    private const string CacheKey = "seat-plan";

    public SeatMapService(HttpClient httpClient, IMemoryCache cache, ILogger<SeatMapService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SeatPlan> GetSeatPlanAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out SeatPlan cached))
        {
            return cached;
        }

        try
        {
            var options = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
            {
                AllowTrailingCommas = true
            };
            var upstream = await _httpClient.GetFromJsonAsync<UpstreamSeatMap[]>("", options, cancellationToken);
            var first = upstream?.FirstOrDefault() ?? throw new InvalidOperationException("Upstream returned no data");
            var plan = SeatMapper.Map(first);
            _cache.Set(CacheKey, plan, TimeSpan.FromSeconds(5));
            return plan;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch seat map");
            if (_cache.TryGetValue(CacheKey, out SeatPlan fallback))
            {
                return fallback;
            }
            throw;
        }
    }

    public async Task<bool?> CheckSeatAsync(string row, int seatNumber, CancellationToken cancellationToken)
    {
        var plan = await GetSeatPlanAsync(cancellationToken);
        var seat = plan.Seats.FirstOrDefault(s =>
            string.Equals(s.Row, row, StringComparison.OrdinalIgnoreCase) &&
            s.Number == seatNumber);
        return seat?.Status == "available";
    }
}
