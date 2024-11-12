export interface CatalogFilters {
    search?: string;
    categoryId?: string;
    minPrice?: number;
    maxPrice?: number;
    discounted?: boolean;
    page?: number;
    limit?: number;
}
