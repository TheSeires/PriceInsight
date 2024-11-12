import { ReactNode, useEffect, useState } from 'react';
import { changeLanguage } from '../../services/i18n';
import { getLocaleFromCookies } from '../../utilities/utils';

const navigatorLocale =
    navigator.languages && navigator.languages.length
        ? navigator.languages[0].split('-')[0]
        : navigator.language.split('-')[0];

const cookieLocale = getLocaleFromCookies();
const localeToUse = cookieLocale ? cookieLocale : navigatorLocale;

const LocalizationProvider = (props: {
    children: ReactNode;
    loadingTemplate?: ReactNode;
    failureTemplate?: ReactNode;
}) => {
    const [isLoading, setIsLoading] = useState(true);
    const [failedToLoad, setFailedToLoad] = useState(false);

    useEffect(() => {
        const initializeI18n = async () => {
            try {
                await changeLanguage(
                    localeToUse,
                    false,
                    true,
                    (error, response) => {
                        if (response?.redirected) {
                            location.reload();
                            return;
                        }

                        throw error;
                    }
                );
            } catch (error) {
                console.error('Failed to load localization:', error);
                setFailedToLoad(true);
            } finally {
                setIsLoading(false);
            }
        };

        initializeI18n();
    }, []);

    if (isLoading) {
        return props.loadingTemplate;
    }

    if (failedToLoad) {
        return props.failureTemplate;
    }

    return props.children;
};

export default LocalizationProvider;
