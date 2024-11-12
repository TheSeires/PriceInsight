import { ContextualArrowButtons } from '../UI/EmblaCarousel/EmblaArrowButtons';
import {
    EmblaCarouselProps,
    ContextualEmblaCarousel,
} from '../UI/EmblaCarousel/EmblaCarousel';
import EmblaCarouselContextProvider from '../UI/EmblaCarousel/EmblaCarouselContextProvider';
import { ReactNode } from 'react';
import { EmblaOptionsType } from 'embla-carousel';

export interface SectionalCarousel<T> extends EmblaCarouselProps<T> {
    heading: string | undefined;
    customArrowsContent?: () => ReactNode;
    options?: EmblaOptionsType;
}

const SectionalCarousel = <T,>({
    heading,
    emblaClass,
    slideSize = '16.66%',
    slideSpacing = '1rem',
    items,
    itemContent,
    customArrowsContent,
    options = {
        loop: false,
        align: 'start',
        skipSnaps: true,
    },
}: SectionalCarousel<T>) => {
    return (
        <EmblaCarouselContextProvider options={options}>
            <div className="flex flex-wrap items-center justify-between gap-2 pb-2 border-b border-b-[var(--clr-border)]">
                <h2 className="text-2xl font-semibold">{heading}</h2>
                {customArrowsContent && customArrowsContent()}
                {!customArrowsContent && <ContextualArrowButtons />}
            </div>

            <ContextualEmblaCarousel<T>
                emblaClass={emblaClass}
                slideSize={slideSize}
                slideSpacing={slideSpacing}
                items={items}
                itemContent={itemContent}
                useCustomButtons={true}
            />
        </EmblaCarouselContextProvider>
    );
};

export default SectionalCarousel;
