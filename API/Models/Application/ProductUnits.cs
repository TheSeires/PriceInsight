namespace API.Models.Application;

public struct ProductUnits
{
    public string Volume { get; set; }
    public string Weight { get; set; }
    public string Amount { get; set; }
    public string NameWithoutUnits { get; set; }

    public ProductUnits(string volume, string weight, string amount, string nameWithoutUnits)
    {
        Volume = volume;
        Weight = weight;
        Amount = amount;
        NameWithoutUnits = nameWithoutUnits;
    }

    public readonly bool HasUnits =>
        !string.IsNullOrEmpty(Volume)
        || !string.IsNullOrEmpty(Weight)
        || !string.IsNullOrEmpty(Amount);
}
