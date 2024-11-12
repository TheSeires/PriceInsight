import React, { FC, FormEvent, useState } from 'react';
import {
    RegisterUserForm,
    RegisterUserFormErrors,
} from '../../models/dto/RegisterUser';
import FormInput from '../../components/UI/FormInput/FormInput';
import { Navigate, NavLink, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import authService from '../../services/authService';
import Loader from '../../components/UI/Loader/Loader.tsx';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider.tsx';

const RegistrationPage: FC = () => {
    const navigate = useNavigate();
    const { t } = useTranslation();
    const { isLoading, isAuthorized } = useUserContext();

    const [registerForm, setRegisterForm] = useState<RegisterUserForm>({
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
    });

    const [registerFormErrors, setRegisterFormErrors] =
        useState<RegisterUserFormErrors>({});
    const [commonFormError, setCommonFormError] = useState('');

    if (isLoading) return <Loader className="py-6" />;
    if (isAuthorized) return <Navigate to="/" />;

    const register = async () => {
        const registerResult = await authService.register(
            registerForm,
            setCommonFormError,
            navigate,
            '/login'
        );

        if (registerResult instanceof Error) {
            setCommonFormError(registerResult.message);
        } else {
            setCommonFormError('');
        }
    };

    const handleRegisterSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (
            !authService.validateRegisterForm(
                registerForm,
                setRegisterFormErrors
            )
        ) {
            return;
        }

        await register();
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setRegisterForm((prevState) => ({ ...prevState, [name]: value }));
    };

    return (
        <div className="max-w-md mx-auto mt-12 px-12 py-10 panel">
            <form className="flex flex-col" onSubmit={handleRegisterSubmit}>
                <FormInput
                    className="mb-4"
                    label={t('common.auth.username')}
                    type="text"
                    name="username"
                    placeholder="John"
                    onValueChange={handleInputChange}
                    validationError={registerFormErrors.username}
                />

                <FormInput
                    className="mb-4"
                    label="Email"
                    type="email"
                    name="email"
                    placeholder="john@domain.com"
                    onValueChange={handleInputChange}
                    validationError={registerFormErrors.email}
                />

                <FormInput
                    className="mb-4"
                    label={t('common.auth.password')}
                    type="password"
                    name="password"
                    placeholder="********"
                    onValueChange={handleInputChange}
                    validationError={registerFormErrors.password}
                />

                <FormInput
                    className="mb-6"
                    label={t('common.auth.confirmPassword')}
                    type="password"
                    name="confirmPassword"
                    placeholder="********"
                    onValueChange={handleInputChange}
                    validationError={registerFormErrors.password}
                />

                {!!commonFormError && (
                    <div className="text-start clr-error text-xs my-2">
                        * {commonFormError}
                    </div>
                )}

                <button className="mb-5" type="submit">
                    {t('common.auth.register')}
                </button>
            </form>

            <div className="flex items-center mb-3">
                <span className="w-full border border-t border-b-0 opacity-50"></span>
                <span className="font-medium px-5">{t('common.or')}</span>
                <span className="w-full border border-t border-b-0 opacity-50"></span>
            </div>

            <NavLink to="/login">{t('common.auth.login')}</NavLink>
        </div>
    );
};

export default RegistrationPage;
