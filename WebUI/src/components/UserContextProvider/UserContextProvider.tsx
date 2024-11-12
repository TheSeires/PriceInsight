import {
    createContext,
    FC,
    ReactNode,
    useContext,
    useEffect,
    useState,
} from 'react';
import { User } from '../../models/dto/User';
import { setDelay } from '../../utilities/utils';
import { useLocation } from 'react-router-dom';
import useProvisionalState from '../../hooks/useProvisionalState.ts';
import userService from '../../services/userService.ts';

interface UserContextProps {
    user: User | null;
    isAuthorized: boolean;
    isLoading: boolean;
    updateUserContext: (newUser: User | null) => void;
    fetchUser: () => Promise<User | null>;
    fetchAndUpdateUser: () => Promise<void>;
    toggleProductFollowing: (productId: string) => Promise<void>;
    isFollowingProduct: (productId?: string) => boolean;
}

interface UserContextProviderProps {
    children: ReactNode;
}

const UserContext = createContext<UserContextProps | undefined>(undefined);

const UserContextProvider: FC<UserContextProviderProps> = ({ children }) => {
    const location = useLocation();
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [user, setUser] = useState<User | null>(null);
    const [isFirstLoad, setIsFirstLoad] = useState<boolean>(true);
    const [provisionalFollowingProductIds, setProvisionalFollowingProductIds] =
        useProvisionalState<string[]>([]);

    const isFollowingProduct = (productId?: string) => {
        if (!productId) return false;
        return provisionalFollowingProductIds.includes(productId);
    };

    const toggleProductFollowing = async (productId: string) => {
        if (!productId || !user) return;

        if (provisionalFollowingProductIds.includes(productId)) {
            setProvisionalFollowingProductIds(
                async (currentFollowingProductIds) => {
                    const removeResponse =
                        await userService.removeProductFollowing(
                            user.id,
                            productId
                        );

                    if (removeResponse instanceof Error || !removeResponse) {
                        return [...currentFollowingProductIds, productId];
                    }

                    return currentFollowingProductIds.filter(
                        (fpi) => fpi !== productId
                    );
                },
                (currentFollowingProductIds) =>
                    currentFollowingProductIds.filter((fpi) => fpi != productId)
            );
        } else {
            setProvisionalFollowingProductIds(
                async (currentFollowingProductIds) => {
                    const createResponse =
                        await userService.createProductFollowing(
                            user.id,
                            productId
                        );

                    if (createResponse instanceof Error || !createResponse) {
                        return currentFollowingProductIds.filter(
                            (fpi) => fpi != productId
                        );
                    }

                    return [...currentFollowingProductIds, productId];
                },
                (currentFollowingProductIds) => [
                    ...currentFollowingProductIds,
                    productId,
                ]
            );
        }
    };

    const setFollowingProductIds = async () => {
        if (!user) return;

        const result = await userService.getProductFollowings(user.id);
        if (result instanceof Error) {
            console.error(result);
            return;
        }

        setProvisionalFollowingProductIds(result.map((pf) => pf.productId));
    };

    const updateUserContext = (newUser: User | null) => {
        setUser(newUser);
    };

    const fetchUser = async (): Promise<User | null> => {
        const maxRetries = 3;
        const delay = 1000;

        for (let attempt = 0; attempt < maxRetries; attempt++) {
            try {
                const response = await fetch('/api/v1/auth/ping', {
                    method: 'GET',
                });
                if (response.ok) {
                    return (await response.json()) as User;
                } else if (response.status === 401) {
                    return null;
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            } catch (error) {
                console.error('Fetch error:', error);
                if (attempt === maxRetries - 1) return null;
                await setDelay(delay);
            }
        }
        return null;
    };

    const fetchAndUpdateUser = async () => {
        if (isFirstLoad) {
            setIsLoading(true);
        }

        const fetchedUser = await fetchUser();
        updateUserContext(fetchedUser);

        if (isFirstLoad) {
            setIsLoading(false);
            setIsFirstLoad(false);
        }
    };

    useEffect(() => {
        fetchAndUpdateUser();
    }, [location]);

    useEffect(() => {
        setFollowingProductIds();
    }, [user]);

    const contextValue: UserContextProps = {
        user,
        isAuthorized: !!user,
        isLoading,
        updateUserContext,
        fetchUser,
        fetchAndUpdateUser,
        toggleProductFollowing,
        isFollowingProduct,
    };

    return (
        <UserContext.Provider value={contextValue}>
            {children}
        </UserContext.Provider>
    );
};

export const useUserContext = (): UserContextProps => {
    const context = useContext(UserContext);
    if (!context) {
        throw new Error(
            'useUserContext must be used within a UserContextProvider'
        );
    }
    return context;
};

export default UserContextProvider;
