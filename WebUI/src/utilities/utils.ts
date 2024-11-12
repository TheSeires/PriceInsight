/* eslint-disable @typescript-eslint/no-explicit-any */
import { CSSProperties } from 'react';
import { User } from '../models/dto/User';
import { UserRole } from '../models/UserRoles';
import { AxiosError, AxiosResponse } from 'axios';

export interface CSSInlineProps extends CSSProperties {
    [key: `--${string}`]: string | number;
}

export function formatDate(date: Date) {
    const day = date.getDate();
    const month = date.getMonth() + 1;
    const year = date.getFullYear();
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');

    return `${day}.${month}.${year} ${hours}:${minutes}`;
}

export function trimStart(value: string, searchString: string): string {
    if (value.startsWith(searchString)) {
        return value.slice(searchString.length);
    }
    return value;
}

export function trimEnd(value: string, searchString: string): string {
    if (value.endsWith(searchString)) {
        return value.slice(0, -searchString.length);
    }
    return value;
}

export function trimEdges(value: string, searchString: string): string {
    return trimEnd(trimStart(value, searchString), searchString);
}

export function defaultIfEmpty(
    value: string | undefined,
    defaultValue: string
): string {
    return value && value.trim() !== '' ? value : defaultValue;
}

export function setDelay(delay: number): Promise<unknown> {
    return new Promise((resolve) => setTimeout(resolve, delay));
}

export function buildClassName(
    ...strings: (string | null | undefined)[]
): string {
    const filteredStrings = strings.filter(
        (str): str is string => str != null && str.trim() !== ''
    );
    return filteredStrings.join(' ');
}

export function getCookieValue(cookieName: string): string | null {
    const cookies = document.cookie.split(';');
    for (const cookie of cookies) {
        const [key, value] = cookie.split('=');

        if (key.trim() === cookieName) {
            return value;
        }
    }

    return null;
}

export function getLocaleFromCookies(): string | null {
    const encodedUriLocale = getCookieValue('.AspNetCore.Culture');

    if (!encodedUriLocale) return null;

    const decodedUriLocale = decodeURIComponent(encodedUriLocale);
    const parts = decodedUriLocale.split('|');
    const cPart = parts.find((part) => part.startsWith('c='));
    const locale = cPart ? cPart.split('=')[1] : null;

    return locale;
}

export async function getErrorFromResponse(
    response: Response
): Promise<string | null>;
export function getErrorFromResponse(response: AxiosResponse): string | null;
export function getErrorFromResponse(error: AxiosError): string | null;
export function getErrorFromResponse(jsonResponse: any): string | null;

export function getErrorFromResponse(
    arg: Response | AxiosResponse | AxiosError | any
): Promise<string | null> | (string | null) {
    if (!arg) return null;

    const hasErrorProp = (obj: any): obj is { error: string } =>
        obj && typeof obj.error === 'string';

    if (arg instanceof Response) {
        return (async () => {
            try {
                const jsonResponse = await arg.json();

                if (Array.isArray(jsonResponse)) {
                    for (const item of jsonResponse) {
                        if (hasErrorProp(item)) {
                            return item.error;
                        }
                    }
                }

                return getErrorFromResponse(jsonResponse);
            } catch (e) {
                console.error(e);
                return null;
            }
        })();
    } else if (arg instanceof AxiosError) {
        const data = arg.response?.data;
        if (!data) return null;

        if (Array.isArray(data)) {
            for (const item of data) {
                if (hasErrorProp(item)) {
                    return item.error;
                }
            }
        } else if (hasErrorProp(data)) {
            return data.error;
        }
        return null;
    } else if ('data' in arg) {
        const data = arg.data;
        if (hasErrorProp(data)) {
            return data.error;
        }
        return null;
    } else {
        if (!hasErrorProp(arg)) {
            return null;
        }
        return arg.error;
    }
}

export function createUnhandledError(
    response: Response | AxiosResponse
): Error {
    const errorMessage = getErrorFromResponse(response);
    return new Error(
        `Unhandled error, status '${response.status}': ${errorMessage}`
    );
}

export function hasUserRole(user: User, role: UserRole): boolean {
    return user.roles.includes(role);
}

export function getLastUrlSegment(url: string) {
    const segments = url.split('/');
    return segments.pop() || segments.pop();
}

export function parseDateFields<T extends Record<string, any>>(
    obj: T,
    dateFields: (keyof T)[],
    innerFieldParsers?: ((item: T) => T)[]
): T {
    let result = { ...obj };

    for (const field of dateFields) {
        if (typeof obj[field] !== 'string') continue;

        const date = new Date(obj[field]);
        if (!isNaN(date.getTime())) {
            (result[field] as Date) = date;
        }
    }

    if (!innerFieldParsers) return result;

    for (const parser of innerFieldParsers) {
        result = parser(result);
    }

    return result;
}
