namespace TripTracker.Services;

public static class CurrencyEstimator
{
    // Rough, static rates to USD (not live).
    private static readonly Dictionary<string, decimal> RatesToUsd = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 1.00m,
        ["EUR"] = 1.09m,
        ["GBP"] = 1.27m,
        ["CAD"] = 0.74m,
        ["MXN"] = 0.059m,
        ["JPY"] = 0.0067m,
        ["BRL"] = 0.20m,
        ["AUD"] = 0.66m,
        ["CHF"] = 1.12m,
        ["CNY"] = 0.14m,
        ["INR"] = 0.012m
    };

    public static bool TryConvert(decimal amount, string fromCurrency, string toCurrency, out decimal converted)
    {
        converted = 0m;
        if (amount < 0m)
        {
            return false;
        }

        if (!RatesToUsd.TryGetValue((fromCurrency ?? string.Empty).Trim(), out var fromRate))
        {
            return false;
        }

        if (!RatesToUsd.TryGetValue((toCurrency ?? string.Empty).Trim(), out var toRate))
        {
            return false;
        }

        if (toRate == 0m)
        {
            return false;
        }

        var usdAmount = amount * fromRate;
        converted = decimal.Round(usdAmount / toRate, 2, MidpointRounding.AwayFromZero);
        return true;
    }

    public static string SupportedCodes()
    {
        return string.Join(", ", RatesToUsd.Keys.OrderBy(k => k));
    }

    public static IReadOnlyDictionary<string, decimal> GetRatesToUsd()
    {
        return RatesToUsd;
    }
}
