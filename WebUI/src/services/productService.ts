import axios, { AxiosResponse, HttpStatusCode } from 'axios';
import { parseProduct, parseProducts, Product } from '../models/dto/Product';
import { createUnhandledError } from '../utilities/utils';
import { MatchOperators } from '../models/ApiQueryParams';
import { ApiQueryBuilder } from '../utilities/apiQueryBuilder';
import { PriceEntry } from '../models/dto/PriceEntry.ts';
import {
    isPaginatedItemsResponse,
    PaginatedItemsResponse,
} from '../models/dto/PaginatedItems.ts';
import { CatalogFilters } from '../models/CatalogFilters.ts';

const getByApiQuery = async (
    apiQueryBuilder: ApiQueryBuilder<Product>
): Promise<Product[] | Error> => {
    try {
        return getBySearchParams(apiQueryBuilder.build());
    } catch (error) {
        return error as Error;
    }
};

const getDiscountedProducts = async (
    page: number,
    limit: number
): Promise<Product[] | Error> => {
    try {
        const priceEntryApiParams = new ApiQueryBuilder<PriceEntry>()
            .match('discountedPrice', MatchOperators.exists, 'true')
            .paginate(page, limit)
            .build();

        const priceEntriesRes = await axios.get(
            `/api/v1/price-entries?${priceEntryApiParams}`
        );
        if (priceEntriesRes.status !== HttpStatusCode.Ok) {
            return createUnhandledError(priceEntriesRes);
        }

        const paginatedPriceEntries =
            priceEntriesRes.data as PaginatedItemsResponse<PriceEntry>;
        if (!paginatedPriceEntries || !paginatedPriceEntries.items.length)
            return [];

        const productIds = paginatedPriceEntries.items
            .map((pe) => pe.productId)
            .join(',');
        const productApiParams = new ApiQueryBuilder<Product>()
            .match('id', MatchOperators.in, `[${productIds}]`)
            .build();

        return getBySearchParams(productApiParams);
    } catch (error) {
        return error as Error;
    }
};

const getCatalog = async (filters: CatalogFilters) => {
    try {
        const response = await axios.get(`/api/v1/products/catalog`, {
            params: filters,
        });
        if (
            response.status === HttpStatusCode.Ok &&
            isPaginatedItemsResponse<Product>(response.data)
        ) {
            return {
                ...response.data,
                items: parseProducts(response.data.items),
            };
        }
        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const getBySearchParams = async (
    apiParams: URLSearchParams
): Promise<Product[] | Error> => {
    try {
        const response = await axios.get(`/api/v1/products?${apiParams}`);
        return getResultFromResponse(response);
    } catch (error) {
        return error as Error;
    }
};

const getResultFromResponse = (
    response: AxiosResponse<any, any>
): Product[] | Error => {
    if (response.status === HttpStatusCode.Ok) {
        if (Array.isArray(response.data)) {
            return parseProducts(response.data);
        }

        return parseProducts(response.data.items);
    }
    return createUnhandledError(response);
};

const getById = async (id: string): Promise<Product | Error> => {
    try {
        const response = await axios.get(`/api/v1/products/${id}`);
        if (response.status === HttpStatusCode.Ok) {
            return parseProduct(response.data);
        }

        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const productService = {
    getByApiQuery,
    getDiscountedProducts,
    getById,
    getCatalog,
};
export default productService;
