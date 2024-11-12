import { FC, FormEvent, useCallback, useEffect, useState } from 'react';
import { getErrorFromResponse } from '../../utilities/utils';
import Modal, { BaseModalProps } from '../UI/Modal/Modal';
import FormInput from '../UI/FormInput/FormInput';
import {
    ArrowLongDownIcon,
    ArrowLongRightIcon,
} from '@heroicons/react/24/outline';
import Select from '../UI/Select/Select';
import {
    ConvertToSelectOption,
    ConvertToSelectOptions,
    SelectOption,
} from '../../models/SelectOption';
import { Category } from '../../models/dto/Category';
import { CreateCategoryMappingRequest } from '../../models/dto/CategoryMapping';
import { t } from 'i18next';
import { CategorizationIssue } from '../../models/dto/CategorizationIssue';
import { MultiValue, SingleValue } from 'react-select';
import CreateCategoryModal from './CreateCategoryModal';

interface CreateCategoryMappingModalProps extends BaseModalProps {
    onCreated: () => void;
    onCategoryCreated?: () => void;
    categorizationIssue?: CategorizationIssue;
}

const CreateCategoryMappingModal: FC<CreateCategoryMappingModalProps> = ({
    isOpen,
    onClose,
    onCreated,
    categorizationIssue,
    onCategoryCreated,
}) => {
    const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
    const [categoryOptions, setCategoryOptions] = useState<
        SelectOption<Category>[]
    >([]);
    const [targetCategory, setTargetCategory] = useState<Category>();
    const [error, setError] = useState('');

    const fetchCategories = useCallback(async () => {
        try {
            if (!categorizationIssue) {
                setCategoryOptions([]);
                return;
            }

            const response = await fetch(
                `/api/v1/categories/?excludeMappedCategory=${categorizationIssue?.sourceCategory}`
            );

            if (response.status === 200) {
                const categories = (await response.json()) as Category[];
                setCategoryOptions(ConvertToSelectOptions(categories, 'name'));
                return;
            }

            const errorMessage = getErrorFromResponse(response);
            console.error(errorMessage);
        } catch (e) {
            console.error(e);
        }
    }, [categorizationIssue]);

    useEffect(() => {
        fetchCategories();
    }, [fetchCategories]);

    const handleOnCategoryCreated = useCallback(
        (category: Category) => {
            const categoryOption = ConvertToSelectOption(category, 'name');
            setCategoryOptions((prev) => [...prev, categoryOption]);
            if (onCategoryCreated) onCategoryCreated();
        },
        [onCategoryCreated]
    );

    const handleSetTargetCategory = (
        e:
            | SingleValue<SelectOption<Category>>
            | MultiValue<SelectOption<Category>>
    ) => {
        if (Array.isArray(e)) {
            return;
        }
        const singleValue = e as SingleValue<SelectOption<Category>>;
        setTargetCategory(singleValue?.value);
    };

    const openCategoryModal = useCallback(() => {
        setIsCategoryModalOpen(true);
    }, []);

    const closeCategoryModal = useCallback(() => {
        setIsCategoryModalOpen(false);
    }, []);

    const onCreateCategoryMappingSubmit = async (
        e: FormEvent<HTMLFormElement>
    ) => {
        e.preventDefault();
        setError('');

        if (!categorizationIssue) {
            setError('Selected categorization issue is unexpectedly null');
            return;
        }

        if (!targetCategory) {
            setError('Target category is unexpectedly null');
            return;
        }

        try {
            const response = await fetch('/api/v1/category-mappings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    sourceCategory: categorizationIssue.sourceCategory,
                    sourceMarketId: categorizationIssue.marketId,
                    targetCategoryId: targetCategory.id,
                } as CreateCategoryMappingRequest),
            });

            if (response.status === 200) {
                onClose();
                onCreated();
                return;
            }

            const errorMessage = await getErrorFromResponse(response);
            if (response.status === 400) {
                setError(
                    errorMessage ??
                        t('models.category.validation.nameIsTooLong')
                );
            }
            console.error(errorMessage);
        } catch (e) {
            console.error(e);
        }
    };

    return (
        <>
            <Modal
                isOpen={isOpen}
                header={t('common.createCategoryMapping')}
                onClose={onClose}
            >
                <form onSubmit={onCreateCategoryMappingSubmit}>
                    {error ? <p className="clr-error mb-3">* {error}</p> : null}

                    <div className="flex items-end justify-between gap-3 mb-6 max-[500px]:flex-col max-[500px]:items-start">
                        <FormInput
                            className="mb-0 min-w-[100px] w-full"
                            label={t('common.categoryOf', {
                                market: `'${categorizationIssue?.market?.name}'`,
                            })}
                        >
                            <input
                                className="min-w-[100px]"
                                defaultValue={
                                    categorizationIssue?.sourceCategory
                                }
                                type="text"
                                disabled={true}
                            />
                        </FormInput>

                        <ArrowLongRightIcon className="ms-auto me-auto size-8 flex-shrink-0 mb-[0.6rem] max-[500px]:hidden" />
                        <ArrowLongDownIcon className="hidden mx-auto size-8 flex-shrink-0 max-[500px]:block" />

                        <FormInput
                            className="mb-0 min-w-[100px] w-full"
                            label={t('common.targetCategory')}
                        >
                            <Select
                                options={categoryOptions}
                                onChange={(e) => handleSetTargetCategory(e)}
                            />
                        </FormInput>
                    </div>

                    <div className="flex gap-3 justify-between flex-wrap">
                        <button onClick={openCategoryModal} type="button">
                            {t('common.createCategory')}
                        </button>
                        <button type="submit">{t('common.submit')}</button>
                    </div>
                </form>
            </Modal>

            <CreateCategoryModal
                isOpen={isCategoryModalOpen}
                onClose={closeCategoryModal}
                onCreated={handleOnCategoryCreated}
            />
        </>
    );
};

export default CreateCategoryMappingModal;
