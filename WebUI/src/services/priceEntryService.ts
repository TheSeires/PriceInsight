import axios, { HttpStatusCode } from 'axios';
import { createUnhandledError } from '../utilities/utils.ts';
import { ApiQueryBuilder } from '../utilities/apiQueryBuilder.ts';
import { parsePriceEntry, PriceEntry } from '../models/dto/PriceEntry.ts';
import { SortDirections } from '../models/ApiQueryParams.ts';
import { isPaginatedItemsResponse } from '../models/dto/PaginatedItems.ts';

const getHighestPrice = async (): Promise<number | Error> => {
    try {
        const queryBuilder = new ApiQueryBuilder<PriceEntry>()
            .sort('price', SortDirections.desc)
            .paginate(1, 1)
            .build();

        const response = await axios.get(
            `/api/v1/price-entries/?${queryBuilder}`
        );
        if (
            response.status !== HttpStatusCode.Ok ||
            !isPaginatedItemsResponse<PriceEntry>(response.data)
        ) {
            return createUnhandledError(response);
        }

        const item = response.data.items[0];
        if (!item) return createUnhandledError(response);
        return parsePriceEntry(item).price;
    } catch (error) {
        return error as Error;
    }
};

const priceEntryService = {
    getHighestPrice,
};
export default priceEntryService;
