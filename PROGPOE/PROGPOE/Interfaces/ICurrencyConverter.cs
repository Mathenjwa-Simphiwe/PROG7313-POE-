namespace PROGPOE.Interfaces
{
    public interface ICurrencyConverter
    {
        decimal Convert(decimal amount, string fromCurrency, string toCurrency);
        decimal GetExchangeRate(string fromCurrency, string toCurrency);
    }
}