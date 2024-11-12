import { FC, FormEvent, useCallback, useEffect, useState } from 'react';
import Modal from '../UI/Modal/Modal';
import { Category, UpdateCategoryRequest } from '../../models/dto/Category';
import FormInput from '../UI/FormInput/FormInput';
import {
    ConvertToSelectOptions,
    SelectOption,
} from '../../models/SelectOption';
import Select from '../UI/Select/Select';
import { MultiValue, SingleValue } from 'react-select';
import { t } from 'i18next';
import categoryService from '../../services/categoryService';

interface UpdateCategoryModalProps {
    isOpen: boolean;
    categoryToUpdate?: Category;
    onClose: () => void;
    onUpdated?: (category: Category) => void;
}

const emptyUpdateCategory = (): UpdateCategoryRequest => {
    return {
        id: '',
        name: '',
        parentId: '',
        nameTranslationKey: '',
        shortId: '',
    };
};

const UpdateCategoryModal: FC<UpdateCategoryModalProps> = ({
    isOpen,
    categoryToUpdate,
    onClose,
    onUpdated,
}) => {
    const [updateCategoryReq, setUpdateCategoryReq] =
        useState<UpdateCategoryRequest>(emptyUpdateCategory());
    const [selectedParentOption, setSelectedParentOption] =
        useState<SelectOption<Category> | null>(null);

    const [categoryOptions, setCategoryOptions] =
        useState<SelectOption<Category>[]>();
    const [error, setError] = useState('');

    const fetchCategories = useCallback(async () => {
        const categoriesResult = await categoryService.getAll();
        if (categoriesResult instanceof Error) {
            console.error(categoriesResult.message);
            return;
        }
        const options = ConvertToSelectOptions(
            categoriesResult,
            'name'
        ) as SelectOption<Category>[];
        setCategoryOptions(options);
    }, []);

    useEffect(() => {
        if (isOpen) {
            fetchCategories();
        }
    }, [isOpen, fetchCategories]);

    useEffect(() => {
        if (isOpen && categoryToUpdate) {
            setUpdateCategoryReq(categoryToUpdate);
            const parentOption = categoryOptions?.find(
                (x) => x.value.id === categoryToUpdate.parentId
            );
            setSelectedParentOption(parentOption ?? null);
        } else {
            setUpdateCategoryReq(emptyUpdateCategory());
            setSelectedParentOption(null);
            setError('');
        }
    }, [isOpen, categoryToUpdate, categoryOptions]);

    useEffect(() => {
        if (updateCategoryReq.parentId) {
            const selectedOption = categoryOptions?.find(
                (option) => option.value.id === updateCategoryReq.parentId
            );
            setSelectedParentOption(selectedOption ?? null);
        }
    }, [categoryOptions, updateCategoryReq.parentId]);

    const onUpdateCategorySubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError('');

        if (!updateCategoryReq.id) {
            const notSelectedError = 'Category is not selected';
            setError(notSelectedError);
            console.error(notSelectedError);
            return;
        }

        const response = await categoryService.update(
            updateCategoryReq,
            setError
        );
        if (response instanceof Error) {
            console.error(response);
            return;
        }

        onClose();
        if (onUpdated) onUpdated(response);
    };

    const handleSetParentCategory = (
        e:
            | SingleValue<SelectOption<Category>>
            | MultiValue<SelectOption<Category>>
    ) => {
        if (Array.isArray(e)) {
            return;
        }

        const singleValue = e as SingleValue<SelectOption<Category>>;
        setSelectedParentOption(singleValue);
        setUpdateCategoryReq((value) => ({
            ...value,
            parentId: singleValue?.value.id ?? '',
        }));
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;

        setUpdateCategoryReq((prevState) => ({
            ...prevState,
            [name]: value,
        }));
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            header={t('common.updateCategory')}
        >
            <form onSubmit={onUpdateCategorySubmit}>
                {error ? <p className="clr-error mb-3">* {error}</p> : null}

                <FormInput className="mb-4" label={t('models.category.name')}>
                    <input
                        name="name"
                        defaultValue={updateCategoryReq.name}
                        onChange={handleInputChange}
                        type="text"
                    />
                </FormInput>

                <FormInput
                    className="mb-4"
                    label={t('models.category.nameTranslationKey')}
                >
                    <input
                        name="nameTranslationKey"
                        defaultValue={updateCategoryReq.nameTranslationKey}
                        onChange={handleInputChange}
                        type="text"
                    />
                </FormInput>

                <FormInput
                    className="mb-4"
                    label={t('models.category.shortId')}
                >
                    <input
                        name="shortId"
                        defaultValue={updateCategoryReq.shortId}
                        onChange={handleInputChange}
                        type="text"
                    />
                </FormInput>

                <FormInput
                    className="mb-8"
                    label={`${t('models.category.parentId')} (${t('common.optionalField')})`}
                >
                    <Select
                        name="parentId"
                        isMulti={false}
                        isClearable={true}
                        value={selectedParentOption}
                        options={categoryOptions}
                        onChange={handleSetParentCategory}
                    />
                </FormInput>

                <div className="text-end">
                    <button type="submit">{t('common.submit')}</button>
                </div>
            </form>
        </Modal>
    );
};

export default UpdateCategoryModal;
