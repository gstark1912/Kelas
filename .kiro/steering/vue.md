# Convenciones Vue.js — Kelas Frontend

## Estructura del Proyecto

```
src/
├── assets/               # Estilos globales, imágenes, iconos
├── components/
│   ├── common/           # Componentes reutilizables (DataTable, Modal, KpiCard, etc.)
│   ├── layout/           # Layout principal (Sidebar, Header, etc.)
│   └── [modulo]/         # Componentes específicos de un módulo
├── views/                # Páginas/vistas (una por ruta principal)
│   ├── DashboardView.vue
│   ├── ProductsView.vue
│   ├── RawMaterialsView.vue
│   ├── ProductionView.vue
│   ├── SalesView.vue
│   ├── ExpensesView.vue
│   ├── CashView.vue
│   └── LoginView.vue
├── services/             # Llamadas a la API (una por módulo)
│   ├── api.js            # Instancia base de axios/fetch con interceptors
│   ├── authService.js
│   ├── rawMaterialService.js
│   ├── productService.js
│   └── ...
├── stores/               # Estado global (Pinia)
│   └── authStore.js
├── router/
│   └── index.js          # Definición de rutas + guards
├── composables/          # Composables reutilizables (useFilters, usePagination, etc.)
├── utils/                # Funciones utilitarias (formateo de moneda, fechas, etc.)
├── App.vue
└── main.js
```

## Componentes Reutilizables

El sistema tiene muchos patrones repetidos. Crear componentes base:

### DataTable

```vue
<!-- components/common/DataTable.vue -->
<!-- Props: columns, data, sortable, searchable -->
<!-- Emits: sort, search, row-click -->
<!-- Features: ordenamiento por columna, búsqueda, slots para acciones -->
```

### KpiCard

```vue
<!-- components/common/KpiCard.vue -->
<!-- Props: title, value, subtitle, variant (default/success/danger/warning) -->
```

### FilterBar

```vue
<!-- components/common/FilterBar.vue -->
<!-- Props: filters (array de definiciones) -->
<!-- Emits: filter-change -->
<!-- Soporta: date range, select, text search -->
```

### AppModal

```vue
<!-- components/common/AppModal.vue -->
<!-- Props: title, show, size -->
<!-- Emits: close, confirm -->
<!-- Slots: default (body), footer -->
```

### FormField

```vue
<!-- components/common/FormField.vue -->
<!-- Props: label, error, required -->
<!-- Slot: default (input) -->
```

### ConfirmDialog

```vue
<!-- components/common/ConfirmDialog.vue -->
<!-- Props: title, message, confirmText, variant (danger/warning) -->
<!-- Emits: confirm, cancel -->
```

## Arquitectura de Vistas

Cada vista sigue el mismo patrón:

```vue
<template>
  <div class="view-container">
    <!-- KPIs del módulo -->
    <div class="kpi-row">
      <KpiCard v-for="kpi in kpis" :key="kpi.title" v-bind="kpi" />
    </div>

    <!-- Filtros -->
    <FilterBar :filters="filterConfig" @filter-change="onFilterChange" />

    <!-- Tabla principal -->
    <DataTable :columns="columns" :data="items" sortable @sort="onSort">
      <template #actions="{ row }">
        <!-- Botones de acción por fila -->
      </template>
    </DataTable>

    <!-- Modal de creación/edición -->
    <AppModal :show="showModal" :title="modalTitle" @close="closeModal">
      <!-- Formulario -->
    </AppModal>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useFilters } from '@/composables/useFilters'
// ...
</script>
```

## Llamadas a la API

### Instancia Base

```javascript
// services/api.js
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
```

### Servicios por Módulo

```javascript
// services/rawMaterialService.js
import api from './api'

export default {
  getAll(filters = {}) {
    return api.get('/raw-materials', { params: filters })
  },
  getById(id) {
    return api.get(`/raw-materials/${id}`)
  },
  create(data) {
    return api.post('/raw-materials', data)
  },
  update(id, data) {
    return api.put(`/raw-materials/${id}`, data)
  }
}
```

## Autenticación

### Store

```javascript
// stores/authStore.js
import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null
  }),
  getters: {
    isAuthenticated: (state) => !!state.token
  },
  actions: {
    setToken(token) {
      this.token = token
      localStorage.setItem('token', token)
    },
    logout() {
      this.token = null
      localStorage.removeItem('token')
    }
  }
})
```

### Router Guard

```javascript
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
```

## Naming Conventions

- **Componentes**: PascalCase (`DataTable.vue`, `KpiCard.vue`).
- **Vistas**: PascalCase con sufijo View (`ProductsView.vue`).
- **Servicios**: camelCase (`rawMaterialService.js`).
- **Composables**: camelCase con prefijo `use` (`useFilters.js`).
- **Stores**: camelCase con sufijo Store (`authStore.js`).
- **Props**: camelCase en JS, kebab-case en template.
- **Eventos**: kebab-case (`@filter-change`, `@row-click`).
- **CSS classes**: kebab-case o BEM si se necesita.

## Manejo de Estado

- **Pinia** solo para estado global (auth, configuración).
- Estado local de cada vista en `ref`/`reactive` dentro del `<script setup>`.
- No crear stores por módulo a menos que se necesite compartir estado entre vistas.

## Formateo y Utilidades

```javascript
// utils/format.js
export function formatCurrency(amount) {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(amount)
}

export function formatDate(date) {
  return new Intl.DateTimeFormat('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(new Date(date))
}

export function formatPercent(value) {
  return `${value.toFixed(1)}%`
}
```

## Layout y Navegación

La aplicación usa un layout con sidebar (`AppLayout.vue` + `AppSidebar.vue`) que se muestra automáticamente en todas las rutas con `meta.requiresAuth: true`. Las rutas sin auth (login) se renderizan sin layout.

### Cómo funciona

- `App.vue` evalúa `route.meta.requiresAuth` para decidir si envuelve la vista en `<AppLayout>`.
- `AppLayout.vue` renderiza el `<AppSidebar>` + un `<main>` con slot para el contenido.
- `AppSidebar.vue` contiene la navegación con secciones y links.

### Al agregar una nueva pantalla/vista

1. Crear la vista en `src/views/{Nombre}View.vue`
2. Registrar la ruta en `src/router/index.js` con `meta: { requiresAuth: true }`
3. Agregar el item de navegación en `AppSidebar.vue` → array `operationItems` (o en la sección correspondiente)
4. Cambiar `enabled: true` cuando la ruta esté implementada

### Estructura del sidebar

```
PRINCIPAL
  └── Dashboard (/)

OPERACIONES
  ├── Productos (/products) — disabled
  ├── Materias Primas (/raw-materials) — enabled
  ├── Producción (/production) — disabled
  ├── Ventas (/sales) — disabled
  ├── Gastos (/expenses) — disabled
  └── Caja (/cash) — disabled
```

Los items deshabilitados tienen `enabled: false` y se muestran con opacidad reducida sin permitir navegación. Al implementar un módulo nuevo, cambiar a `enabled: true`.

## Reglas Generales

- Composition API con `<script setup>` (no Options API).
- No usar Vuex — usar Pinia si se necesita estado global.
- Componentes pequeños y enfocados.
- Props tipadas con `defineProps`.
- Emits declarados con `defineEmits`.
- CSS scoped en cada componente.
- Responsive design (mobile-friendly).
- Feedback visual: loading states, mensajes de éxito/error (toast/notification).
