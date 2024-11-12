import { FC } from 'react';
import styles from './Pagination.module.css';
import { ArrowLeftIcon, ArrowRightIcon } from '@heroicons/react/24/outline';
import { useTranslation } from 'react-i18next';
import { buildClassName } from '../../../utilities/utils.ts';

export interface PaginationProps {
    className?: string;
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

const Pagination: FC<PaginationProps> = (props: PaginationProps) => {
    const { t } = useTranslation();

    return (
        <div
            className={buildClassName(
                props.className,
                styles['pagination-container']
            )}
        >
            <button
                className={styles['button']}
                onClick={() => props.onPageChange(props.currentPage - 1)}
                disabled={props.currentPage === 1}
            >
                <ArrowLeftIcon className="size-4" />
            </button>

            <span className="mx-5">
                {t('common.paginationText', {
                    current: props.totalPages === 0 ? 0 : props.currentPage,
                    total: props.totalPages,
                })}
            </span>

            <button
                className={styles['button']}
                onClick={() => props.onPageChange(props.currentPage + 1)}
                disabled={props.currentPage >= props.totalPages}
            >
                <ArrowRightIcon className="size-4" />
            </button>
        </div>
    );
};

export default Pagination;
