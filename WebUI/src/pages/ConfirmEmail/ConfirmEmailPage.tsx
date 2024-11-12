import { FC, ReactNode, useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import Loader from '../../components/UI/Loader/Loader';
import { getErrorFromResponse, trimEdges } from '../../utilities/utils';
import { useTranslation } from 'react-i18next';

const ConfirmEmailPage: FC = () => {
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [searchParams] = useSearchParams();
    const { t } = useTranslation();

    const [renderedContent, setRenderedContent] = useState<ReactNode>();

    useEffect(() => {
        try {
            if (isLoading === false) {
                setIsLoading(true);
            }

            let email = searchParams.get('email');
            let token = searchParams.get('token');

            if (!email || !token) {
                setRenderedContent(
                    <ErrorContent error="Invalid email or token." />
                );
                return;
            }

            const fetchConfirmEmailResult = async () => {
                let response = await fetch(
                    `/api/v1/auth/confirm-email?token=${token}&email=${email}`,
                    {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                    }
                );

                if (response.status === 200) {
                    setRenderedContent(<EmailConfirmed />);
                    return;
                }
                let errorRes =
                    (await getErrorFromResponse(response)) ??
                    t('common.unexpectedError');
                let error = trimEdges(errorRes, '"');
                setRenderedContent(<ErrorContent error={error} />);
            };

            fetchConfirmEmailResult();
        } catch (error) {
            console.error(error);
            let errorMessage = t('common.unexpectedError');

            if (error instanceof Error) {
                errorMessage = error.message;
            }
            setRenderedContent(<ErrorContent error={errorMessage} />);
        } finally {
            setIsLoading(false);
        }
    }, [searchParams]);

    return (
        <>
            {isLoading ? <Loader className="mt-6" /> : renderedContent}

            {isLoading == false}
        </>
    );
};

const ErrorContent: FC<{ error: string }> = ({ error }) => (
    <h3 className="text-red-400 mt-6">{error}</h3>
);
const EmailConfirmed = () => (
    <h3 className="mt-10 max-w-sm mx-auto">
        Your email address has been successfully confirmed and you can now{' '}
        <a href="/login">log in</a> to your account
    </h3>
);

export default ConfirmEmailPage;
