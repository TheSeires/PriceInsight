import { FC, useCallback, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import Table, { TableRef } from '../../components/UI/Table/Table';
import { CategorizationIssue } from '../../models/dto/CategorizationIssue';
import { getErrorFromResponse, getLastUrlSegment } from '../../utilities/utils';
import RoleView from '../../components/RoleView/RoleView';
import NavigateTo from '../../components/NavigateTo/NavigateTo';
import CreateCategoryModal from '../../components/Modals/CreateCategoryModal';
import { Category } from '../../models/dto/Category';
import CreateCategoryMappingModal from '../../components/Modals/CreateCategoryMappingModal';
import { PaginatedItemsResponse } from '../../models/dto/PaginatedItems';
import { UserRoles } from '../../models/UserRoles';
import UpdateCategoryModal from '../../components/Modals/UpdateCategoryModal';
import axios, { HttpStatusCode } from 'axios';
import { ApiQueryBuilder } from '../../utilities/apiQueryBuilder.ts';

const AdministrationPage: FC = () => {
    const { t } = useTranslation();
    const catIssuesTableRef = useRef<TableRef>(null);
    const categoriesTableRef = useRef<TableRef>(null);

    const [isCategoryMappingModalOpen, setIsCategoryMappingModalOpen] =
        useState(false);
    const [isCreateCategoryModalOpen, setIsCreateCategoryModalOpen] =
        useState(false);
    const [isUpdateCategoryModalOpen, setIsUpdateCategoryModalOpen] =
        useState(false);

    const [updateCategory, setUpdateCategory] = useState<Category>();
    const [selectedCategorizationIssue, setSelectedCategorizationIssue] =
        useState<CategorizationIssue>();

    const forceCrawling = useCallback(async () => {
        try {
            const confirmed = confirm(
                'Do you want to force start of crawling products from all available markets?'
            );
            if (!confirmed) return;

            const parseProductPageInfo = confirm(
                'Do you want to parse info on each product page? (This will take much more time)'
            );

            const result = await axios.get(
                `api/v1/services/force-start-crawling?parseProductPageInfo=${parseProductPageInfo}`
            );
            if (result.status == HttpStatusCode.Ok) {
                alert('Successfully forced to start crawling');
            }
        } catch (e) {
            const error = getErrorFromResponse(e);
            console.error(error ?? e);
        }
    }, []);

    const refreshCategorizationIssuesTable = () => {
        catIssuesTableRef?.current?.refreshItems();
    };

    const refreshCategoriesTable = () => {
        categoriesTableRef?.current?.refreshItems();
    };

    const openCategoryMappingModal = useCallback(
        (item: CategorizationIssue) => {
            if (!item) return;

            setSelectedCategorizationIssue(item);
            setIsCategoryMappingModalOpen(true);
        },
        []
    );

    const openUpdateCategoryModal = useCallback((item: Category) => {
        if (!item) return;

        setUpdateCategory(item);
        setIsUpdateCategoryModalOpen(true);
    }, []);

    const fetchCategorizationIssuesPage = useCallback(
        async (
            page: number,
            limit: number
        ): Promise<PaginatedItemsResponse<CategorizationIssue>> => {
            try {
                const params = new ApiQueryBuilder<CategorizationIssue>()
                    .paginate(page, limit)
                    .build();

                const response = await fetch(
                    `/api/v1/categorization-issues/?${params}`
                );

                if (response.status === 200) {
                    return (await response.json()) as PaginatedItemsResponse<CategorizationIssue>;
                }

                const error = await getErrorFromResponse(response);
                if (error) {
                    console.error(error);
                }
            } catch (e) {
                console.error(e);
            }

            return { items: [], totalItems: 0, limit: 0 };
        },
        []
    );

    const fetchCategoriesPage = useCallback(
        async (
            page: number,
            limit: number
        ): Promise<PaginatedItemsResponse<Category>> => {
            try {
                const params = new ApiQueryBuilder<CategorizationIssue>()
                    .paginate(page, limit)
                    .build();

                const response = await fetch(`/api/v1/categories/?${params}`);

                if (response.status === 200) {
                    return (await response.json()) as PaginatedItemsResponse<Category>;
                }

                const error = await getErrorFromResponse(response);
                if (error) {
                    console.error(error);
                }
            } catch (e) {
                console.error(e);
            }

            return { items: [], totalItems: 0, limit: 0 };
        },
        []
    );

    const renderUrlLink = useCallback(
        (item: CategorizationIssue) => (
            <a href={item.sourceProductUrl} target="_blank">
                /{getLastUrlSegment(item.sourceProductUrl)}
            </a>
        ),
        []
    );

    const renderCategorizeButton = useCallback(
        (item: CategorizationIssue) => (
            <button
                className="py-[0.4rem] px-[0.75rem]"
                onClick={() => openCategoryMappingModal(item)}
            >
                {t('common.categorize')}
            </button>
        ),
        [openCategoryMappingModal, t]
    );

    const renderUpdateCategoryButton = useCallback(
        (item: Category) => (
            <button
                className="py-[0.4rem] px-[0.75rem]"
                onClick={() => openUpdateCategoryModal(item)}
            >
                {t('common.update')}
            </button>
        ),
        [openUpdateCategoryModal, t]
    );

    const renderCategoryTranslatedName = useCallback(
        (item: Category) =>
            item.nameTranslationKey && t(item.nameTranslationKey),
        [t]
    );

    return (
        <RoleView
            role={UserRoles.admin}
            hasRole={() => (
                <>
                    <section className="mb-3">
                        <div className="text-start">
                            <button onClick={forceCrawling}>
                                {t('common.forceCrawling')}
                            </button>
                        </div>
                    </section>

                    <section>
                        <h3 className="uppercase text-2xl mb-6 font-semibold">
                            {t('common.categorizationIssues')}:
                        </h3>

                        <Table<CategorizationIssue>
                            ref={catIssuesTableRef}
                            containerClass="w-full"
                            bodyColumnClass="!py-2"
                            limit={10}
                            columns={[
                                {
                                    renderContent: 'sourceCategory',
                                    header: t(
                                        'models.categorizationIssue.sourceCategory'
                                    ),
                                },
                                {
                                    renderContent: (ci: CategorizationIssue) =>
                                        ci.market?.name,
                                    header: t(
                                        'models.categorizationIssue.marketName'
                                    ),
                                },
                                {
                                    renderContent: renderUrlLink,
                                    header: t(
                                        'models.categorizationIssue.sourceProductUrl'
                                    ),
                                    bodyClassName: 'text-start',
                                    headerClassName: 'text-start',
                                },
                                {
                                    renderContent: 'marketId',
                                    header: t(
                                        'models.categorizationIssue.marketId'
                                    ),
                                },
                                {
                                    renderContent: renderCategorizeButton,
                                    header: t('common.action'),
                                },
                            ]}
                            onItemsRequested={fetchCategorizationIssuesPage}
                        />
                    </section>

                    <section className="mt-24 mb-12">
                        <h3 className="uppercase text-2xl mb-3 font-semibold">
                            {t('common.categories')}:
                        </h3>

                        <div className="text-start mb-4">
                            <button
                                onClick={() =>
                                    setIsCreateCategoryModalOpen(true)
                                }
                                type="button"
                            >
                                {t('common.createCategory')}
                            </button>
                        </div>

                        <Table<Category>
                            ref={categoriesTableRef}
                            containerClass="w-full"
                            bodyColumnClass="!py-2"
                            limit={10}
                            columns={[
                                {
                                    renderContent: 'name',
                                    header: t('models.category.name'),
                                },
                                {
                                    renderContent: 'parentId',
                                    header: t('models.category.parentId'),
                                },
                                {
                                    renderContent: renderCategoryTranslatedName,
                                    header: t('models.category.translatedName'),
                                },
                                {
                                    renderContent: 'shortId',
                                    header: t('models.category.shortId'),
                                },
                                {
                                    renderContent: renderUpdateCategoryButton,
                                    header: t('common.action'),
                                },
                            ]}
                            onItemsRequested={fetchCategoriesPage}
                        />
                    </section>

                    <CreateCategoryMappingModal
                        isOpen={isCategoryMappingModalOpen}
                        onClose={() => setIsCategoryMappingModalOpen(false)}
                        onCreated={refreshCategorizationIssuesTable}
                        categorizationIssue={selectedCategorizationIssue}
                    />

                    <CreateCategoryModal
                        isOpen={isCreateCategoryModalOpen}
                        onClose={() => setIsCreateCategoryModalOpen(false)}
                        onCreated={refreshCategoriesTable}
                    />

                    <UpdateCategoryModal
                        categoryToUpdate={updateCategory}
                        isOpen={isUpdateCategoryModalOpen}
                        onClose={() => setIsUpdateCategoryModalOpen(false)}
                        onUpdated={refreshCategoriesTable}
                    />
                </>
            )}
            hasNoRole={<NavigateTo path="/" />}
        />
    );
};

export default AdministrationPage;
