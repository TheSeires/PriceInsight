import { Market } from "./Market";

export type CategorizationIssue = {
    id: string;
    sourceCategory: string,
    sourceProductUrl: string,
    marketId: string,
    market?: Market,
    markets?: Market[]
}