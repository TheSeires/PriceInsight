import i18n from 'i18next';
import Backend from 'i18next-http-backend';
import { initReactI18next } from 'react-i18next';

const fetchLocalization = async (
    locale: string,
    changeLanguage: boolean = false,
    onLoadError: (e: any, response?: Response | null) => void = () => {}
) => {
    let response = null;

    try {
        response = await fetch(
            `/api/localization/${locale}?changeLanguage=${changeLanguage}`
        );

        if (response.status === 200) {
            const localizationJson = await response.json();

            if (localizationJson) {
                return localizationJson;
            }
            throw Error('Invalid response body for localization');
        }

        const error = await response.text();
        throw Error(error);
    } catch (error) {
        if (onLoadError) {
            onLoadError(error, response);
        }

        console.error(error);
    }

    return null;
};

const setCultureCookie = async (locale: string) => {
    try {
        await fetch(`/api/localization/set-culture?locale=${locale}`);
    } catch (error) {
        console.error(error);
    }
};

export const loadLocalization = async (
    locale: string,
    changeLanguage: boolean = false,
    onLoadError: (e: any, response?: Response | null) => void = () => {}
) => {
    const localization = await fetchLocalization(
        locale,
        changeLanguage,
        onLoadError
    );

    if (!localization) return;

    i18n.addResourceBundle(locale, 'translation', localization);
};

i18n.use(Backend)
    .use(initReactI18next)
    .init({
        debug: true,
        fallbackLng: 'en',
        interpolation: {
            escapeValue: false,
        },
    });

export const supportedLocales: SupportedLocales = {
    en: { title: 'English' },
    uk: { title: 'Українська' },
};

export const getCurrentLocale = () => i18n.resolvedLanguage ?? 'en';

export const isLocaleSupported = (locale: string): boolean => {
    return locale in supportedLocales;
};

export const changeLanguage = async (
    locale: string,
    updateLocaleCookie: boolean = false,
    ignoreIsSupportedCheck: boolean = false,
    onLoadError: (e: any, response?: Response | null) => void = () => {}
): Promise<boolean> => {
    if (!ignoreIsSupportedCheck && !isLocaleSupported(locale)) {
        return false;
    }

    if (!i18n.hasResourceBundle(locale, 'translation')) {
        await loadLocalization(locale, updateLocaleCookie, onLoadError);
    } else {
        await setCultureCookie(locale);
    }

    await i18n.changeLanguage(locale);
    return true;
};

type SupportedLocales = {
    [key: string]: {
        title: string;
    };
};

export default i18n;
