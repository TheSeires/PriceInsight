import ReactSelect, {
    ActionMeta,
    GetOptionLabel,
    GroupBase,
    MultiValue,
    Options,
    OptionsOrGroups,
    PropsValue,
    SingleValue,
} from 'react-select';
import { buildClassName } from '../../../utilities/utils';
import styles from './Select.module.css';

export interface SelectProps<T> {
    className?: string;
    placeholder?: string;
    name?: string;
    options?: OptionsOrGroups<T, GroupBase<T>> | undefined;
    onChange?:
        | ((
              newValue: SingleValue<T> | MultiValue<T>,
              actionMeta: ActionMeta<T>
          ) => void)
        | undefined;
    isMulti?: boolean;
    value?: PropsValue<T>;
    defaultValue?: PropsValue<T>;
    isOptionSelected?: (option: T, selectValue: Options<T>) => boolean;
    getOptionLabel?: GetOptionLabel<T> | undefined;
    isClearable?: boolean;
}

export const Select = <T,>({
    className,
    placeholder = '',
    name,
    options,
    onChange,
    isMulti,
    value,
    defaultValue,
    isOptionSelected,
    getOptionLabel,
    isClearable,
}: SelectProps<T>) => {
    return (
        <div className={styles['select-component']}>
            <ReactSelect
                className={buildClassName(className)}
                isMulti={isMulti}
                isClearable={isClearable}
                classNamePrefix="select"
                getOptionLabel={getOptionLabel}
                name={name}
                placeholder={placeholder}
                value={value}
                defaultValue={defaultValue}
                isOptionSelected={isOptionSelected}
                options={options}
                onChange={onChange}
            />
        </div>
    );
};

export default Select;
