<template>
  <aside class="sidebar">
    <div class="sidebar-logo">
      <span class="sidebar-logo-text">Kelas</span><span class="sidebar-logo-dot">.</span>
    </div>

    <nav class="sidebar-nav">
      <div class="sidebar-section-label">Principal</div>
      <router-link to="/" class="nav-link" exact-active-class="active">
        <span class="icon">📊</span> Dashboard
      </router-link>

      <div class="sidebar-section-label">Operaciones</div>
      <router-link
        v-for="item in operationItems"
        :key="item.path"
        :to="item.enabled ? item.path : ''"
        class="nav-link"
        :class="{ disabled: !item.enabled }"
        active-class="active"
        @click.prevent="!item.enabled && null"
      >
        <span class="icon">{{ item.icon }}</span> {{ item.label }}
      </router-link>
    </nav>

    <div class="sidebar-footer">
      <button class="nav-link logout-link" @click="handleLogout">
        <span class="icon">🚪</span> Cerrar sesión
      </button>
    </div>
  </aside>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = useRouter()
const auth = useAuthStore()

const operationItems = [
  { path: '/products', label: 'Productos', icon: '🏷️', enabled: false },
  { path: '/raw-materials', label: 'Materias Primas', icon: '🧱', enabled: true },
  { path: '/production', label: 'Producción', icon: '⚙️', enabled: false },
  { path: '/sales', label: 'Ventas', icon: '💰', enabled: false },
  { path: '/expenses', label: 'Gastos', icon: '📋', enabled: false },
  { path: '/cash', label: 'Caja', icon: '🏦', enabled: false }
]

function handleLogout() {
  auth.logout()
  router.push('/login')
}
</script>

<style scoped>
.sidebar {
  width: var(--sidebar-width, 240px);
  background: #fafafa;
  border-right: 1px solid var(--color-border, #e5e5e7);
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  z-index: 100;
}

.sidebar-logo {
  padding: 16px 20px;
  font-size: 1.15rem;
  font-weight: 700;
  color: var(--color-text, #1a1a1a);
  letter-spacing: -0.3px;
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  display: flex;
  align-items: center;
  gap: 0;
}

.sidebar-logo-dot {
  color: var(--color-primary, #5b5bd6);
}

.sidebar-nav {
  flex: 1;
  padding: 12px 10px;
  overflow-y: auto;
}

.sidebar-section-label {
  font-size: 0.72rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted, #9b9ba7);
  padding: 16px 12px 6px;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-radius: var(--radius, 6px);
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.92rem;
  font-weight: 500;
  transition: background 0.1s, color 0.1s;
  text-decoration: none;
  cursor: pointer;
  border: none;
  background: none;
  width: 100%;
  text-align: left;
  font-family: inherit;
}

.nav-link:hover:not(.disabled) {
  background: var(--color-bg-hover, #f0f0f2);
  color: var(--color-text, #1a1a1a);
  text-decoration: none;
}

.nav-link.active {
  background: var(--color-primary-light, #ededfd);
  color: var(--color-primary, #5b5bd6);
  font-weight: 600;
}

.nav-link.disabled {
  opacity: 0.4;
  cursor: not-allowed;
  pointer-events: none;
}

.nav-link .icon {
  font-size: 1.1rem;
  width: 20px;
  text-align: center;
}

.sidebar-footer {
  padding: 12px 10px;
  border-top: 1px solid var(--color-border, #e5e5e7);
}

.logout-link {
  color: var(--color-negative, #c53030);
}

.logout-link:hover {
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
}
</style>
