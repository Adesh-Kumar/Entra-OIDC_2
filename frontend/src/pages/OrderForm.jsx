import React, { useEffect, useState } from 'react';
import axiosClient from '../api/axiosClient';
import { useNavigate, useParams, Link } from 'react-router-dom';

export default function OrderForm() {
    const { id } = useParams();
    const navigate = useNavigate();
    const isEditing = Boolean(id);

    const [formData, setFormData] = useState({
        customerName: '',
        totalAmount: 0,
        status: 'Pending'
    });

    useEffect(() => {
        if (isEditing) {
            axiosClient.get(`/orders/${id}`)
                .then(res => setFormData(res.data))
                .catch(() => navigate('/orders'));
        }
    }, [id, isEditing, navigate]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: name === 'totalAmount' ? parseFloat(value) : value }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        const req = isEditing 
            ? axiosClient.put(`/orders/${id}`, { ...formData, id: parseInt(id) })
            : axiosClient.post('/orders', formData);

        req.then(() => navigate('/orders'))
           .catch(err => alert('Failed to save order'));
    };

    return (
        <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
            <div className="bg-white p-8 rounded-2xl shadow-xl max-w-lg w-full">
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-2xl font-bold text-gray-800">{isEditing ? 'Edit Order' : 'Create Order'}</h2>
                    <Link to="/orders" className="text-gray-500 hover:text-gray-700 font-medium text-sm">Cancel</Link>
                </div>

                <form onSubmit={handleSubmit} className="space-y-5">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Customer Name</label>
                        <input 
                            type="text" 
                            name="customerName" 
                            value={formData.customerName} 
                            onChange={handleChange} 
                            required 
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition"
                            placeholder="e.g. John Doe"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Total Amount</label>
                        <input 
                            type="number" 
                            step="0.01" 
                            name="totalAmount" 
                            value={formData.totalAmount} 
                            onChange={handleChange} 
                            required 
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition"
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                        <select 
                            name="status" 
                            value={formData.status} 
                            onChange={handleChange}
                            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none transition bg-white"
                        >
                            <option value="Pending">Pending</option>
                            <option value="Shipped">Shipped</option>
                            <option value="Delivered">Delivered</option>
                            <option value="Cancelled">Cancelled</option>
                        </select>
                    </div>
                    <button 
                        type="submit" 
                        className="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-3 rounded-lg shadow transition transform hover:-translate-y-0.5"
                    >
                        {isEditing ? 'Save Changes' : 'Create Order'}
                    </button>
                </form>
            </div>
        </div>
    );
}
