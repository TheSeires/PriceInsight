export type SelectOption<T> = {
    value: T;
    label: string;
};

export function ConvertToSelectOptions<T>(items: T[], propAsLabel: keyof T): SelectOption<T>[] {
    return items.map(item => ({
        value: item,
        label: String(item[propAsLabel])
    }));
}

export function ConvertToSelectOption<T>(item: T, propAsLabel: keyof T): SelectOption<T> {
    return {
        value: item,
        label: String(item[propAsLabel])
    };
}