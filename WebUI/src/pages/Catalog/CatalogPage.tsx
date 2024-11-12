import React, { FC, useCallback, useEffect, useState } from 'react';
import productService from '../../services/productService.ts';
import categoryService from '../../services/categoryService.ts';
import { useSearchParams } from 'react-router-dom';
import { Category } from '../../models/dto/Category.ts';
import ProductCard from '../../components/Cards/ProductCard.tsx';
import { CatalogFilters } from '../../models/CatalogFilters.ts';
import { t } from 'i18next';
import Select from '../../components/UI/Select/Select.tsx';
import FormInput from '../../components/UI/FormInput/FormInput.tsx';
import {
    ConvertToSelectOption,
    ConvertToSelectOptions,
    SelectOption,
} from '../../models/SelectOption.ts';
import { MultiValue, SingleValue } from 'react-select';
import { MagnifyingGlassIcon } from '@heroicons/react/24/outline';
import Loader from '../../components/UI/Loader/Loader.tsx';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider.tsx';
import Pagination from '../../components/UI/Pagination/Pagination.tsx';
import usePagination from '../../components/UI/Pagination/usePagination.ts';
import priceEntryService from '../../services/priceEntryService.ts';
import RangeSlider from '../../components/UI/RangeSlider/RangeSlider.tsx';

const PAGE_ITEMS_LIMIT = 28;

const CatalogPage: FC = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const [categoryOptions, setCategoryOptions] =
        useState<SelectOption<Category>[]>();
    const [selectedCategory, setSelectedCategory] =
        useState<SelectOption<Category> | null>(null);
    const [maxPriceBound, setMaxPriceBound] = useState<number>(0);
    const { isAuthorized } = useUserContext();

    const filters: CatalogFilters = {
        search: searchParams.get('search') || undefined,
        categoryId: searchParams.get('categoryId') || undefined,
        minPrice: searchParams.get('minPrice')
            ? Number(searchParams.get('minPrice'))
            : undefined,
        maxPrice: searchParams.get('maxPrice')
            ? Number(searchParams.get('maxPrice'))
            : undefined,
        discounted: searchParams.get('discounted') === 'true',
    };

    const [searchInput, setSearchInput] = useState(filters?.search || '');
    const [minPrice, setMinPrice] = useState(filters?.minPrice || '0');
    const [maxPrice, setMaxPrice] = useState(filters?.maxPrice || '');

    const fetchProducts = useCallback(
        async (page: number, limit: number) => {
            try {
                const result = await productService.getCatalog({
                    ...filters,
                    page: page,
                    limit: limit,
                });

                if (result instanceof Error) {
                    console.error(result);
                    return;
                }

                return result;
            } catch (error) {
                console.error('Failed to fetch products:', error);
            }
        },
        [searchParams]
    );

    const {
        currentPage,
        totalPages,
        handlePageChange,
        items: products,
        isLoadingInitialPage,
        isLoadingPage,
    } = usePagination({
        defaultLimit: PAGE_ITEMS_LIMIT,
        useQueryState: true,
        getItemsCallback: fetchProducts,
    });

    useEffect(() => {
        const fetchCategories = async () => {
            const categoriesResult = await categoryService.getAll();
            if (categoriesResult instanceof Error) {
                console.error(categoriesResult.message);
                return;
            }
            const options = ConvertToSelectOptions(
                categoriesResult,
                'nameTranslationKey'
            ) as SelectOption<Category>[];
            setCategoryOptions(options);

            const category = filters.categoryId
                ? options.find((o) => o.value.id === filters.categoryId)?.value
                : null;

            setSelectedCategory(
                category
                    ? ConvertToSelectOption(category, 'nameTranslationKey')
                    : null
            );
        };

        const fetchHighestPriceBound = async () => {
            const result = await priceEntryService.getHighestPrice();
            if (result instanceof Error) {
                console.error(result.message);
                return;
            }

            setMaxPriceBound(result);
            setMaxPrice(result);
        };

        fetchCategories();
        fetchHighestPriceBound();
    }, []);

    const updateFilters = (newFilters: Partial<CatalogFilters>) => {
        const updatedFilters = { ...filters, ...newFilters };
        const cleanedUrlFilters = Object.fromEntries(
            Object.entries(updatedFilters).filter(
                ([_, value]) => value !== undefined && value !== ''
            )
        );

        setSearchParams(cleanedUrlFilters as Record<string, string>);
    };

    const handleSetCategory = (
        e:
            | SingleValue<SelectOption<Category>>
            | MultiValue<SelectOption<Category>>
    ) => {
        if (Array.isArray(e)) return;

        const singleValue = e as SingleValue<SelectOption<Category>>;
        setSelectedCategory(singleValue);
        updateFilters({ categoryId: singleValue?.value.id });
    };

    const handlePaginationParamChange = (
        e: React.ChangeEvent<HTMLInputElement>,
        param: 'minPrice' | 'maxPrice'
    ) => {
        const { value } = e.target;
        const valueNum = Number(value);
        const num = valueNum ? valueNum : undefined;

        updateFilters({
            [param]: num && num > 0 ? num : undefined,
        });
    };

    const handleInputEscapeOnKeyDown = (
        e: React.KeyboardEvent<HTMLInputElement>
    ) => {
        if (!(e.target instanceof HTMLInputElement)) return;

        if (e.key !== 'Enter' && e.key !== 'Escape') return;
        e.preventDefault();
        e.target.blur();
    };

    return (
        <>
            <div className="grid gap-x-6 gap-y-6 lg:grid-cols-[320px,1fr] text-start">
                <div>
                    <div className="panel px-4 py-4 z-[1] relative">
                        <div className="flex gap-x-2">
                            <input
                                className="min-w-0 max-w-[240px] w-full"
                                type="search"
                                placeholder={`${t('common.search')}...`}
                                value={searchInput}
                                onChange={(e) => setSearchInput(e.target.value)}
                                onBlur={() =>
                                    updateFilters({
                                        search: searchInput || undefined,
                                    })
                                }
                                onKeyDown={handleInputEscapeOnKeyDown}
                            />

                            <button className="button-primary h-[2.32rem] aspect-square !p-0 !flex justify-center items-center">
                                <MagnifyingGlassIcon className="size-5" />
                            </button>
                        </div>

                        <FormInput
                            className="mt-4 mb-4"
                            label={`${t('common.category')}`}
                        >
                            <Select
                                name="category"
                                isMulti={false}
                                isClearable={true}
                                value={selectedCategory}
                                getOptionLabel={(o) => t(o.label)}
                                options={categoryOptions}
                                onChange={handleSetCategory}
                            />
                        </FormInput>

                        <FormInput
                            className="mb-6 !flex-row-reverse justify-end gap-x-3 items-center"
                            label={`${t('common.discountedProducts')}`}
                            labelFor="discount-filter-checkbox"
                            labelClass="mt-[0.07rem]"
                        >
                            <input
                                id="discount-filter-checkbox"
                                type="checkbox"
                                checked={filters.discounted || false}
                                onChange={(e) =>
                                    updateFilters({
                                        discounted:
                                            e.target.checked || undefined,
                                    })
                                }
                            />
                        </FormInput>

                        <FormInput
                            className="mt-4 mb-8"
                            label={`${t('common.priceRange')}`}
                        >
                            <div className="mt-3 mb-4">
                                <RangeSlider
                                    values={[
                                        Number(minPrice),
                                        Number(maxPrice),
                                    ]}
                                    min={0}
                                    max={
                                        maxPriceBound || Number(maxPrice) || 100
                                    }
                                    onChange={(values) => {
                                        setMinPrice(values[0]);
                                        setMaxPrice(values[1]);
                                    }}
                                    onFinalChange={(values) =>
                                        updateFilters({
                                            minPrice: values[0],
                                            maxPrice: values[1],
                                        })
                                    }
                                    step={Math.floor(
                                        maxPriceBound ? maxPriceBound / 200 : 1
                                    )}
                                />
                            </div>

                            <div className="flex gap-2">
                                <input
                                    className="border p-2 rounded w-24"
                                    type="number"
                                    value={minPrice}
                                    onChange={(e) =>
                                        setMinPrice(e.target.value)
                                    }
                                    onBlur={(e) =>
                                        handlePaginationParamChange(
                                            e,
                                            'minPrice'
                                        )
                                    }
                                    onKeyDown={handleInputEscapeOnKeyDown}
                                />

                                <input
                                    className="border p-2 rounded w-24"
                                    type="number"
                                    value={maxPrice}
                                    max={maxPriceBound}
                                    onChange={(e) =>
                                        setMaxPrice(e.target.value)
                                    }
                                    onBlur={(e) =>
                                        handlePaginationParamChange(
                                            e,
                                            'maxPrice'
                                        )
                                    }
                                    onKeyDown={handleInputEscapeOnKeyDown}
                                />
                            </div>
                        </FormInput>
                    </div>
                </div>

                <div className="">
                    {isLoadingInitialPage ? (
                        <Loader className="py-6" />
                    ) : products.length === 0 ? (
                        <div className="text-center py-8">
                            {t('catalog.noProductsMatchingCriteria')}
                        </div>
                    ) : (
                        <>
                            <div className="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-6">
                                {products.map((product) => (
                                    <ProductCard
                                        key={product.id}
                                        showFollowButton={isAuthorized}
                                        product={product}
                                        isLoading={isLoadingPage}
                                    />
                                ))}
                            </div>

                            <Pagination
                                className="py-8"
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={handlePageChange}
                            />
                        </>
                    )}
                </div>
            </div>
        </>
    );
};

export default CatalogPage;
