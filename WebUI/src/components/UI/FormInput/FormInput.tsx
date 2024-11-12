import { ChangeEvent, HTMLInputTypeAttribute, ReactNode } from 'react';
import { buildClassName } from '../../../utilities/utils.ts';

interface FormInputProps {
    value?: string | number | undefined;
    onValueChange?: (e: ChangeEvent<HTMLInputElement>) => void;
    label?: string | ReactNode | undefined;
    labelFor?: string | undefined;
    labelClass?: string | undefined;
    placeholder?: string | undefined;
    name?: string | undefined;
    type?: HTMLInputTypeAttribute | undefined;
    className?: string | undefined;
    validationError?: string | undefined;
    children?: ReactNode;
}

const FormInput = (props: FormInputProps) => {
    return (
        <div
            className={
                'flex flex-col gap-1 mb-2' +
                (props.className ? ' ' + props.className : '')
            }
        >
            <label
                className={buildClassName('text-start', props.labelClass)}
                htmlFor={props.labelFor}
            >
                {props.label}
            </label>
            {props.children ? (
                props.children
            ) : (
                <input
                    className={props.validationError ? 'invalid' : ''}
                    defaultValue={props.value}
                    onChange={props.onValueChange}
                    name={props.name}
                    type={props.type ?? 'text'}
                    placeholder={props.placeholder}
                />
            )}
            {props.validationError && (
                <div className="text-start clr-error text-xs my-2">
                    * {props.validationError}
                </div>
            )}
        </div>
    );
};

export default FormInput;
