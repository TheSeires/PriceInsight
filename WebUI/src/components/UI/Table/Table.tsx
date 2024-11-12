import {
    forwardRef,
    MouseEvent,
    ReactNode,
    Ref,
    useCallback,
    useImperativeHandle,
} from 'react';
import styles from './Table.module.css';
import { t } from 'i18next';
import { buildClassName } from '../../../utilities/utils';
import { PaginatedItemsResponse } from '../../../models/dto/PaginatedItems';
import { v4 as newUuid } from 'uuid';
import Pagination from '../Pagination/Pagination.tsx';
import usePagination from '../Pagination/usePagination.ts';
import Loader from '../Loader/Loader.tsx';

export interface TableColumn<T> {
    header: string;
    renderContent?: keyof T | ((item: T) => ReactNode);
    headerClassName?: string;
    bodyClassName?: string;
    onClick?: (item: T, e?: MouseEvent) => void;
}

interface TableProps<T> {
    containerClass?: string;
    bodyColumnClass?: string;
    columns: TableColumn<T>[];
    limit?: number;
    noItemsContent?: ReactNode;
    onItemsRequested: (
        page: number,
        limit: number
    ) => Promise<PaginatedItemsResponse<T>>;
    useQueryState?: boolean;
}

export interface TableRef {
    refreshItems: () => Promise<void>;
}

function Table<T>(props: TableProps<T>, ref: Ref<TableRef>) {
    const {
        containerClass,
        bodyColumnClass,
        columns,
        limit = 20,
        noItemsContent,
        onItemsRequested,
        useQueryState = false,
    } = props;

    const {
        items,
        currentPage,
        totalPages,
        isLoadingInitialPage,
        handlePageChange,
        refreshItems,
    } = usePagination({
        defaultLimit: limit,
        getItemsCallback: onItemsRequested,
        useQueryState,
    });

    const onColumnClick = useCallback(
        (e: MouseEvent, column: TableColumn<T>, item: T) => {
            if (column.onClick) column.onClick(item, e);
        },
        []
    );

    useImperativeHandle(ref, () => ({
        refreshItems,
    }));

    return (
        <div>
            <div
                className={buildClassName(
                    styles['table-container'],
                    containerClass
                )}
            >
                <table className={styles['table']}>
                    <thead>
                        <tr>
                            {columns.map((column) => (
                                <th
                                    key={newUuid()}
                                    className={column.headerClassName}
                                >
                                    {column.header}
                                </th>
                            ))}
                        </tr>
                    </thead>

                    <tbody>
                        {isLoadingInitialPage ? (
                            <tr>
                                <td
                                    colSpan={columns.length}
                                    className="text-center"
                                >
                                    <Loader className="py-6" />
                                </td>
                            </tr>
                        ) : items && items.length > 0 ? (
                            items.map((item) => (
                                <tr key={newUuid()}>
                                    {columns.map((column) => (
                                        <td
                                            key={newUuid()}
                                            className={buildClassName(
                                                bodyColumnClass,
                                                column.bodyClassName
                                            )}
                                            onClick={(e) =>
                                                onColumnClick(e, column, item)
                                            }
                                        >
                                            {column.renderContent
                                                ? typeof column.renderContent ===
                                                  'function'
                                                    ? column.renderContent(item)
                                                    : String(
                                                          item[
                                                              column
                                                                  .renderContent
                                                          ]
                                                      )
                                                : null}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        ) : (
                            (noItemsContent ?? (
                                <tr>
                                    <td className="!py-6" colSpan={5}>
                                        {t('common.noTableItems')}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <Pagination
                className="mt-3"
                currentPage={currentPage}
                totalPages={totalPages}
                onPageChange={handlePageChange}
            />
        </div>
    );
}

export default forwardRef(Table) as <T>(
    props: TableProps<T> & { ref?: Ref<TableRef> }
) => JSX.Element;
