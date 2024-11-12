import { ReactNode } from 'react';
import { NextButton, PrevButton } from './EmblaArrowButtons';
import styles from './EmblaCarousel.module.css';
import { buildClassName, CSSInlineProps } from '../../../utilities/utils';
import { useEmblaCarouselContext } from './EmblaCarouselContext';
import useEmblaCarousel, { EmblaViewportRefType } from 'embla-carousel-react';
import { EmblaOptionsType } from 'embla-carousel';
import { usePrevNextButtons } from './usePrevNextButtons';

export interface EmblaCarouselProps<T> {
    emblaClass?: string;
    slideSize?: string;
    slideSpacing?: string;
    items: T[];
    itemContent: (item: T) => ReactNode;
    useCustomButtons?: boolean;
    options?: EmblaOptionsType;
}

export const EmblaCarousel = <T,>({
    emblaClass,
    slideSize = '25%',
    slideSpacing,
    items,
    itemContent,
    useCustomButtons = true,
    options = {
        loop: false,
        align: 'start',
        skipSnaps: true,
    },
}: EmblaCarouselProps<T>) => {
    const [emblaRef, emblaApi] = useEmblaCarousel(options);
    const {
        prevBtnDisabled,
        nextBtnDisabled,
        onPrevButtonClick,
        onNextButtonClick,
    } = usePrevNextButtons(emblaApi);

    return PureEmblaCarousel({
        emblaClass: emblaClass,
        slideSize,
        slideSpacing,
        items,
        itemContent,
        useCustomButtons,
        emblaRef,
        nextBtnDisabled,
        prevBtnDisabled,
        onNextButtonClick,
        onPrevButtonClick,
    });
};

export const ContextualEmblaCarousel = <T,>({
    emblaClass,
    slideSize = '25%',
    slideSpacing,
    items,
    itemContent,
    useCustomButtons = true,
}: EmblaCarouselProps<T>) => {
    const {
        emblaRef,
        onPrevButtonClick,
        onNextButtonClick,
        isPrevButtonDisabled: prevBtnDisabled,
        isNextButtonDisabled: nextBtnDisabled,
    } = useEmblaCarouselContext();

    return PureEmblaCarousel({
        emblaClass: emblaClass,
        slideSize,
        slideSpacing,
        items,
        itemContent,
        useCustomButtons,
        emblaRef,
        nextBtnDisabled,
        prevBtnDisabled,
        onNextButtonClick,
        onPrevButtonClick,
    });
};

export interface PureEmblaCarouselProps<T> extends EmblaCarouselProps<T> {
    emblaRef: EmblaViewportRefType;
    onPrevButtonClick: () => void;
    onNextButtonClick: () => void;
    prevBtnDisabled: boolean;
    nextBtnDisabled: boolean;
}

const PureEmblaCarousel = <T,>({
    emblaClass,
    slideSize,
    slideSpacing,
    items,
    itemContent,
    useCustomButtons,
    emblaRef,
    onPrevButtonClick,
    onNextButtonClick,
    prevBtnDisabled,
    nextBtnDisabled,
}: PureEmblaCarouselProps<T>) => {
    return (
        <div
            className={buildClassName(styles['embla'], emblaClass)}
            style={
                {
                    '--slide-size': `${slideSize}`,
                    '--slide-spacing': `${slideSpacing}`,
                } as CSSInlineProps
            }
        >
            <div className={styles['embla__viewport']} ref={emblaRef}>
                <div
                    className={buildClassName(
                        styles['embla__container'],
                        'py-6'
                    )}
                >
                    {items.map((item, index) => {
                        return (
                            <div key={index} className={styles['embla__slide']}>
                                {itemContent(item)}
                            </div>
                        );
                    })}
                </div>
            </div>

            {!useCustomButtons && (
                <div className={styles['embla__controls']}>
                    <div className={styles['embla__buttons']}>
                        <PrevButton
                            onClick={onPrevButtonClick}
                            disabled={prevBtnDisabled}
                        />
                        <NextButton
                            onClick={onNextButtonClick}
                            disabled={nextBtnDisabled}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};
