import React, { FC, ReactNode } from 'react';
import { buildClassName } from '../../../utilities/utils';
import styles from './Card.module.css';
import { useNavigate } from 'react-router-dom';

export interface CardProps {
    className?: string;
    imgUrl?: string;
    imgHref?: string;
    imgAlt?: string;
    href?: string;
    category?: string;
    title?: string;
    titleClass?: string;
    cardBodyClass?: string;
    children?: ReactNode;
    onClick?: () => void;
    isLoading?: boolean;
}

const Card: FC<CardProps> = ({
    className,
    imgUrl,
    imgAlt = 'Card image',
    href,
    category,
    title,
    titleClass,
    cardBodyClass,
    children,
    onClick,
    isLoading = false,
}: CardProps) => {
    const navigate = useNavigate();
    const handleClick = (e: React.MouseEvent<HTMLAnchorElement>) => {
        if (!href) {
            if (onClick) onClick();
            return;
        }

        e.preventDefault();
        return navigate(href);
    };

    return (
        <a
            className={buildClassName(
                styles['card'],
                'panel overflow-hidden h-full flex flex-col',
                className
            )}
            href={href}
            onClick={handleClick}
        >
            {imgUrl && (
                <div className="relative pb-[100%] mx-auto w-full border-b border-[var(--clr-border-light)] bg-gray-200 dark:bg-neutral-800">
                    {isLoading ? (
                        <div className="absolute inset-0 w-full h-full animate-pulse bg-gray-300 dark:bg-neutral-700" />
                    ) : (
                        <img
                            className="absolute left-0 top-0 w-full h-full"
                            src={imgUrl}
                            alt={imgAlt}
                        />
                    )}
                </div>
            )}
            <div
                className={buildClassName(
                    'py-2 px-2 text-start',
                    cardBodyClass
                )}
            >
                {category && (
                    <p
                        className={buildClassName(
                            styles['card-category'],
                            'text-[0.8125rem] line-clamp-1 mb-1',
                            isLoading ? 'w-24' : ''
                        )}
                    >
                        {isLoading ? (
                            <span className="inline-block w-full h-4 animate-pulse bg-gray-300 dark:bg-neutral-700 rounded" />
                        ) : (
                            category
                        )}
                    </p>
                )}
                {title && (
                    <h2
                        className={buildClassName(
                            'text-start text-[0.9375rem] font-medium line-clamp-2',
                            titleClass
                        )}
                    >
                        {isLoading ? (
                            <>
                                <span className="inline-block w-full h-4 animate-pulse bg-gray-300 dark:bg-neutral-700 rounded mb-1" />
                                <span className="inline-block w-2/3 h-4 animate-pulse bg-gray-300 dark:bg-neutral-700 rounded" />
                            </>
                        ) : (
                            title
                        )}
                    </h2>
                )}
                {children}
            </div>
        </a>
    );
};

export default Card;
