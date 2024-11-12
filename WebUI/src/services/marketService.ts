import axios, { HttpStatusCode } from 'axios';
import { createUnhandledError } from '../utilities/utils.ts';
import { Market } from '../models/dto/Market.ts';

const getAll = async () => {
    try {
        const response = await axios.get('api/v1/markets');
        if (response.status === HttpStatusCode.Ok) {
            return response.data as Market[];
        }

        return createUnhandledError(response);
    } catch (error) {
        return error as Error;
    }
};

const marketService = { getAll };
export default marketService;
