import { FC, FormEvent, useEffect, useState } from 'react';
import Modal from '../UI/Modal/Modal';
import { Category, CreateCategoryRequest } from '../../models/dto/Category';
import FormInput from '../UI/FormInput/FormInput';
import {
    ConvertToSelectOptions,
    SelectOption,
} from '../../models/SelectOption';
import Select from '../UI/Select/Select';
import { MultiValue, SingleValue } from 'react-select';
import categoryService from '../../services/categoryService';
import { t } from 'i18next';

interface CreateCategoryModalProps {
    isOpen: boolean;
    onClose: () => void;
    onCreated?: (category: Category) => void;
}

const CreateCategoryModal: FC<CreateCategoryModalProps> = ({
    isOpen,
    onClose,
    onCreated,
}) => {
    const [createCategoryReq, setCreateCategoryReq] =
        useState<CreateCategoryRequest>({
            name: '',
            parentId: '',
            nameTranslationKey: '',
            shortId: '',
        });
    const [categoryOptions, setCategoryOptions] =
        useState<SelectOption<Category>[]>();
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchCategories = async () => {
            const categoriesResult = await categoryService.getAll();
            if (categoriesResult instanceof Error) {
                console.error(categoriesResult.message);
                return;
            }

            setCategoryOptions(
                ConvertToSelectOptions(categoriesResult, 'name')
            );
        };

        fetchCategories();
    }, []);

    const onCreateCategorySubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError('');

        const response = await categoryService.create(
            createCategoryReq,
            setError
        );
        if (response instanceof Error) {
            console.error(response);
            return;
        }

        onClose();
        if (onCreated) onCreated(response);
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
        setCreateCategoryReq((value) => ({
            ...value,
            parentId: singleValue?.value.id ?? '',
        }));
    };

    const handleOnChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;

        setCreateCategoryReq((prevState) => ({
            ...prevState,
            [name]: value,
        }));
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            header={t('common.createCategory')}
        >
            <form onSubmit={onCreateCategorySubmit}>
                {error ? <p className="clr-error mb-3">* {error}</p> : null}

                <FormInput className="mb-4" label={t('models.category.name')}>
                    <input name="name" onChange={handleOnChange} type="text" />
                </FormInput>

                <FormInput
                    className="mb-4"
                    label={t('models.category.nameTranslationKey')}
                >
                    <input
                        name="nameTranslationKey"
                        onChange={handleOnChange}
                        type="text"
                    />
                </FormInput>

                <FormInput
                    className="mb-4"
                    label={t('models.category.shortId')}
                >
                    <input
                        name="shortId"
                        onChange={handleOnChange}
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

export default CreateCategoryModal;
