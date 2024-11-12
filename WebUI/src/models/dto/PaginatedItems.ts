export type PaginatedItemsResponse<T> = {
    items: T[];
    totalItems: number;
    limit: number;
};

export const isPaginatedItemsResponse = <T>(
    data: any
): data is PaginatedItemsResponse<T> => {
    return 'items' in data && 'totalItems' in data && 'limit' in data;
};
