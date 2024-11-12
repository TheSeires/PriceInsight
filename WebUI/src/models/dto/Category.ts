export type Category = {
    id: string;
} & CreateCategoryRequest;

export type CreateCategoryRequest = {
    name: string;
    parentId: string;
    nameTranslationKey: string;
    shortId: string;
};

export type UpdateCategoryRequest = {} & Category & CreateCategoryRequest;
