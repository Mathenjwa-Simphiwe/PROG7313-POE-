using PROGPOE.Services;
using Xunit;

namespace PROGPOE.Tests
{
    public class CurrencyConverterTests
    {
        [Fact]
        public void USD_To_EUR() { var c = new CurrencyConverter(); Assert.Equal(92m, c.Convert(100m, "USD", "EUR")); }
        [Fact]
        public void Same_Currency() { var c = new CurrencyConverter(); Assert.Equal(100m, c.Convert(100m, "USD", "USD")); }
        [Fact]
        public void Rate_Positive() { var c = new CurrencyConverter(); Assert.True(c.GetExchangeRate("USD", "EUR") > 0); }
    }
}