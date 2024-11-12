/** @type {import('tailwindcss').Config} */
export default {
    darkMode: 'class',
    content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
    theme: {
        extend: {
            keyframes: {
                'fade-in': {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                'fade-out': {
                    '0%': { opacity: '1' },
                    '100%': { opacity: '0' },
                },
                'scale-in': {
                    '0%': { transform: 'scale(0.95) translateY(10px)' },
                    '100%': { transform: 'scale(1) translateY(0)' },
                },
                'scale-out': {
                    '0%': { transform: 'scale(1) translateY(0)' },
                    '100%': { transform: 'scale(0.95) translateY(10px)' },
                },
            },
            animation: {
                'fade-in': 'fade-in 0.3s ease-out',
                'fade-out': 'fade-out 0.3s ease-in',
                'scale-in': 'scale-in 0.3s ease-out',
                'scale-out': 'scale-out 0.3s ease-in',
            },
        },
        screens: {
            '2xl-max': { max: '1535px' },
            'xl-max': { max: '1279px' },
            'lg-max': { max: '1023px' },
            'md-max': { max: '767px' },
            'sm-max': { max: '639px' },

            sm: '640px',
            md: '768px',
            lg: '1024px',
            xl: '1280px',
            '2xl': '1536px',
        },
    },
    plugins: [
        function ({ addBase, addUtilities, matchUtilities, theme }) {
            addBase({
                ':root': {
                    '--clip-path-offset': '0px',
                },
            });

            addUtilities({
                '.clip-path-wave': {
                    clipPath:
                        'polygon(100% 100%, 0% 100%, 0% calc(98% - var(--clip-path-offset) / 2), 1% calc(97.51% - var(--clip-path-offset) / 2), 2% calc(96.08% - var(--clip-path-offset) / 2), 3% calc(93.8% - var(--clip-path-offset) / 2), 4% calc(90.81% - var(--clip-path-offset) / 2), 5% calc(87.29% - var(--clip-path-offset) / 2), 6% calc(83.47% - var(--clip-path-offset) / 2), 7% calc(79.6% - var(--clip-path-offset) / 2), 8% calc(75.9% - var(--clip-path-offset) / 2), 9% calc(72.62% - var(--clip-path-offset) / 2), 10% calc(69.96% - var(--clip-path-offset) / 2), 11% calc(68.09% - var(--clip-path-offset) / 2), 12% calc(67.12% - var(--clip-path-offset) / 2), 13% calc(67.12% - var(--clip-path-offset) / 2), 14% calc(68.09% - var(--clip-path-offset) / 2), 15% calc(69.96% - var(--clip-path-offset) / 2), 16% calc(72.62% - var(--clip-path-offset) / 2), 17% calc(75.9% - var(--clip-path-offset) / 2), 18% calc(79.6% - var(--clip-path-offset) / 2), 19% calc(83.47% - var(--clip-path-offset) / 2), 20% calc(87.29% - var(--clip-path-offset) / 2), 21% calc(90.81% - var(--clip-path-offset) / 2), 22% calc(93.8% - var(--clip-path-offset) / 2), 23% calc(96.08% - var(--clip-path-offset) / 2), 24% calc(97.51% - var(--clip-path-offset) / 2), 25% calc(98% - var(--clip-path-offset) / 2), 26% calc(97.51% - var(--clip-path-offset) / 2), 27% calc(96.08% - var(--clip-path-offset) / 2), 28% calc(93.8% - var(--clip-path-offset) / 2), 29% calc(90.81% - var(--clip-path-offset) / 2), 30% calc(87.29% - var(--clip-path-offset) / 2), 31% calc(83.47% - var(--clip-path-offset) / 2), 32% calc(79.6% - var(--clip-path-offset) / 2), 33% calc(75.9% - var(--clip-path-offset) / 2), 34% calc(72.62% - var(--clip-path-offset) / 2), 35% calc(69.96% - var(--clip-path-offset) / 2), 36% calc(68.09% - var(--clip-path-offset) / 2), 37% calc(67.12% - var(--clip-path-offset) / 2), 38% calc(67.12% - var(--clip-path-offset) / 2), 39% calc(68.09% - var(--clip-path-offset) / 2), 40% calc(69.96% - var(--clip-path-offset) / 2), 41% calc(72.62% - var(--clip-path-offset) / 2), 42% calc(75.9% - var(--clip-path-offset) / 2), 43% calc(79.6% - var(--clip-path-offset) / 2), 44% calc(83.47% - var(--clip-path-offset) / 2), 45% calc(87.29% - var(--clip-path-offset) / 2), 46% calc(90.81% - var(--clip-path-offset) / 2), 47% calc(93.8% - var(--clip-path-offset) / 2), 48% calc(96.08% - var(--clip-path-offset) / 2), 49% calc(97.51% - var(--clip-path-offset) / 2), 50% calc(98% - var(--clip-path-offset) / 2), 51% calc(97.51% - var(--clip-path-offset) / 2), 52% calc(96.08% - var(--clip-path-offset) / 2), 53% calc(93.8% - var(--clip-path-offset) / 2), 54% calc(90.81% - var(--clip-path-offset) / 2), 55% calc(87.29% - var(--clip-path-offset) / 2), 56% calc(83.47% - var(--clip-path-offset) / 2), 57% calc(79.6% - var(--clip-path-offset) / 2), 58% calc(75.9% - var(--clip-path-offset) / 2), 59% calc(72.62% - var(--clip-path-offset) / 2), 60% calc(69.96% - var(--clip-path-offset) / 2), 61% calc(68.09% - var(--clip-path-offset) / 2), 62% calc(67.12% - var(--clip-path-offset) / 2), 63% calc(67.12% - var(--clip-path-offset) / 2), 64% calc(68.09% - var(--clip-path-offset) / 2), 65% calc(69.96% - var(--clip-path-offset) / 2), 66% calc(72.62% - var(--clip-path-offset) / 2), 67% calc(75.9% - var(--clip-path-offset) / 2), 68% calc(79.6% - var(--clip-path-offset) / 2), 69% calc(83.47% - var(--clip-path-offset) / 2), 70% calc(87.29% - var(--clip-path-offset) / 2), 71% calc(90.81% - var(--clip-path-offset) / 2), 72% calc(93.8% - var(--clip-path-offset) / 2), 73% calc(96.08% - var(--clip-path-offset) / 2), 74% calc(97.51% - var(--clip-path-offset) / 2), 75% calc(98% - var(--clip-path-offset) / 2), 76% calc(97.51% - var(--clip-path-offset) / 2), 77% calc(96.08% - var(--clip-path-offset) / 2), 78% calc(93.8% - var(--clip-path-offset) / 2), 79% calc(90.81% - var(--clip-path-offset) / 2), 80% calc(87.29% - var(--clip-path-offset) / 2), 81% calc(83.47% - var(--clip-path-offset) / 2), 82% calc(79.6% - var(--clip-path-offset) / 2), 83% calc(75.9% - var(--clip-path-offset) / 2), 84% calc(72.62% - var(--clip-path-offset) / 2), 85% calc(69.96% - var(--clip-path-offset) / 2), 86% calc(68.09% - var(--clip-path-offset) / 2), 87% calc(67.12% - var(--clip-path-offset) / 2), 88% calc(67.12% - var(--clip-path-offset) / 2), 89% calc(68.09% - var(--clip-path-offset) / 2), 90% calc(69.96% - var(--clip-path-offset) / 2), 91% calc(72.62% - var(--clip-path-offset) / 2), 92% calc(75.9% - var(--clip-path-offset) / 2), 93% calc(79.6% - var(--clip-path-offset) / 2), 94% calc(83.47% - var(--clip-path-offset) / 2), 95% calc(87.29% - var(--clip-path-offset) / 2), 96% calc(90.81% - var(--clip-path-offset) / 2), 97% calc(93.8% - var(--clip-path-offset) / 2), 98% calc(96.08% - var(--clip-path-offset) / 2), 99% calc(97.51% - var(--clip-path-offset) / 2), 100% calc(98% - var(--clip-path-offset) / 2))',
                },
                '.panel': {
                    boxShadow: 'var(--box-shadow)',
                    backgroundColor: 'var(--clr-panel)',
                    border: '1px solid var(--clr-border-light)',
                    borderRadius: 'calc(var(--border-radius) * 2)',
                    backdropFilter: 'var(--backdrop-filter-glassmorphism)',
                    WebkitBackdropFilter:
                        'var(--backdrop-filter-glassmorphism)',
                },
            });

            matchUtilities(
                {
                    'clip-path-offset': (value) => ({
                        '--clip-path-offset': value,
                    }),
                },
                { values: theme('spacing') }
            );
        },
    ],
};
