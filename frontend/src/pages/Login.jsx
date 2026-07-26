import React, { useEffect, useState } from 'react';
import { authClient, AUTH_API_BASE_URL, FRONTEND_URL } from '../api/axiosClient';
import { useNavigate } from 'react-router-dom';

export default function Login() {
    const navigate = useNavigate();
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // Check if already authenticated
        authClient.get('/auth/user')
            .then(res => {
                if (res.data) {
                    navigate('/orders');
                }
            })
            .catch(() => {
                setLoading(false);
            });
    }, [navigate]);

    const handleLogin = () => {
        const redirectUrl = new URL('/orders', FRONTEND_URL).toString();
        window.location.href = `${AUTH_API_BASE_URL}/auth/login?returnUrl=${redirectUrl}`;
    };

    if (loading) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-100">
                <p className="text-xl text-gray-600">Loading...</p>
            </div>
        );
    }

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-r from-blue-500 to-indigo-600">
            <div className="bg-white p-10 rounded-2xl shadow-2xl max-w-sm w-full text-center">
                <h2 className="text-3xl font-bold text-gray-800 mb-6">Welcome Back</h2>
                <p className="text-gray-500 mb-8">Sign in to manage your orders securely with Entra ID.</p>
                <button 
                    onClick={handleLogin}
                    className="w-full bg-blue-600 text-white font-semibold py-3 px-4 rounded-lg shadow-md hover:bg-blue-700 hover:shadow-lg transition duration-200 ease-in-out transform hover:-translate-y-0.5"
                >
                    Login with Azure AD
                </button>
            </div>
        </div>
    );
}
