import { ChangeEvent, FC, FormEvent, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { LoginUserForm, LoginUserFormErrors } from '../../models/dto/LoginUser';
import { NavLink, useNavigate } from 'react-router-dom';
import FormInput from '../../components/UI/FormInput/FormInput';
import { useUserContext } from '../../components/UserContextProvider/UserContextProvider';
import authService from '../../services/authService';
import NavigateTo from '../../components/NavigateTo/NavigateTo.tsx';
import Loader from '../../components/UI/Loader/Loader.tsx';

const LoginPage: FC = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const emptyUser: LoginUserForm = { email: '', password: '' };
    const { isLoading, isAuthorized, updateUserContext } = useUserContext();

    const [loginForm, setLoginForm] = useState<LoginUserForm>(emptyUser);
    const [loginFormErrors, setLoginFormErrors] = useState<LoginUserFormErrors>(
        {}
    );
    const [commonFormError, setCommonFormError] = useState('');

    if (isLoading) return <Loader className="py-6" />;
    if (isAuthorized) return <NavigateTo path="/" />;

    const login = async () => {
        const loginResult = await authService.login(
            loginForm,
            updateUserContext,
            navigate,
            '/'
        );

        if (loginResult instanceof Error) {
            setCommonFormError(loginResult.message);
        } else {
            setCommonFormError('');
        }
    };

    const handleLoginFormSubmit = async (e: FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        if (!authService.validateLoginForm(loginForm, setLoginFormErrors)) {
            return;
        }

        await login();
    };

    const handleInputChange = (e: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setLoginForm((prevState) => ({ ...prevState, [name]: value }));
    };

    return (
        <div className="max-w-md mx-auto mt-12 px-12 py-10 panel">
            <form className="flex flex-col" onSubmit={handleLoginFormSubmit}>
                <FormInput
                    className="mb-4"
                    label="Email"
                    type="email"
                    name="email"
                    placeholder="john@domain.com"
                    onValueChange={handleInputChange}
                    validationError={loginFormErrors.email}
                />

                <FormInput
                    className="mb-6"
                    label={t('common.auth.password')}
                    type="password"
                    name="password"
                    placeholder="********"
                    onValueChange={handleInputChange}
                    validationError={loginFormErrors.password}
                />

                {!!commonFormError && (
                    <div className="text-start clr-error text-xs mb-4">
                        * {commonFormError}
                    </div>
                )}

                <button className="mb-5" type="submit">
                    {t('common.auth.login')}
                </button>
            </form>

            <div className="flex items-center mb-3">
                <span className="w-full border border-t border-b-0 opacity-50"></span>
                <span className="font-medium px-5">{t('common.or')}</span>
                <span className="w-full border border-t border-b-0 opacity-50"></span>
            </div>

            <NavLink to="/registration">{t('common.auth.register')}</NavLink>
        </div>
    );
};

export default LoginPage;
