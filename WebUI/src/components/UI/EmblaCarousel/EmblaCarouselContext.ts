import { EmblaCarouselType } from 'embla-carousel';
import { EmblaViewportRefType } from 'embla-carousel-react';
import { createContext, useContext } from 'react';

export type EmblaCarouselContextType = {
    emblaApi: EmblaCarouselType | undefined;
    emblaRef: EmblaViewportRefType;
    isPrevButtonDisabled: boolean;
    isNextButtonDisabled: boolean;
    onPrevButtonClick: () => void;
    onNextButtonClick: () => void;
};

export const EmblaCarouselContext =
    createContext<EmblaCarouselContextType | null>(null);

export const useEmblaCarouselContext = (): EmblaCarouselContextType => {
    const context = useContext(EmblaCarouselContext);
    if (!context) {
        throw new Error('useEmblaContext must be used within an EmblaProvider');
    }
    return context;
};
