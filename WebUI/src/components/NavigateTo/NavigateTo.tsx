import { FC, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

interface NavigateToProps {
    path: string;
}

const NavigateTo: FC<NavigateToProps> = ({ path }: NavigateToProps) => {
    const navigate = useNavigate();

    useEffect(() => {
        navigate(path);
    }, [path, navigate]);

    return null;
};

export default NavigateTo;
