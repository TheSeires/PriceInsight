import { FC, ReactNode } from 'react';
import { EmblaCarouselContext } from './EmblaCarouselContext';
import useEmblaCarousel from 'embla-carousel-react';
import { EmblaOptionsType } from 'embla-carousel';
import { usePrevNextButtons } from './usePrevNextButtons';

interface EmblaCarouselContextProviderProps {
    children: ReactNode;
    options?: EmblaOptionsType;
}

const EmblaCarouselContextProvider: FC<EmblaCarouselContextProviderProps> = ({
    children,
    options = {
        loop: false,
        align: 'start',
        skipSnaps: true,
    },
}) => {
    const [emblaRef, emblaApi] = useEmblaCarousel(options);
    const {
        prevBtnDisabled,
        nextBtnDisabled,
        onPrevButtonClick,
        onNextButtonClick,
    } = usePrevNextButtons(emblaApi);

    return (
        <EmblaCarouselContext.Provider
            value={{
                emblaApi,
                emblaRef,
                isPrevButtonDisabled: prevBtnDisabled,
                isNextButtonDisabled: nextBtnDisabled,
                onPrevButtonClick,
                onNextButtonClick,
            }}
        >
            {children}
        </EmblaCarouselContext.Provider>
    );
};

export default EmblaCarouselContextProvider;
