using System.Text.Json;

namespace PROGPOE.Services
{
    /// <summary>
    /// Fetches live USD → ZAR exchange rates from the open.er-api.com API using HttpClient.
    /// This is the external API integration required by the spec.
    /// Falls back to a hardcoded rate if the network call fails.
    /// </summary>
    public class ExchangeRateService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ExchangeRateService> _logger;

        // Fallback rate used if the external API is unreachable
        private const decimal FallbackUsdToZar = 18.50m;

        public ExchangeRateService(HttpClient http, ILogger<ExchangeRateService> logger)
        {
            _http = http;
            _logger = logger;
        }

        /// <summary>
        /// Calls open.er-api.com to get the latest USD → ZAR rate.
        /// Returns the live rate, or the fallback if the call fails.
        /// </summary>
        public async Task<decimal> GetUsdToZarRateAsync()
        {
            try
            {
                // Free, no-key-required exchange rate API
                var response = await _http.GetAsync("https://open.er-api.com/v6/latest/USD");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("rates", out var rates) &&
                    rates.TryGetProperty("ZAR", out var zarProp))
                {
                    var rate = zarProp.GetDecimal();
                    _logger.LogInformation("Live USD/ZAR rate fetched: {Rate}", rate);
                    return rate;
                }

                _logger.LogWarning("ZAR not found in API response, using fallback rate.");
                return FallbackUsdToZar;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Currency API call failed: {Message}. Using fallback rate {Rate}.",
                    ex.Message, FallbackUsdToZar);
                return FallbackUsdToZar;
            }
        }

        /// <summary>
        /// Converts a USD amount to ZAR using the live exchange rate.
        /// </summary>
        public async Task<(decimal zarAmount, decimal rateUsed)> ConvertUsdToZarAsync(decimal usdAmount)
        {
            var rate = await GetUsdToZarRateAsync();
            return (Math.Round(usdAmount * rate, 2), rate);
        }
    }
}
