import axios from 'axios'
import { useAuthStore } from '@/stores/authStore'
import router from '@/router'

const api = axios.create({
    baseURL: import.meta.env.VITE_API_URL || '/api'
})

// Interceptor: agregar token
api.interceptors.request.use(config => {
    const auth = useAuthStore()
    if (auth.token) {
        config.headers.Authorization = `Bearer ${auth.token}`
    }
    return config
})

// Interceptor: manejar 401
api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            const auth = useAuthStore()
            auth.logout()
            router.push('/login')
        }
        return Promise.reject(error)
    }
)

export default api
