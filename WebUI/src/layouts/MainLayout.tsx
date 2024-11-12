import { FC, ReactNode } from 'react';
import Header from '../components/Header/Header';

const MainLayout: FC<MainLayoutProps> = ({ children, headerClass }) => {
    return (
        <>
            <div className="px-4 sm:px-12">
                <div className="max-w-screen-2xl mx-auto">
                    <Header headerClass={headerClass} />
                    {children && <div className="my-6">{children}</div>}
                </div>
            </div>
        </>
    );
};

type MainLayoutProps = {
    children?: ReactNode;
    headerClass?: string;
};

export default MainLayout;
