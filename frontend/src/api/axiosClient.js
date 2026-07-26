import axios from 'axios';

const getEnvValue = (name, fallback) => {
    const value = import.meta.env?.[name];
    return value || fallback;
};

const frontendOrigin = typeof window !== 'undefined' ? window.location.origin : 'https://localhost:5173';

export const AUTH_API_BASE_URL = getEnvValue('VITE_AUTH_API_BASE_URL', 'https://localhost:7070/api');
export const ORDER_API_BASE_URL = getEnvValue('VITE_ORDER_API_BASE_URL', 'https://localhost:7091/api');
export const FRONTEND_URL = getEnvValue('VITE_FRONTEND_URL', frontendOrigin);

const axiosClient = axios.create({
    baseURL: ORDER_API_BASE_URL,
    withCredentials: true,
});

export const authClient = axios.create({
    baseURL: AUTH_API_BASE_URL,
    withCredentials: true,
});

export default axiosClient;
