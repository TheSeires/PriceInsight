import { useTranslation } from 'react-i18next';
import styles from './HomePageHeroSection.module.css';
import { buildClassName } from '../../utilities/utils';
import { useEffect, useState } from 'react';
import { Market } from '../../models/dto/Market.ts';
import marketService from '../../services/marketService.ts';

const HomePageHeroSection = () => {
    const { t } = useTranslation();
    const [markets, setMarkets] = useState<Market[]>([]);

    useEffect(() => {
        const fetchMarkets = async () => {
            const response = await marketService.getAll();
            if (response instanceof Error) {
                console.error(response);
                return;
            }

            setMarkets(response);
        };

        fetchMarkets();
    }, []);

    return (
        <section
            className={`h-[calc(100dvh-1.5rem)] max-h-[1080px] min-h-[600px] flex flex-col w-full grow relative`}
        >
            <div className="pt-[2.4rem] flex justify-center items-center grow">
                <div>
                    <h2 className="text-center sm:text-left text-[1.9em] sm:text-[2em] sm:leading-[1.2] lg:text-[2.5em] lg:leading-[1.1] xl:text-[3em] 2xl:text-[3.2em] font-semibold max-w-[610px] text-balance">
                        {t('header.hero.heading')}
                    </h2>
                    <p className="text-center sm:text-left mt-6 text-base lg:text-lg">
                        {t('header.hero.paragraph')}
                    </p>

                    <div className="mt-12 flex flex-wrap mx-auto sm:mx-0 justify-center sm:justify-start gap-2 max-w-[500px]">
                        {markets.map((market) => (
                            <div
                                key={market.id}
                                className="basis-[max(4rem,20%)] rounded-[var(--border-radius-x2)] bg-white overflow-hidden"
                            >
                                <img src={market.iconUrl} alt={market.name} />
                            </div>
                        ))}
                    </div>
                </div>

                <div className="relative hidden sm:block max-w-[500px] hero-img-container">
                    <img
                        className="hero-img w-full max-h-[60vh]"
                        src="/media/hero.png"
                        alt="PriceInsight"
                    />
                    <svg
                        className="absolute z-[-1] size-[100%] left-[50%] top-[50%] translate-x-[-50%] translate-y-[-40%] opacity-50 blur-2xl"
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

            <div className={styles['wave-container']}>
                <div
                    className={buildClassName(
                        styles['wave'],
                        'before:clip-path-wave before:clip-path-offset-32 before:bg-[var(--clr-series-primary-1)]'
                    )}
                ></div>
                <div
                    className={buildClassName(
                        styles['wave'],
                        'before:clip-path-wave before:clip-path-offset-28 before:bg-[var(--clr-series-primary-3)]'
                    )}
                ></div>
                <div
                    className={buildClassName(
                        styles['wave'],
                        'before:clip-path-wave before:clip-path-offset-16 before:bg-[var(--clr-series-primary-4)]'
                    )}
                ></div>
            </div>
        </section>
    );
};

export default HomePageHeroSection;
