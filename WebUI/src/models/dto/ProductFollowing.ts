import { parseDateFields } from '../../utilities/utils.ts';

export type ProductFollowing = {
    id: string;
    userId: string;
    productId: string;
    created: Date;
};

export const parseProductFollowing = (data: any): ProductFollowing => {
    return parseDateFields<ProductFollowing>(data, ['created']);
};

export const parseProductFollowings = (data: any[]): ProductFollowing[] => {
    return data.map(parseProductFollowing);
};
