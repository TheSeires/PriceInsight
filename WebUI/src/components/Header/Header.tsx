import styles from './Header.module.css';
import { buildClassName, hasUserRole } from '../../utilities/utils';
import { FC, useState } from 'react';
import {
    getCurrentLocale,
    supportedLocales,
    changeLanguage,
} from '../../services/i18n';
import Dropdown from '../UI/Dropdown/Dropdown';
import { NavLink, useLocation } from 'react-router-dom';
import AuthorizeView from '../AuthorizeView/AuthorizeView';
import { UserRoles } from '../../models/UserRoles';
import { Option } from 'react-dropdown';
import { Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';
import { t } from 'i18next';
import ThemeToggle from '../ThemeToggle/ThemeToggle.tsx';

interface HeaderProps {
    headerClass?: string;
}

const Header: FC<HeaderProps> = ({ headerClass }) => {
    const location = useLocation();
    const [isMenuOpen, setIsMenuOpen] = useState(false);

    const toggleMenu = () => {
        setIsMenuOpen((prev) => !prev);
    };

    const localeDropdownOptions = Object.entries(supportedLocales).map(
        ([key, value]) => ({
            value: key,
            label: value.title,
        })
    );

    const currentLocale = getCurrentLocale();
    const onLanguageChange = async (langOption: Option) => {
        const changed = await changeLanguage(langOption.value, true);
        if (changed) {
            window.location.reload();
        }
    };

    const isDiscountedActive = () => {
        const queryParams = new URLSearchParams(location.search);
        return queryParams.get('discounted') === 'true';
    };

    return (
        <div className={'relative flex flex-col'}>
            <header
                className={buildClassName(
                    'py-6 relative flex justify-between items-center max-w-screen-2xl w-full mx-auto z-[100]',
                    headerClass
                )}
            >
                <NavLink
                    className={(_) =>
                        'text-xl font-semibold tracking-wide clr-text hover-clr-text'
                    }
                    to="/"
                >
                    PriceInsight
                    <span
                        className={`clr-primary-dark ${styles['site-title']}`}
                    >
                        .
                    </span>
                </NavLink>

                <button
                    className="p-[0.6875rem] md:hidden z-10"
                    title={t('header.showNavMenu')}
                    onClick={toggleMenu}
                >
                    {isMenuOpen ? (
                        <XMarkIcon className="size-4" />
                    ) : (
                        <Bars3Icon className="size-4" />
                    )}
                </button>

                <nav
                    className={buildClassName(
                        'md:visible md:opacity-100 fixed left-0 w-full top-0 bottom-0 md-max:panel md:static transition-[bottom,visibility,opacity,padding] duration-700 md:text-sm md-max:overflow-hidden',
                        isMenuOpen
                            ? 'visible py-6'
                            : 'invisible bottom-full opacity-0'
                    )}
                >
                    <ul className="list-style-none flex flex-col md:flex-row gap-x-3 gap-y-4 items-center justify-end">
                        <li>
                            <NavLink
                                to="/catalog?discounted=true"
                                title={t('header.nav.deals')}
                                className={({ isActive }) => {
                                    return isActive && isDiscountedActive()
                                        ? 'block p-2 active'
                                        : 'block p-2';
                                }}
                            >
                                {t('header.nav.deals')}
                            </NavLink>
                        </li>

                        <li>
                            <NavLink
                                to="/catalog"
                                title={t('header.nav.catalog')}
                                className="block p-2"
                            >
                                {t('header.nav.catalog')}
                            </NavLink>
                        </li>

                        <AuthorizeView
                            authorized={(user) => (
                                <>
                                    <li>
                                        <NavLink
                                            to="/profile"
                                            title={t('common.profile')}
                                            className="block p-2"
                                        >
                                            {t('common.profile')}
                                        </NavLink>
                                    </li>

                                    {hasUserRole(user, UserRoles.admin) && (
                                        <li>
                                            <NavLink
                                                to="/administration"
                                                title={t(
                                                    'header.nav.administration'
                                                )}
                                                className="block p-2"
                                            >
                                                {t('header.nav.administration')}
                                            </NavLink>
                                        </li>
                                    )}
                                </>
                            )}
                            notAuthorized={
                                <li>
                                    <NavLink
                                        to="/login"
                                        title={t('common.auth.login')}
                                        className="block p-2"
                                    >
                                        {t('common.auth.login')}
                                    </NavLink>
                                </li>
                            }
                        />

                        <li>
                            <Dropdown
                                value={currentLocale}
                                controlClassName="min-w-[120px]"
                                options={localeDropdownOptions}
                                onChange={onLanguageChange}
                            />
                        </li>

                        <li>
                            <ThemeToggle />
                        </li>
                    </ul>
                </nav>
            </header>

            <div className="-z-10 min-h-[600px] absolute top-0 h-dvh w-dvw overflow-x-hidden left-1/2 -translate-x-1/2">
                <svg
                    className="absolute size-[500px] z-[-1] left-[calc((100vw-min(100vw-1rem,1536px))/2)] sm:left-[calc((100vw-min(100vw-3rem,1536px))/2)] top-0 translate-x-[-25%] translate-y-[-25%] opacity-40 blur-3xl rotate-90"
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="152.5 120.25 645.45 680.9"
                >
                    <defs>
                        <linearGradient
                            id="blobGradient"
                            gradientTransform="rotate(-45 .5 .5)"
                        >
                            <stop offset="0%" stopColor="#08AEEA"></stop>
                            <stop offset="100%" stopColor="#2AF598"></stop>
                        </linearGradient>
                        <clipPath id="blobClipPath">
                            <path
                                fill="currentColor"
                                d="M747.5 648Q671 796 497 801T209.5 653q-113.5-153-1-308T533 132.5q212-57.5 251.5 155t-37 360.5Z"
                            ></path>
                        </clipPath>
                    </defs>
                    <g clipPath="url(#blobClipPath)">
                        <path
                            fill="url(#blobGradient)"
                            d="M747.5 648Q671 796 497 801T209.5 653q-113.5-153-1-308T533 132.5q212-57.5 251.5 155t-37 360.5Z"
                        ></path>
                    </g>
                </svg>

                <svg
                    className="absolute size-[400px] z-[-1] right-[calc((100vw-min(100vw-1rem,1536px))/2)] sm:right-[calc((100vw-min(100vw-3rem,1536px))/2)] top-0 opacity-30 blur-3xl"
                    xmlns="http://www.w3.org/2000/svg"
                    viewBox="152.5 120.25 645.45 680.9"
                >
                    <defs>
                        <linearGradient
                            id="blobGradient"
                            gradientTransform="rotate(-45 .5 .5)"
                        >
                            <stop offset="0%" stopColor="#08AEEA"></stop>
                            <stop offset="100%" stopColor="#2AF598"></stop>
                        </linearGradient>
                        <clipPath id="blobClipPath">
                            <path
                                fill="currentColor"
                                d="M747.5 648Q671 796 497 801T209.5 653q-113.5-153-1-308T533 132.5q212-57.5 251.5 155t-37 360.5Z"
                            ></path>
                        </clipPath>
                    </defs>
                    <g clipPath="url(#blobClipPath)">
                        <path
                            fill="url(#blobGradient)"
                            d="M747.5 648Q671 796 497 801T209.5 653q-113.5-153-1-308T533 132.5q212-57.5 251.5 155t-37 360.5Z"
                        ></path>
                    </g>
                </svg>
            </div>
        </div>
    );
};

export default Header;
