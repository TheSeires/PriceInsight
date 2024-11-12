import { useMemo } from 'react';
import { PriceEntry, splitPrice } from '../../../models/dto/PriceEntry.ts';
import { getDiscountPercentage } from '../../../models/dto/Product.ts';

export interface ProductPriceProps {
    priceEntry: PriceEntry;
}

const ProductPrice = ({ priceEntry }: ProductPriceProps) => {
    if (priceEntry.discountedPrice) {
        const [oldPriceIntegerPart, oldPriceDecimalPart] = useMemo(
            () => splitPrice(priceEntry.price),
            [priceEntry.price]
        );

        const [discountedPriceIntegerPart, discountedPriceDecimalPart] =
            useMemo(
                () => splitPrice(priceEntry.discountedPrice),
                [priceEntry.discountedPrice]
            );

        return (
            <>
                <div className="flex flex-wrap gap-x-2 items-center">
                    <data
                        value={priceEntry.price}
                        className="product-price product-price--old"
                    >
                        <span>
                            {oldPriceIntegerPart}.
                            <sup className="product-price--decimal-part">
                                {oldPriceDecimalPart}
                            </sup>
                        </span>
                    </data>

                    <span className="badge badge--red mb-0.5">
                        -
                        {getDiscountPercentage(
                            priceEntry.price,
                            priceEntry.discountedPrice
                        )}
                        %
                    </span>
                </div>

                <data
                    value={priceEntry.discountedPrice}
                    className="product-price product-price--actual"
                >
                    <span>
                        {discountedPriceIntegerPart}.
                        <sup className="product-price--decimal-part">
                            {discountedPriceDecimalPart}
                        </sup>
                    </span>
                    <abbr title="Гривня">грн</abbr>
                </data>
            </>
        );
    } else {
        const [priceIntegerPart, priceDecimalPart] = splitPrice(
            priceEntry.price
        );

        return (
            <data value={priceEntry.price} className="product-price">
                <span>
                    {priceIntegerPart}.
                    <sup className="product-price--decimal-part">
                        {priceDecimalPart}
                    </sup>
                </span>
                <abbr title="Гривня">грн</abbr>
            </data>
        );
    }
};

export default ProductPrice;
