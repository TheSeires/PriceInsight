import axios, { AxiosError, HttpStatusCode } from 'axios';
import {
    Category,
    CreateCategoryRequest,
    UpdateCategoryRequest,
} from '../models/dto/Category';
import { createUnhandledError, getErrorFromResponse } from '../utilities/utils';
import { Dispatch, SetStateAction } from 'react';

const getAll = async (): Promise<Category[] | Error> => {
    try {
        const response = await axios.get(`/api/v1/categories/`);
        if (response.status === HttpStatusCode.Ok) {
            return response.data as Category[];
        }
        return createUnhandledError(response);
    } catch (e) {
        return e as Error;
    }
};

const create = async (
    createCategory: CreateCategoryRequest,
    setValidationError?: Dispatch<SetStateAction<string>>
): Promise<Category | Error> => {
    try {
        const response = await axios.post('/api/v1/categories', createCategory);
        if (response.status === HttpStatusCode.Created) {
            return response.data as Category;
        }
        if (setValidationError) {
            const error = getErrorFromResponse(response);
            if (error) setValidationError(error);
        }
        return createUnhandledError(response);
    } catch (e) {
        if (e instanceof AxiosError && e.status === HttpStatusCode.BadRequest) {
            const error = getErrorFromResponse(e);
            if (error && setValidationError) {
                setValidationError(error);
            }
            return error ? new Error(error) : (e as Error);
        }
        return e as Error;
    }
};

const update = async (
    updateCategory: UpdateCategoryRequest,
    setValidationError?: Dispatch<SetStateAction<string>>
): Promise<Category | Error> => {
    try {
        const response = await axios.put('/api/v1/categories', updateCategory);
        if (response.status === HttpStatusCode.Ok) {
            return response.data as Category;
        }
        if (setValidationError) {
            const error = getErrorFromResponse(response);
            if (error) setValidationError(error);
        }
        return createUnhandledError(response);
    } catch (e) {
        if (e instanceof AxiosError && e.status === HttpStatusCode.BadRequest) {
            const error = getErrorFromResponse(e);
            if (error && setValidationError) {
                setValidationError(error);
            }
            return error ? new Error(error) : (e as Error);
        }
        return e as Error;
    }
};

const categoryService = { getAll, create, update };
export default categoryService;
