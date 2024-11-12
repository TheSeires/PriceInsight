using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using API.Models.Application;
using API.Models.Database;
using API.Models.Dto;
using API.Services.Interfaces;
using FuzzySharp;
using MongoDB.Bson;

namespace API.Services;

public class FuzzyProductMatchingStrategy : IProductMatchingStrategy
{
    private static readonly Regex VolumeRegEx = new Regex(
        @"(\d+(?:[,.]\d+)?)\s*(л|l|мл|ml)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WeightRegEx = new Regex(
        @"(\d+(?:[,.]\d+)?)\s*(кг|kg|г|g)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AmountRegEx = new Regex(
        @"(\d+)\s*(шт|pc|pcs)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ConcurrentDictionary<string, ProductUnits> _unitsCache =
        new ConcurrentDictionary<string, ProductUnits>();

    private readonly ConcurrentDictionary<string, string> _normalizedNameCache =
        new ConcurrentDictionary<string, string>();

    private class ProductMatchData
    {
        public required ProductDto Product { get; init; }
        public ProductUnits Units { get; init; }
        public required string NormalizedName { get; init; }
        public required List<ProductAlias> NormalizedAliases { get; init; }
    }

    public (ProductDto Product, RequiredDatabaseAction Action) MatchOrCreateProduct(ProductToMatch productToMatch,
        string marketId, string? mappedCategoryId, List<ProductDto> allProducts, double matchThreshold)
    {
        var inputName = productToMatch.Name;
        var inputUnits = GetOrExtractUnits(inputName);
        var normalizedInputName = GetNormalizedName(inputName);

        var bestProductMatching =
            FindBestProductMatching(marketId, allProducts, normalizedInputName, inputUnits, matchThreshold);

        if (bestProductMatching is not null)
        {
            if (bestProductMatching.Aliases.Any(a => a.MarketId == marketId && a.Name == productToMatch.Name) == false)
            {
                bestProductMatching.Aliases.Add(new ProductAlias(productToMatch.Name, marketId));
            }

            if (productToMatch.Attributes.Count != 0)
            {
                foreach (var newAttrKvp in productToMatch.Attributes)
                {
                    var exists = bestProductMatching.Attributes.Any(existingAttrKvp =>
                        string.Equals(existingAttrKvp.Key, newAttrKvp.Key, StringComparison.OrdinalIgnoreCase));

                    if (exists) continue;
                    bestProductMatching.Attributes.Add(newAttrKvp.Key, newAttrKvp.Value);
                }
            }

            if (marketId == bestProductMatching.SourceMarketId)
            {
                bestProductMatching.ImageUrl = productToMatch.ImageUrl;
            }

            bestProductMatching.Updated = DateTime.UtcNow;
            return (bestProductMatching, RequiredDatabaseAction.Update);
        }

        var utcNow = DateTime.UtcNow;
        var newProduct = new ProductDto
        {
            Id = ObjectId.GenerateNewId(utcNow).ToString(),
            Name = productToMatch.Name,
            ImageUrl = productToMatch.ImageUrl,
            Description = productToMatch.Description,
            SourceMarketId = marketId,
            CategoryId = mappedCategoryId ?? ObjectId.Empty.ToString(),
            SourceCategory = productToMatch.Category,
            Attributes = new Dictionary<string, string>(productToMatch.Attributes),
            Aliases = [new ProductAlias(productToMatch.Name, marketId)],
            Added = utcNow,
            Updated = utcNow,
        };

        allProducts.Add(newProduct);
        return (newProduct, RequiredDatabaseAction.Create);
    }

    private ProductDto? FindBestProductMatching(string marketId, List<ProductDto> products, string normalizedInputName,
        ProductUnits inputUnits, double matchThreshold)
    {
        var productBatches = products
            .AsParallel()
            .Select(product => new ProductMatchData
            {
                Product = product,
                Units = GetOrExtractUnits(product.Name),
                NormalizedName = GetNormalizedName(product.Name),
                NormalizedAliases =
                    [..product.Aliases.Select(x => new ProductAlias(GetNormalizedName(x.Name), x.MarketId))]
            })
            .Where(pmd => AreUnitsMatching(inputUnits, pmd.Units))
            .ToList();

        if (productBatches.Count == 0)
        {
            return null;
        }

        var quickMatches = productBatches
            .Where(pmd =>
                pmd.NormalizedName.Contains(normalizedInputName, StringComparison.OrdinalIgnoreCase) ||
                normalizedInputName.Contains(pmd.NormalizedName, StringComparison.OrdinalIgnoreCase) ||
                pmd.NormalizedAliases.Any(alias =>
                    alias.MarketId != marketId &&
                    (alias.Name.Contains(normalizedInputName, StringComparison.OrdinalIgnoreCase) ||
                     normalizedInputName.Contains(alias.Name, StringComparison.OrdinalIgnoreCase))))
            .ToList();

        var candidatesForFuzzyMatch = quickMatches.Count > 0 ? quickMatches : productBatches;

        var bestMatch = candidatesForFuzzyMatch
            .AsParallel()
            .Select(pmd =>
            {
                var isUncreatedSiblingProduct = pmd.NormalizedAliases.Any(a =>
                    a.MarketId == marketId && normalizedInputName != a.Name &&
                    Fuzz.WeightedRatio(a.Name, normalizedInputName) >= matchThreshold);

                // To create a related product that has almost the same name, but with a slight difference
                if (isUncreatedSiblingProduct)
                {
                    return new
                    {
                        Product = pmd.Product,
                        Score = 0
                    };
                }

                var nameScore = Fuzz.WeightedRatio(pmd.NormalizedName, normalizedInputName);
                var aliasScore = pmd.NormalizedAliases
                    .Select(alias => Fuzz.WeightedRatio(alias.Name, normalizedInputName))
                    .DefaultIfEmpty(0)
                    .Max();

                return new
                {
                    Product = pmd.Product,
                    Score = Math.Max(nameScore, aliasScore)
                };
            })
            .MaxBy(x => x.Score);

        return bestMatch?.Score > matchThreshold ? bestMatch.Product : null;
    }

    private string GetNormalizedName(string input)
    {
        return _normalizedNameCache.GetOrAdd(input, name =>
        {
            // Remove common noise words and characters
            var normalized = name.ToLowerInvariant()
                .Replace("\"", "")
                .Replace("'", "")
                .Replace(",", " ")
                .Replace(".", " ");

            // Replace multiple spaces with single space
            normalized = string.Join(" ",
                normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            return normalized;
        });
    }

    private ProductUnits GetOrExtractUnits(string name)
    {
        return _unitsCache.GetOrAdd(name, ExtractUnits);
    }

    private ProductUnits ExtractUnits(string name)
    {
        var volume = ExtractUnit(name, VolumeRegEx);
        var weight = ExtractUnit(name, WeightRegEx);
        var amount = ExtractUnit(name, AmountRegEx);

        var nameWithoutUnits = VolumeRegEx.Replace(name, "");
        nameWithoutUnits = WeightRegEx.Replace(nameWithoutUnits, "");
        nameWithoutUnits = AmountRegEx.Replace(nameWithoutUnits, "");

        return new ProductUnits(
            NormalizeVolume(volume),
            NormalizeWeight(weight),
            amount,
            nameWithoutUnits.Trim()
        );
    }

    private static string ExtractUnit(string name, Regex regex)
    {
        var match = regex.Match(name);
        return match.Success ? match.Value : string.Empty;
    }

    private static bool AreUnitsMatching(ProductUnits units1, ProductUnits units2)
    {
        if (units1.HasUnits != units2.HasUnits)
            return false;

        if (!units1.HasUnits)
            return true;

        return units1.Volume == units2.Volume
               && units1.Weight == units2.Weight
               && units1.Amount == units2.Amount;
    }

    private static string NormalizeUnit(
        string input,
        Regex regex,
        string smallUnit,
        string largeUnit,
        decimal conversionFactor
    )
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var match = regex.Match(input);
        if (match.Success == false)
            return input;

        var value = decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups[2].Value.ToLower();

        if (unit == smallUnit.ToLower())
        {
            value /= conversionFactor;
        }

        return $"{value}{largeUnit}";
    }

    private string NormalizeVolume(string volume) =>
        NormalizeUnit(volume, VolumeRegEx, "мл", "л", 1000);

    private string NormalizeWeight(string weight) =>
        NormalizeUnit(weight, WeightRegEx, "г", "кг", 1000);
}