import { useSearchParams } from 'react-router-dom';
import {
    isPaginatedItemsResponse,
    PaginatedItemsResponse,
} from '../../../models/dto/PaginatedItems.ts';
import { useCallback, useEffect, useState } from 'react';

interface UsePaginationProps<T> {
    defaultLimit?: number;
    useQueryState?: boolean;
    getItemsCallback: (
        page: number,
        limit: number
    ) => Promise<PaginatedItemsResponse<T> | undefined>;
}

interface UsePaginationReturn<T> {
    items: T[];
    currentPage: number;
    totalPages: number;
    totalItems: number;
    limit: number;
    isLoadingInitialPage: boolean;
    isLoadingPage: boolean;
    error: Error | null;
    handlePageChange: (newPage: number) => void;
    refreshItems: () => Promise<void>;
}

const usePagination = <T>({
    defaultLimit = 20,
    useQueryState = false,
    getItemsCallback,
}: UsePaginationProps<T>): UsePaginationReturn<T> => {
    const [items, setItems] = useState<T[]>([]);
    const [totalItems, setTotalItems] = useState(0);
    const [limit, setLimit] = useState(defaultLimit);
    const [isLoadingInitialPage, setIsLoadingInitialPage] = useState(true);
    const [isLoadingPage, setIsLoadingPage] = useState(true);
    const [error, setError] = useState<Error | null>(null);
    const [currentPageState, setCurrentPageState] = useState(1);

    const [searchParams, setSearchParams] = useSearchParams();
    const getPageFromQuery = () => Number(searchParams.get('page')) || 1;

    const currentPage = useQueryState ? getPageFromQuery() : currentPageState;
    const totalPages = Math.ceil(totalItems / limit);

    const fetchItems = useCallback(async () => {
        setIsLoadingPage(true);
        setError(null);

        try {
            const response = await getItemsCallback(currentPage, limit);
            if (!response || !isPaginatedItemsResponse<T>(response)) return;

            setItems(response.items);
            setTotalItems(response.totalItems);
            setLimit(Math.min(defaultLimit, response.limit));

            if (currentPage > Math.ceil(response.totalItems / response.limit)) {
                const lastPage = Math.max(
                    1,
                    Math.ceil(response.totalItems / response.limit)
                );
                if (
                    useQueryState &&
                    searchParams.get('page') !== String(lastPage)
                ) {
                    setSearchParams((prev) => ({
                        ...Object.fromEntries(prev),
                        page: String(lastPage),
                    }));
                } else {
                    setCurrentPageState(lastPage);
                }
            } else if (currentPage < 1) {
                if (useQueryState && searchParams.get('page') !== '1') {
                    setSearchParams((prev) => ({
                        ...Object.fromEntries(prev),
                        page: '1',
                    }));
                } else {
                    setCurrentPageState(1);
                }
            }
        } catch (err) {
            setError(
                err instanceof Error ? err : new Error('Failed to fetch items')
            );
        } finally {
            setIsLoadingPage(false);
            setIsLoadingInitialPage(false);
        }
    }, [
        currentPage,
        limit,
        defaultLimit,
        getItemsCallback,
        setSearchParams,
        useQueryState,
    ]);

    useEffect(() => {
        fetchItems();
    }, [fetchItems]);

    const handlePageChange = useCallback(
        (newPage: number) => {
            if (newPage < 1 || newPage > totalPages) return;

            if (useQueryState) {
                setSearchParams((prev) => ({
                    ...Object.fromEntries(prev),
                    page: String(newPage),
                }));
            } else {
                setCurrentPageState(newPage);
            }
        },
        [totalPages, useQueryState, setSearchParams]
    );

    return {
        items,
        currentPage,
        totalPages,
        totalItems,
        limit,
        isLoadingInitialPage,
        isLoadingPage,
        error,
        handlePageChange,
        refreshItems: fetchItems,
    };
};

export default usePagination;
