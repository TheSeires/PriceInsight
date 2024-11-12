import { NavigateFunction } from 'react-router-dom';
import { User } from '../models/dto/User';
import { LoginUserForm, LoginUserFormErrors } from '../models/dto/LoginUser';
import { createUnhandledError, getErrorFromResponse } from '../utilities/utils';
import { t } from 'i18next';
import { Dispatch, SetStateAction } from 'react';
import {
    RegisterUserForm,
    RegisterUserFormErrors,
} from '../models/dto/RegisterUser';
import axios, { AxiosError, HttpStatusCode } from 'axios';
import { toast } from 'react-toastify';

const login = async (
    loginForm: LoginUserForm,
    updateUserContext: (newUser: User | null) => void,
    navigate: NavigateFunction,
    successUrl: string
): Promise<void | Error> => {
    try {
        const response = await axios.post('/api/v1/auth/login', loginForm);
        if (response.status === HttpStatusCode.Ok) {
            const user = response.data as User;
            navigate(successUrl);
            updateUserContext(user);
            return;
        }
        return createUnhandledError(response);
    } catch (e) {
        if (
            e instanceof AxiosError &&
            e.status === HttpStatusCode.Unauthorized
        ) {
            const error = getErrorFromResponse(e);
            return error ? new Error(error) : (e as Error);
        }

        return e as Error;
    }
};

const validateLoginForm = (
    loginUserForm: LoginUserForm,
    setLoginFormErrors: Dispatch<SetStateAction<LoginUserFormErrors>>
): boolean => {
    const formErrors: LoginUserFormErrors = {};

    if (!loginUserForm.email) {
        formErrors.email = t('auth.common.validation.email.required');
    }

    if (!loginUserForm.password) {
        formErrors.password = t('auth.common.validation.password.required');
    }

    setLoginFormErrors(formErrors);
    return Object.keys(formErrors).length === 0;
};

const register = async (
    registerForm: RegisterUserForm,
    setError: Dispatch<SetStateAction<string>>,
    navigate: NavigateFunction,
    successUrl: string
): Promise<void | Error> => {
    try {
        const response = await axios.post(
            '/api/v1/auth/register',
            registerForm
        );
        if (response.status === HttpStatusCode.Ok) {
            setTimeout(
                () => toast.success(t('auth.registration.success')),
                500
            );
            setError('');
            navigate(successUrl);
            return;
        }

        return createUnhandledError(response);
    } catch (e) {
        if (e instanceof AxiosError && e.status === HttpStatusCode.Conflict) {
            const error = getErrorFromResponse(e);
            return error ? new Error(error) : (e as Error);
        }

        return e as Error;
    }
};

const validateRegisterForm = (
    registerForm: RegisterUserForm,
    setRegisterFormErrors: Dispatch<SetStateAction<RegisterUserFormErrors>>
): boolean => {
    const formErrors: RegisterUserFormErrors = {};

    if (!registerForm.username) {
        formErrors.username = t(
            'auth.registration.validation.username.required'
        );
    } else if (registerForm.username.length < 3) {
        formErrors.username = t('auth.registration.validation.username.length');
    }

    if (!registerForm.email) {
        formErrors.email = t('auth.common.validation.email.required');
    } else if (!/\S+@\S+\.\S+/.test(registerForm.email)) {
        formErrors.email = t(
            'auth.registration.validation.email.invalidFormat'
        );
    }

    if (!registerForm.password) {
        formErrors.password = t('auth.common.validation.password.required');
    } else if (registerForm.password.length < 8) {
        formErrors.password = t('auth.registration.validation.password.length');
    }

    if (registerForm.password !== registerForm.confirmPassword) {
        formErrors.password = t(
            'auth.registration.validation.password.mismatch'
        );
    }

    setRegisterFormErrors(formErrors);
    return Object.keys(formErrors).length === 0;
};

const logout = async (
    updateUserContext: (newUser: User | null) => void,
    onSuccess: () => void
): Promise<void | Error> => {
    try {
        const response = await axios.post('/api/v1/auth/logout');
        if (response.status === HttpStatusCode.Ok) {
            updateUserContext(null);
            onSuccess();
            return;
        }

        return createUnhandledError(response);
    } catch (e) {
        return e as Error;
    }
};

const authService = {
    login,
    validateLoginForm,
    register,
    validateRegisterForm,
    logout,
};

export default authService;
