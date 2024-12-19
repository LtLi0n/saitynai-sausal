import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5264', // Replace with your backend URL
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token'); // Get JWT token from storage
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;