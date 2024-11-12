export type ApiQueryParams<T> = {
    pagination?: ApiPaginationParams;
    sort?: ApiSortParams;
    matchFilters?: MatchFilter<T>[];
};

export type ApiSortParams = {
    sortBy: string;
    sortDirection: SortDirection;
};

export const SortDirections = {
    asc: 'ascending',
    desc: 'descending',
} as const;

export type SortDirection =
    (typeof SortDirections)[keyof typeof SortDirections];

export type ApiPaginationParams = {
    page: number;
    limit: number;
};

export type MatchFilter<T> = {
    field: keyof T;
    operator: MatchOperator;
    value: unknown;
};

export const MatchOperators = {
    exists: 'exists',
    regex: 'regex',
    lt: 'lt',
    gt: 'gt',
    lte: 'lte',
    gte: 'gte',
    eq: 'eq',
    ne: 'ne',
    in: 'in',
} as const;

export type MatchOperator =
    (typeof MatchOperators)[keyof typeof MatchOperators];
