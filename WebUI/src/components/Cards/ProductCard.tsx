import { getLowestPriceEntry, Product } from '../../models/dto/Product.ts';
import Card from '../UI/Card/Card.tsx';
import { DefaultImages } from '../../config/DefaultImages.ts';
import { t } from 'i18next';
import { PriceEntry } from '../../models/dto/PriceEntry.ts';
import ProductPrice from '../UI/ProductPrice/ProductPrice.tsx';
import { useUserContext } from '../UserContextProvider/UserContextProvider.tsx';
import React, { useMemo } from 'react';
import { HeartIcon } from '@heroicons/react/24/outline';
import { buildClassName } from '../../utilities/utils.ts';

interface ProductCardProps {
    product: Product;
    showFollowButton?: boolean;
    isLoading?: boolean;
}

const ProductCard = ({
    product,
    showFollowButton = false,
    isLoading = false,
}: ProductCardProps) => {
    const lowestPriceEntry: PriceEntry | undefined =
        getLowestPriceEntry(product);

    const { toggleProductFollowing, isFollowingProduct } = useUserContext();
    const isFollowing = useMemo(
        () => isFollowingProduct(product.id),
        [product, isFollowingProduct]
    );

    const onToggleFollowClick = async (
        e: React.MouseEvent<HTMLButtonElement>
    ) => {
        e.preventDefault();
        e.stopPropagation();
        await toggleProductFollowing(product.id);
    };

    return (
        <Card
            className="cursor-pointer group/product-card"
            imgUrl={product.imageUrl ? product.imageUrl : DefaultImages.product}
            href={`/product/${product.id}`}
            title={product.name}
            titleClass="clr-text"
            category={
                product.category?.nameTranslationKey
                    ? t(product.category.nameTranslationKey)
                    : product.category?.name
            }
            cardBodyClass="flex-grow flex flex-col"
            isLoading={isLoading}
        >
            {(lowestPriceEntry || isLoading) && (
                <div className="flex flex-col flex-grow gap-y-0.5 justify-end mt-2">
                    {isLoading ? (
                        <div className="flex flex-col gap-1">
                            <div className="h-6 w-24 animate-pulse bg-gray-300 dark:bg-neutral-700 rounded" />
                            <div className="h-4 w-32 animate-pulse bg-gray-300 dark:bg-neutral-700 rounded" />
                        </div>
                    ) : (
                        lowestPriceEntry && (
                            <ProductPrice priceEntry={lowestPriceEntry} />
                        )
                    )}
                </div>
            )}

            {showFollowButton && !isLoading && (
                <button
                    title={
                        isFollowing ? t('common.unfollow') : t('common.follow')
                    }
                    className={buildClassName(
                        '!hidden group-hover/product-card:!inline-block transition-all duration-300 absolute top-1 right-1 !p-2 group/follow',
                        isFollowing ? 'button-red' : 'button-primary'
                    )}
                    onClick={onToggleFollowClick}
                >
                    <HeartIcon
                        className={buildClassName(
                            'size-5 stroke-[0.125rem] stroke-[var(--clr-base)] transition-all duration-300',
                            isFollowing
                                ? 'fill-[var(--clr-base)] group-hover/follow:fill-[transparent]'
                                : 'group-hover/follow:fill-[var(--clr-base)] fill-[transparent]'
                        )}
                    />
                </button>
            )}
        </Card>
    );
};

export default ProductCard;
