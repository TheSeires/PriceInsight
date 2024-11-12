import { t } from 'i18next';
import { Category } from '../../models/dto/Category';
import Card from '../UI/Card/Card';
import { useNavigate } from 'react-router-dom';
import { FC } from 'react';

interface CategoryCardProps {
    category: Category;
}

export const CategoryCard: FC<CategoryCardProps> = ({
    category,
}: CategoryCardProps) => {
    const categoryDisplayName = category.nameTranslationKey
        ? t(category.nameTranslationKey, category.name)
        : category.name;
    const navigate = useNavigate();

    return (
        <Card
            className="cursor-pointer px-2 text-[var(--clr-text)] hover:text-[var(--clr-text)]"
            title={categoryDisplayName}
            titleClass="line-clamp-2 text-sm"
            cardBodyClass="my-auto"
            onClick={() => navigate(`/catalog/?categoryId=${category.id}`)}
        />
    );
};
