import { Market } from './Market.ts';
import { parseDateFields } from '../../utilities/utils.ts';

export interface PriceEntry {
    id: string;
    productId: string;
    marketId: string;
    market?: Market;
    productUrl: string;
    price: number;
    discountedPrice?: number;
    lastUpdated: Date;
}

export const parsePriceEntry = (data: any): PriceEntry => {
    return parseDateFields<PriceEntry>(data, ['lastUpdated']);
};

export const parsePriceEntries = (
    data: any[] | undefined
): PriceEntry[] | undefined => {
    return data?.map(parsePriceEntry);
};

export const splitPrice = (
    price: number | string | undefined
): [string, string] | [undefined, undefined] => {
    if (price === undefined) {
        return [undefined, undefined];
    }

    const priceStr =
        typeof price === 'number' ? price.toFixed(2) : price.replace(',', '.');

    const [integerPart, decimalPart] = priceStr.split('.');
    return [integerPart, decimalPart || '00'];
};
