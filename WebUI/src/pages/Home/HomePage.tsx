import { FC, useEffect, useState } from 'react';
import HomePageHeroSection from '../../components/HomePageHeroSection/HomePageHeroSection';
import { t } from 'i18next';
import SectionalCarousel from '../../components/Carousels/SectionalCarousel';
import { Category } from '../../models/dto/Category';
import categoryService from '../../services/categoryService';
import { CategoryCard } from '../../components/Cards/CategoryCard';
import productService from '../../services/productService';
import { Product } from '../../models/dto/Product';
import ProductCard from '../../components/Cards/ProductCard.tsx';
import { ContextualArrowButtons } from '../../components/UI/EmblaCarousel/EmblaArrowButtons.tsx';
import { ApiQueryBuilder } from '../../utilities/apiQueryBuilder.ts';
import { MatchOperators, SortDirections } from '../../models/ApiQueryParams.ts';
import sectionalCarouselStyles from '../../components/Carousels/SectionalCarousel.module.css';
import { useNavigate } from 'react-router-dom';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider.tsx';

const INITIAL_PAGE = 1;
const LIMIT_PER_PAGE = 20;
const ADDITIONAL_CATEGORY_IDS =
    import.meta.env.VITE_HOMEPAGE_CATEGORY_IDS.split(',') as string[];

const renderShowMoreFilteredProducts = (onClick: () => void) => (
    <div className="flex gap-x-4 gap-y-2 flex-wrap items-center">
        <button
            className="text-sm px-[0.625rem] py-[0.3125rem]"
            onClick={onClick}
        >
            {t('common.viewAll')}
        </button>
        <ContextualArrowButtons />
    </div>
);

const HomePage: FC = () => {
    const [categories, setCategories] = useState<Category[]>([]);
    const [newArrivals, setNewArrivals] = useState<Product[]>([]);
    const [discountedProducts, setDiscountedProducts] = useState<Product[]>([]);
    const [additionalCategoryProducts, setAdditionalCategoryProducts] =
        useState<Product[][]>([]);
    const { isAuthorized } = useUserContext();

    const navigate = useNavigate();

    useEffect(() => {
        const fetchData = async <T,>(
            fetcherFunc: () => Promise<T[] | Error>
        ) => {
            const result = await fetcherFunc();
            if (result instanceof Error) {
                console.error(result.message);
                return [];
            }
            return result;
        };

        const newArrivalsApiQuery = new ApiQueryBuilder<Product>()
            .sort('added', SortDirections.desc)
            .paginate(INITIAL_PAGE, LIMIT_PER_PAGE);

        const createCategoryQuery = (categoryId: string) =>
            new ApiQueryBuilder<Product>()
                .match('categoryId', MatchOperators.eq, categoryId)
                .paginate(INITIAL_PAGE, LIMIT_PER_PAGE);

        const fetchAllData = async () => {
            setCategories(await fetchData(categoryService.getAll));
            setNewArrivals(
                await fetchData(() =>
                    productService.getByApiQuery(newArrivalsApiQuery)
                )
            );
            setDiscountedProducts(
                await fetchData(() =>
                    productService.getDiscountedProducts(
                        INITIAL_PAGE,
                        LIMIT_PER_PAGE
                    )
                )
            );

            const categoryProducts = await Promise.all(
                ADDITIONAL_CATEGORY_IDS.map((categoryId) =>
                    fetchData(() =>
                        productService.getByApiQuery(
                            createCategoryQuery(categoryId)
                        )
                    )
                )
            );
            setAdditionalCategoryProducts(categoryProducts);
        };

        fetchAllData();
    }, []);

    return (
        <>
            <HomePageHeroSection />

            {categories && categories.length > 0 && (
                <section className="my-12">
                    <SectionalCarousel
                        slideSize="auto"
                        heading={t('common.categories')}
                        items={categories}
                        itemContent={(c) => <CategoryCard category={c} />}
                    />
                </section>
            )}

            {newArrivals && newArrivals.length > 0 && (
                <section className="my-12">
                    <SectionalCarousel
                        emblaClass={sectionalCarouselStyles['product-carousel']}
                        heading={t('common.newArrivals')}
                        items={newArrivals}
                        itemContent={(product) => (
                            <ProductCard
                                showFollowButton={isAuthorized}
                                product={product}
                            />
                        )}
                    />
                </section>
            )}

            {discountedProducts && discountedProducts.length > 0 && (
                <section className="my-12">
                    <SectionalCarousel
                        emblaClass={sectionalCarouselStyles['product-carousel']}
                        heading={t('common.discountedProducts')}
                        items={discountedProducts}
                        itemContent={(product) => (
                            <ProductCard
                                showFollowButton={isAuthorized}
                                product={product}
                            />
                        )}
                        customArrowsContent={() =>
                            renderShowMoreFilteredProducts(() =>
                                navigate(
                                    `/catalog?categoryId=${discountedProducts[0].categoryId}`
                                )
                            )
                        }
                    />
                </section>
            )}

            {additionalCategoryProducts &&
                additionalCategoryProducts.length > 0 &&
                additionalCategoryProducts.map((categoryProducts, index) => {
                    if (!categoryProducts || categoryProducts.length === 0)
                        return;

                    const categoryName = categoryProducts[0].category?.name;
                    const categoryId = categoryProducts[0].categoryId;
                    const translation =
                        categoryProducts[0].category?.nameTranslationKey;

                    return (
                        <section className="my-12" key={index}>
                            <SectionalCarousel
                                emblaClass={
                                    sectionalCarouselStyles['product-carousel']
                                }
                                heading={
                                    translation
                                        ? t(translation)
                                        : (categoryName ?? '')
                                }
                                items={categoryProducts}
                                itemContent={(product) => (
                                    <ProductCard
                                        showFollowButton={isAuthorized}
                                        product={product}
                                    />
                                )}
                                customArrowsContent={() =>
                                    renderShowMoreFilteredProducts(() =>
                                        navigate(
                                            `/catalog?categoryId=${categoryId}`
                                        )
                                    )
                                }
                            />
                        </section>
                    );
                })}
        </>
    );
};

export default HomePage;
