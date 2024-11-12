import { ReactNode } from 'react';
import { useUserContext } from '../UserContextProvider/UserContextProvider';
import { User } from '../../models/dto/User';

const AuthorizeView = (props: {
    authorized?: (user: User) => ReactNode;
    notAuthorized?: ReactNode;
}) => {
    const { isAuthorized, isLoading, user } = useUserContext();

    if (isLoading) return null;

    if (isAuthorized && user) {
        return props.authorized ? props.authorized(user) : null;
    }

    return props.notAuthorized;
};

export default AuthorizeView;
