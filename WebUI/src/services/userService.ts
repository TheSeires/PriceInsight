import axios, { HttpStatusCode } from 'axios';
import { createUnhandledError } from '../utilities/utils.ts';
import {
    parseProductFollowing,
    parseProductFollowings,
    ProductFollowing,
} from '../models/dto/ProductFollowing.ts';
import { parseProducts, Product } from '../models/dto/Product.ts';
import {
    isPaginatedItemsResponse,
    PaginatedItemsResponse,
} from '../models/dto/PaginatedItems.ts';

const getFollowedProducts = async (
    userId: string,
    page?: number,
    limit?: number
): Promise<PaginatedItemsResponse<Product> | Product[] | Error> => {
    try {
        const response = await axios.get(
            `/api/v1/users/${userId}/followed-products/`,
            !!page && !!limit ? { params: { page, limit } } : undefined
        );

        if (response.status === HttpStatusCode.Ok) {
            const data = response.data;
            if (isPaginatedItemsResponse<Product>(data)) {
                return {
                    ...data,
                    items: parseProducts(data.items),
                };
            } else {
                if (Array.isArray(response.data)) {
                    return parseProducts(response.data);
                }

                return parseProducts(response.data.items);
            }
        }

        if (response.status === HttpStatusCode.NoContent) {
            return !!page && !!limit
                ? ({
                      items: [],
                      limit: 20,
                      totalItems: 0,
                  } as PaginatedItemsResponse<Product>)
                : [];
        }

        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const getProductFollowings = async (
    userId: string
): Promise<ProductFollowing[] | Error> => {
    try {
        const response = await axios.get(
            `/api/v1/users/${userId}/product-followings/`
        );
        if (response.status === HttpStatusCode.Ok) {
            return parseProductFollowings(response.data);
        }

        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const createProductFollowing = async (
    userId: string,
    productId: string
): Promise<ProductFollowing | Error> => {
    try {
        const response = await axios.post(
            `/api/v1/users/${userId}/product-followings/`,
            {
                productId,
            }
        );
        if (response.status === HttpStatusCode.Ok) {
            return parseProductFollowing(response.data);
        }

        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const removeProductFollowing = async (
    userId: string,
    productId: string
): Promise<boolean | Error> => {
    try {
        const response = await axios.delete(
            `/api/v1/users/${userId}/product-followings/`,
            {
                data: { productId },
            }
        );
        return response.status === HttpStatusCode.Ok;
    } catch (error) {
        return error as Error;
    }
};

const userService = {
    createProductFollowing,
    removeProductFollowing,
    getProductFollowings,
    getFollowedProducts,
};
export default userService;
