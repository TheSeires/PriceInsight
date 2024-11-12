import { ToastContainer } from 'react-toastify';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import MainLayout from './layouts/MainLayout';
import HomePage from './pages/Home/HomePage';
import NotFoundPage from './pages/NotFound/NotFoundPage';
import RegistrationPage from './pages/Registration/RegistrationPage';
import ConfirmEmailPage from './pages/ConfirmEmail/ConfirmEmailPage';
import LoginPage from './pages/Login/LoginPage';
import UserContextProvider from './components/UserContextProvider/UserContextProvider';
import AdministrationPage from './pages/Administration/AdministrationPage';
import ProfilePage from './pages/Profile/ProfilePage';
import './App.css';
import 'react-toastify/dist/ReactToastify.css';
import ProductPage from './pages/Product/ProductPage.tsx';
import CatalogPage from './pages/Catalog/CatalogPage.tsx';

function App() {
    return (
        <>
            {/* Shared components */}
            <ToastContainer theme="dark" />

            <Router>
                <UserContextProvider>
                    <Routes>
                        {/* Home page */}
                        <Route
                            path="/"
                            element={
                                <MainLayout
                                    headerClass={
                                        '!absolute top-0 left-0 w-full'
                                    }
                                >
                                    <HomePage />
                                </MainLayout>
                            }
                        />

                        {/* Catalog page */}
                        <Route
                            path="/catalog"
                            element={
                                <MainLayout>
                                    <CatalogPage />
                                </MainLayout>
                            }
                        />

                        {/* Product page */}
                        <Route
                            path="/product/:id"
                            element={
                                <MainLayout>
                                    <ProductPage />
                                </MainLayout>
                            }
                        />

                        {/* Profile page */}
                        <Route
                            path="/profile"
                            element={
                                <MainLayout>
                                    <ProfilePage />
                                </MainLayout>
                            }
                        />

                        {/* Login page */}
                        <Route
                            path="/login"
                            element={
                                <MainLayout>
                                    <LoginPage />
                                </MainLayout>
                            }
                        />

                        {/* Registration page */}
                        <Route
                            path="/registration"
                            element={
                                <MainLayout>
                                    <RegistrationPage />
                                </MainLayout>
                            }
                        />

                        {/* Confirm email page */}
                        <Route
                            path="/confirm-email"
                            element={
                                <MainLayout>
                                    <ConfirmEmailPage />
                                </MainLayout>
                            }
                        />

                        {/* Administration */}
                        <Route
                            path="/administration"
                            element={
                                <MainLayout>
                                    <AdministrationPage />
                                </MainLayout>
                            }
                        />

                        {/* Not Found page */}
                        <Route
                            path="*"
                            element={
                                <MainLayout>
                                    <NotFoundPage />
                                </MainLayout>
                            }
                        />
                    </Routes>
                </UserContextProvider>
            </Router>
        </>
    );
}

export default App;
