import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const routes = [
    {
        path: '/login',
        name: 'login',
        component: () => import('@/views/LoginView.vue'),
        meta: { requiresAuth: false }
    },
    {
        path: '/',
        name: 'home',
        component: () => import('@/views/HomeView.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/raw-materials',
        name: 'raw-materials',
        component: () => import('@/views/RawMaterialsView.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/products',
        name: 'products',
        component: () => import('@/views/ProductsView.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/production',
        name: 'production',
        component: () => import('@/views/ProductionView.vue'),
        meta: { requiresAuth: true }
    },
    {
        path: '/sales',
        name: 'sales',
        component: () => import('@/views/SalesView.vue'),
        meta: { requiresAuth: true }
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to, from, next) => {
    const auth = useAuthStore()

    if (to.meta.requiresAuth && !auth.isAuthenticated) {
        next('/login')
    } else if (to.path === '/login' && auth.isAuthenticated) {
        next('/')
    } else {
        next()
    }
})

export default router
