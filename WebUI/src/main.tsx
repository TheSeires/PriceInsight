import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './styles/theme.css';
import './index.css';
import LocalizationProvider from './components/LocalizationProvider/LocalizationProvider.tsx';
import Loader from './components/UI/Loader/Loader.tsx';
import { FaceFrownIcon } from '@heroicons/react/24/outline';

const loadingTemplate = (
    <div className="w-full h-full items-center justify-center flex flex-col">
        <Loader className="size-8" />
        <div className="mt-4">Loading, please be patient...</div>
    </div>
);

const failureTemplate = (
    <div className="w-full h-full items-center justify-center flex flex-col">
        <FaceFrownIcon className="size-12 clr-primary" />
        <p className="mt-2">The server could not be reached.</p>
        <p>Please, try again later...</p>
    </div>
);

createRoot(document.getElementById('root')!).render(
    <StrictMode>
        <LocalizationProvider
            loadingTemplate={loadingTemplate}
            failureTemplate={failureTemplate}
        >
            <App />
        </LocalizationProvider>
    </StrictMode>
);
