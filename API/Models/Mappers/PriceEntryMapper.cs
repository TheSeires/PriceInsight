using API.Models.Database;
using API.Models.Dto;

namespace API.Models.Mappers;

public static class PriceEntryMapper
{
    public static PriceEntry MapToDomain(this PriceEntryDto priceEntryDto)
    {
        return new PriceEntry
        {
            Id = priceEntryDto.Id,
            ProductId = priceEntryDto.ProductId,
            MarketId = priceEntryDto.MarketId,
            ProductUrl = priceEntryDto.ProductUrl,
            Price = priceEntryDto.Price,
            DiscountedPrice = priceEntryDto.DiscountedPrice,
            LastUpdated = priceEntryDto.LastUpdated,
        };
    }

    public static PriceEntryDto MapToDto(this PriceEntry priceEntryDto)
    {
        return new PriceEntryDto
        {
            Id = priceEntryDto.Id,
            ProductId = priceEntryDto.ProductId,
            MarketId = priceEntryDto.MarketId,
            ProductUrl = priceEntryDto.ProductUrl,
            Price = priceEntryDto.Price,
            DiscountedPrice = priceEntryDto.DiscountedPrice,
            LastUpdated = priceEntryDto.LastUpdated,
        };
    }
}