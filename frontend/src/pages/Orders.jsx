import React, { useEffect, useState } from 'react';
import axiosClient, { authClient, AUTH_API_BASE_URL } from '../api/axiosClient';

import { Link, useNavigate } from 'react-router-dom';

export default function Orders() {
    const [orders, setOrders] = useState([]);
    const [user, setUser] = useState(null);
    const [ordersError, setOrdersError] = useState(null);
    const [deletingOrderId, setDeletingOrderId] = useState(null);
    const navigate = useNavigate();

    useEffect(() => {
        // Fetch User info — if this fails the user is not authenticated
        authClient.get('/auth/user')
            .then(res => setUser(res.data))
            .catch(() => navigate('/'));

        // Fetch Orders — show error instead of redirecting to avoid loop
        setOrdersError(null);
        axiosClient.get('/orders')
            .then(res => setOrders(res.data))
            .catch(err => {
                if (err.response?.status === 401) {
                    setOrdersError('Session expired or OrderAPI certificate not trusted. Open https://localhost:7091 in a new tab to trust the cert, then refresh.');
                } else {
                    setOrdersError('Could not load orders. Is OrderAPI running?');
                }
            });
    }, [navigate]);

    const handleLogout = () => {
        window.location.href = `${AUTH_API_BASE_URL}/auth/logout`;
    };

    const handleDelete = async (order) => {
        const confirmed = window.confirm(`Delete order #${order.id} for ${order.customerName}?`);
        if (!confirmed) {
            return;
        }

        setDeletingOrderId(order.id);
        try {
            await axiosClient.delete(`/orders/${order.id}`);
            setOrders(prev => prev.filter(item => item.id !== order.id));
        } catch (err) {
            const status = err.response?.status;
            const responseData = err.response?.data;
            const message =
                responseData?.detail ||
                responseData?.title ||
                responseData?.message ||
                err.message ||
                'Unknown error';

            console.error('Failed to delete order', {
                status,
                responseData,
                error: err
            });

            alert(`Failed to delete order${status ? ` (${status})` : ''}: ${message}`);
        } finally {
            setDeletingOrderId(null);
        }
    };

    if (!user) return <div className="p-8">Loading...</div>;

    return (
        <div className="min-h-screen bg-gray-50 text-gray-800">
            <header className="bg-white shadow px-8 py-4 flex justify-between items-center">
                <h1 className="text-2xl font-bold text-indigo-600">Order Management</h1>
                <div className="flex items-center gap-4">
                    <span className="text-gray-600">Hello, {user.Name || 'User'}</span>
                    <button onClick={handleLogout} className="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg font-medium transition">
                        Logout
                    </button>
                </div>
            </header>

            <main className="p-8 max-w-5xl mx-auto">
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-xl font-semibold">Your Orders</h2>
                    <Link to="/orders/new" className="bg-indigo-600 hover:bg-indigo-700 text-white px-5 py-2 rounded-lg font-medium shadow transition">
                        + Create Order
                    </Link>
                </div>

                <div className="bg-white rounded-xl shadow overflow-hidden">
                    <table className="w-full text-left border-collapse">
                        <thead>
                            <tr className="bg-gray-100 text-gray-600 text-sm uppercase tracking-wider">
                                <th className="p-4 border-b">ID</th>
                                <th className="p-4 border-b">Customer Name</th>
                                <th className="p-4 border-b">Total Amount</th>
                                <th className="p-4 border-b">Status</th>
                                <th className="p-4 border-b text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {ordersError ? (
                                <tr>
                                    <td colSpan="5" className="p-8 text-center text-red-500 font-medium">{ordersError}</td>
                                </tr>
                            ) : orders.map(order => (
                                <tr key={order.id} className="border-b hover:bg-gray-50 transition">
                                    <td className="p-4">{order.id}</td>
                                    <td className="p-4 font-medium text-gray-900">{order.customerName}</td>
                                    <td className="p-4">${order.totalAmount.toFixed(2)}</td>
                                    <td className="p-4">
                                        <span className={`px-2 py-1 text-xs font-semibold rounded-full ${order.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' : 'bg-green-100 text-green-800'}`}>
                                            {order.status}
                                        </span>
                                    </td>
                                    <td className="p-4 text-right">
                                        <Link to={`/orders/${order.id}`} className="text-indigo-600 hover:text-indigo-900 font-medium mr-4">Edit</Link>
                                        <button
                                            type="button"
                                            onClick={() => handleDelete(order)}
                                            disabled={deletingOrderId === order.id}
                                            className="text-red-600 hover:text-red-900 font-medium disabled:text-gray-400 disabled:cursor-not-allowed"
                                        >
                                            {deletingOrderId === order.id ? 'Deleting...' : 'Delete'}
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {!ordersError && orders.length === 0 && (
                                <tr>
                                    <td colSpan="5" className="p-8 text-center text-gray-500">No orders found.</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </main>
        </div>
    );
}
