import {
    ApiQueryParams,
    MatchOperator,
    SortDirection,
} from '../models/ApiQueryParams';

export type MatchValueType = string | number | '[]' | 'true' | 'false';

export class ApiQueryBuilder<T> {
    private params: ApiQueryParams<T>;

    constructor() {
        this.params = {};
    }

    sort(sortBy: keyof T & string, sortDirection: SortDirection): this {
        this.params.sort = { sortBy, sortDirection };
        return this;
    }

    match(
        field: keyof T,
        operator: MatchOperator,
        value: MatchValueType
    ): this {
        if (!this.params.matchFilters) {
            this.params.matchFilters = [];
        }
        this.params.matchFilters.push({ field, operator, value });
        return this;
    }

    paginate(page: number, limit: number): this {
        this.params.pagination = { page, limit };
        return this;
    }

    build(): URLSearchParams {
        const params = new URLSearchParams();

        if (this.params.sort) {
            params.append('sortBy', this.params.sort.sortBy);
            params.append(
                'sortDirection',
                String(this.params.sort.sortDirection)
            );
        }

        if (this.params.pagination) {
            params.append('page', String(this.params.pagination.page));
            params.append('limit', String(this.params.pagination.limit));
        }

        if (this.params.matchFilters) {
            this.params.matchFilters.forEach((filter) => {
                params.append(
                    `match[${String(filter.field)}]`,
                    `${filter.operator}:${String(filter.value)}`
                );
            });
        }

        return params;
    }

    toString() {
        return this.build().toString();
    }
}
