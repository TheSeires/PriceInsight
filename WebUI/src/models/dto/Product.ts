import { Category } from './Category';
import { parsePriceEntries, PriceEntry } from './PriceEntry.ts';
import { parseDateFields } from '../../utilities/utils.ts';

export interface Product {
    id: string;
    name: string;
    description: string;
    imageUrl?: string;
    sourceMarketId: string;
    sourceCategory: string;
    categoryId: string;
    category?: Category;
    priceEntries?: PriceEntry[];
    aliases: string[];
    attributes: Record<string, any>;
    added: Date;
    updated: Date;
}

export const parseProduct = (data: any): Product => {
    return parseDateFields<Product>(
        data,
        ['added', 'updated', 'priceEntries'],
        [
            (productData) => ({
                ...productData,
                priceEntries: parsePriceEntries(productData.priceEntries),
            }),
        ]
    );
};

export const parseProducts = (data: any[]): Product[] => {
    return data.map(parseProduct);
};

export const getLowestPriceEntry = (
    product: Product
): PriceEntry | undefined => {
    if (!product.priceEntries || product.priceEntries.length === 0) {
        return undefined;
    }

    return product.priceEntries.reduce((lowest, entry) => {
        const priceToCompare = entry.discountedPrice ?? entry.price;
        const lowestPrice = lowest.discountedPrice ?? lowest.price;

        return priceToCompare < lowestPrice ? entry : lowest;
    });
};

export const getDiscountPercentage = (
    price: number,
    discountedPrice: number
) => {
    const discount = ((price - discountedPrice) / price) * 100;
    return Math.floor(discount);
};
