import { useParams } from 'react-router-dom';
import NotFoundPage from '../NotFound/NotFoundPage.tsx';
import { useEffect, useMemo, useState } from 'react';
import { Product } from '../../models/dto/Product.ts';
import productService from '../../services/productService.ts';
import Loader from '../../components/UI/Loader/Loader.tsx';
import {
    buildClassName,
    defaultIfEmpty,
    formatDate,
} from '../../utilities/utils.ts';
import { DefaultImages } from '../../config/DefaultImages.ts';
import ProductPrice from '../../components/UI/ProductPrice/ProductPrice.tsx';
import { t } from 'i18next';
import { HeartIcon } from '@heroicons/react/24/outline';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider.tsx';

const ProductPage = () => {
    const { id } = useParams();
    const [found, setFound] = useState<boolean>(true);
    const [product, setProduct] = useState<Product>();
    const [attributes, setAttributes] = useState<
        [key: string, value: string][]
    >([]);
    const { toggleProductFollowing, isFollowingProduct } = useUserContext();

    const isFollowing = useMemo(
        () => isFollowingProduct(product?.id),
        [product, isFollowingProduct]
    );

    useEffect(() => {
        if (!id) {
            setFound(false);
            return;
        }

        const maybeFindAndSetProduct = async () => {
            const productResult = await productService.getById(id);

            if (productResult instanceof Error) {
                console.error(productResult.message);
                setFound(false);
            } else if (!productResult.priceEntries) {
                console.error('priceEntries is undefined');
                setFound(false);
            } else {
                setProduct(productResult);
                setAttributes(Object.entries(productResult.attributes));
            }
        };

        maybeFindAndSetProduct();
    }, [id, setProduct]);

    if (!id || !found) return <NotFoundPage />;

    return (
        <>
            {!product ? (
                <div className="my-12">
                    <Loader className="py-6" />
                </div>
            ) : (
                <>
                    <div className="grid md:grid-cols-[350px,1fr] gap-x-6 gap-y-6">
                        <div className="panel p-6 flex-grow">
                            <img
                                className="w-full h-full bg-gray-200 dark:bg-neutral-800 aspect-square max-w-[350px] m-auto"
                                src={defaultIfEmpty(
                                    product.imageUrl,
                                    DefaultImages.product
                                )}
                                alt={product.name}
                            />
                        </div>

                        <div className="panel p-6 text-start">
                            <h2 className="text-2xl font-bold mb-4">
                                {product.name}
                            </h2>

                            <button
                                className={buildClassName(
                                    'mb-9 !p-2 !flex items-center gap-1.5 group transition-all duration-300',
                                    isFollowing
                                        ? 'button-red'
                                        : 'button-primary'
                                )}
                                onClick={() =>
                                    toggleProductFollowing(product.id)
                                }
                            >
                                <HeartIcon
                                    className={buildClassName(
                                        'size-4 stroke-[0.125rem] stroke-[var(--clr-base)] transition-all duration-300',
                                        isFollowing
                                            ? 'fill-[var(--clr-base)] group-hover:fill-[transparent]'
                                            : 'group-hover:fill-[var(--clr-base)] fill-[transparent]'
                                    )}
                                />
                                <p className="text-sm font-semibold text-[var(--clr-base)]">
                                    {isFollowing
                                        ? t('common.following')
                                        : t('common.follow')}
                                </p>
                            </button>

                            <p className="mb-4 pb-1 font-bold border-b border-[var(--clr-border)]">
                                {t('common.marketPrices')}
                            </p>
                            <div className="flex flex-col gap-4">
                                {product.priceEntries &&
                                    product.priceEntries.map((priceEntry) => (
                                        <div
                                            key={priceEntry.id}
                                            className="grid grid-cols-[max-content,1fr] lg:grid-cols-[minmax(50px,60px),auto,auto,1fr] items-center gap-x-6 gap-y-2 [&:not(:last-child)]:pb-3 [&:not(:last-child)]:border-b [&:not(:last-child)]:border-[var(--clr-border)]"
                                        >
                                            <div className="bg-[var(--clr-base)] rounded-[var(--border-radius-x2)] max-w-[60px] overflow-hidden">
                                                <img
                                                    className="bg-white w-full h-full"
                                                    src={
                                                        priceEntry.market
                                                            ?.iconUrl
                                                    }
                                                    alt={
                                                        priceEntry.market?.name
                                                    }
                                                />
                                            </div>

                                            <div>
                                                <ProductPrice
                                                    priceEntry={priceEntry}
                                                />
                                            </div>

                                            <div>
                                                <p className="text-[0.9375rem] font-bold">
                                                    {t('common.updated')}:
                                                </p>
                                                <p className="text-[0.875rem]">
                                                    {formatDate(
                                                        priceEntry.lastUpdated
                                                    )}
                                                </p>
                                            </div>

                                            <div>
                                                <a
                                                    className="button-secondary text-center !font-semibold !text-sm"
                                                    target="_blank"
                                                    href={priceEntry.productUrl}
                                                >
                                                    {t('common.view')}
                                                </a>
                                            </div>
                                        </div>
                                    ))}
                            </div>
                        </div>
                    </div>

                    {attributes && attributes.length !== 0 && (
                        <>
                            <h4 className="mt-9 ms-5 text-start text-xl font-medium">
                                {t('common.attributes')}
                            </h4>

                            <div className="panel mt-3">
                                <ul className="max-w-6xl">
                                    {attributes.map(([key, value], index) => (
                                        <li
                                            key={key}
                                            className={buildClassName(
                                                'flex justify-between w-full text-sm text-start px-5 py-4',
                                                index !== 0
                                                    ? 'border-t dark:border-[var(--clr-border)] border-[var(--clr-border-light)]'
                                                    : ''
                                            )}
                                        >
                                            <span className="dark:text-neutral-400 text-gray-600 font-medium">
                                                {key}
                                            </span>
                                            <span className="sm:max-w-[200px] max-w-[100px] w-full lg:max-w-[400px] break-words">
                                                {value}
                                            </span>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        </>
                    )}
                </>
            )}
        </>
    );
};

export default ProductPage;
