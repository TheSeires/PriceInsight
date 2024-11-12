export type CategoryMapping = {
    id: string;
} & CreateCategoryMappingRequest;

export type CreateCategoryMappingRequest = {
    sourceMarketId: string;
    sourceCategory: string;
    targetCategoryId: string;
};