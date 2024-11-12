import { FC, ReactNode } from 'react';
import { User } from '../../models/dto/User';
import { useUserContext } from '../UserContextProvider/UserContextProvider';
import { hasUserRole } from '../../utilities/utils';
import { UserRole } from '../../models/UserRoles';

interface RoleViewProps {
    role: UserRole;
    hasRole?: (user: User) => ReactNode;
    hasNoRole?: ReactNode;
}

const RoleView: FC<RoleViewProps> = ({ role, hasRole, hasNoRole }) => {
    const { user, isLoading } = useUserContext();

    if (isLoading) return null;

    if (user && hasUserRole(user, role)) {
        return hasRole ? hasRole(user) : null;
    }

    return hasNoRole;
};

export default RoleView;
