import { t } from 'i18next';
import authService from '../../services/authService';
import { useNavigate } from 'react-router-dom';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider';
import { useCallback } from 'react';
import ProductCard from '../../components/Cards/ProductCard.tsx';
import { Product } from '../../models/dto/Product.ts';
import Loader from '../../components/UI/Loader/Loader.tsx';
import NavigateTo from '../../components/NavigateTo/NavigateTo.tsx';
import { ArrowRightStartOnRectangleIcon } from '@heroicons/react/24/outline';
import { PaginatedItemsResponse } from '../../models/dto/PaginatedItems.ts';
import usePagination from '../../components/UI/Pagination/usePagination.ts';
import userService from '../../services/userService.ts';
import Pagination from '../../components/UI/Pagination/Pagination.tsx';

const PAGE_ITEMS_LIMIT = 20;
const EMPTY_PAGINATED_PRODUCTS = {
    items: [],
    totalItems: 0,
    limit: PAGE_ITEMS_LIMIT,
} as PaginatedItemsResponse<Product>;

const ProfilePage = () => {
    const navigate = useNavigate();
    const {
        user,
        isLoading: isFetchingUser,
        isAuthorized,
        updateUserContext,
    } = useUserContext();

    const logout = useCallback(async () => {
        await authService.logout(updateUserContext, () => navigate('/'));
    }, [updateUserContext, navigate]);

    const fetchFollowedProducts = useCallback(
        async (
            page: number,
            limit: number
        ): Promise<PaginatedItemsResponse<Product>> => {
            if (!user) return EMPTY_PAGINATED_PRODUCTS;

            const response = await userService.getFollowedProducts(
                user.id,
                page,
                limit
            );

            if (response instanceof Error) {
                console.error(response);
                return EMPTY_PAGINATED_PRODUCTS;
            }

            if (
                !['items', 'totalItems', 'limit'].every(
                    (key) => key in response
                )
            ) {
                return EMPTY_PAGINATED_PRODUCTS;
            }

            return response as PaginatedItemsResponse<Product>;
        },
        [user]
    );

    const {
        isLoadingInitialPage,
        items: followedProducts,
        currentPage,
        totalPages,
        handlePageChange,
    } = usePagination({
        defaultLimit: PAGE_ITEMS_LIMIT,
        useQueryState: true,
        getItemsCallback: fetchFollowedProducts,
    });

    if (isFetchingUser) return <Loader className="py-6" />;
    if (!user) return <NavigateTo path="/login" />;

    return (
        <div className="grid grid-cols-1 lg:grid-cols-[300px,1fr] gap-8">
            <div>
                <h3 className="text-xl font-bold text-start ms-2 mb-3">
                    {t('common.profile')}
                </h3>

                <div className="panel flex flex-col gap-y-5 p-5">
                    <input type="text" value={user.email} readOnly={true} />

                    <button
                        className="button-red !py-2 !px-3 me-auto !flex items-center gap-1"
                        onClick={logout}
                    >
                        <ArrowRightStartOnRectangleIcon className="size-5" />
                        {t('common.auth.logout')}
                    </button>
                </div>
            </div>

            <div>
                <h3 className="text-xl font-bold text-start ms-2 mb-3">
                    {t('common.productsYouFollow')}
                </h3>

                <div className="">
                    {isLoadingInitialPage ? (
                        <Loader className="py-6" />
                    ) : followedProducts.length === 0 ? (
                        <div className="text-center py-6">
                            {t('common.noFollowedProducts')}
                        </div>
                    ) : (
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6">
                            {followedProducts.map((product) => (
                                <ProductCard
                                    key={product.id}
                                    product={product}
                                    showFollowButton={isAuthorized}
                                />
                            ))}
                        </div>
                    )}
                </div>

                <Pagination
                    className="mt-6"
                    currentPage={currentPage}
                    totalPages={totalPages}
                    onPageChange={handlePageChange}
                />
            </div>
        </div>
    );
};

export default ProfilePage;
