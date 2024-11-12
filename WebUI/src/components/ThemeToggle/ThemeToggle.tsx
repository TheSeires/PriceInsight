import { useState, useEffect } from 'react';
import { MoonIcon, SunIcon } from '@heroicons/react/24/outline';

const ThemeToggle = () => {
    const [theme, setTheme] = useState(() => {
        const storedTheme = localStorage.getItem('theme');
        if (storedTheme) {
            return storedTheme;
        }

        const prefersDarkScheme = window.matchMedia(
            '(prefers-color-scheme: dark)'
        ).matches;
        return prefersDarkScheme ? 'dark' : 'light';
    });

    useEffect(() => {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }

        localStorage.setItem('theme', theme);
    }, [theme]);

    const toggleTheme = () => {
        setTheme((prevTheme) => (prevTheme === 'dark' ? 'light' : 'dark'));
    };

    return (
        <button
            onClick={toggleTheme}
            className="p-2 panel !border-[var(--clr-border)] !rounded-[var(--border-radius)]"
            title={
                theme === 'dark'
                    ? 'Switch to Light Mode'
                    : 'Switch to Dark Mode'
            }
        >
            {theme === 'dark' ? (
                <SunIcon className="size-4" />
            ) : (
                <MoonIcon className="size-4" />
            )}
        </button>
    );
};

export default ThemeToggle;
