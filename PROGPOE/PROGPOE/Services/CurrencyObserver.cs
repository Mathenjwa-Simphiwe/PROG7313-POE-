using PROGPOE.Interfaces;

namespace PROGPOE.Services
{
    public class CurrencyConverter : ICurrencyConverter
    {
        private readonly Dictionary<string, decimal> _rates = new()
        {
            ["USD"] = 1.00m,
            ["EUR"] = 0.92m,
            ["GBP"] = 0.79m,
            ["ZAR"] = 18.50m,
            ["JPY"] = 149.50m
        };

        public decimal Convert(decimal amount, string from, string to)
        {
            var fromRate = _rates.GetValueOrDefault(from.ToUpper(), 1.0m);
            var toRate = _rates.GetValueOrDefault(to.ToUpper(), 1.0m);
            return Math.Round(amount / fromRate * toRate, 2);
        }

        public decimal GetExchangeRate(string from, string to)
        {
            var fromRate = _rates.GetValueOrDefault(from.ToUpper(), 1.0m);
            var toRate = _rates.GetValueOrDefault(to.ToUpper(), 1.0m);
            return Math.Round(toRate / fromRate, 4);
        }
    }
}