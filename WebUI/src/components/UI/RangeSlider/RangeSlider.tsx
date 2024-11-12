import { Range } from 'react-range';
import {
    Direction,
    IRenderMarkParams,
    IRenderThumbParams,
    IRenderTrackParams,
} from 'react-range/lib/types';
import * as React from 'react';

interface RangeSliderProps {
    label?: string;
    labelledBy?: string;
    values: number[];
    min: number;
    max: number;
    step: number;
    direction?: Direction;
    allowOverlap?: boolean;
    draggableTrack?: boolean;
    disabled?: boolean;
    rtl?: boolean;
    onChange: (values: number[]) => void;
    onFinalChange?: (values: number[]) => void;
    renderMark?: (params: IRenderMarkParams) => React.ReactNode;
    renderThumb?: (params: IRenderThumbParams) => React.ReactNode;
    renderTrack?: (params: IRenderTrackParams) => React.ReactNode;
}

const RangeSlider = (props: RangeSliderProps) => {
    const {
        renderTrack = ({ props, children }) => (
            <div
                {...props}
                style={{
                    ...props.style,
                    height: '6px',
                    width: '100%',
                    backgroundColor: 'var(--clr-base)',
                }}
            >
                {children}
            </div>
        ),
        renderThumb = ({ props }) => (
            <div
                {...props}
                key={props.key}
                style={{
                    ...props.style,
                    height: '20px',
                    width: '14px',
                    backgroundColor: 'var(--clr-primary)',
                    borderRadius: 'var(--border-radius)',
                }}
            />
        ),
    } = props;

    return (
        <Range {...props} renderTrack={renderTrack} renderThumb={renderThumb} />
    );
};

export default RangeSlider;
