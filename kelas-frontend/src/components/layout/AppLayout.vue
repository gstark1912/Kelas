<template>
  <div class="layout">
    <AppSidebar />
    <main class="main-area">
      <header class="topbar">
        <div class="topbar-left">
          <span class="topbar-title">{{ pageTitle }}</span>
        </div>
        <div class="topbar-right">
          <span class="topbar-context">{{ currentMonth }}</span>
        </div>
      </header>
      <div class="content">
        <slot></slot>
      </div>
    </main>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './AppSidebar.vue'

const route = useRoute()

const pageTitles = {
  '/': 'Dashboard',
  '/raw-materials': 'Materias Primas',
  '/products': 'Productos',
  '/production': 'Producción',
  '/sales': 'Ventas',
  '/expenses': 'Gastos',
  '/cash': 'Caja'
}

const pageTitle = computed(() => {
  return pageTitles[route.path] || ''
})

const currentMonth = computed(() => {
  const now = new Date()
  return now.toLocaleDateString('es-AR', { month: 'long', year: 'numeric' })
    .replace(/^\w/, c => c.toUpperCase())
})
</script>

<style scoped>
.layout {
  display: flex;
  min-height: 100vh;
}

.main-area {
  margin-left: var(--sidebar-width, 240px);
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.topbar {
  height: var(--topbar-height, 52px);
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 28px;
  background: var(--color-bg, #ffffff);
  position: sticky;
  top: 0;
  z-index: 50;
}

.topbar-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.topbar-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--color-text, #1a1a1a);
}

.topbar-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.topbar-context {
  font-size: 0.82rem;
  color: var(--color-text-muted, #9b9ba7);
}

.content {
  padding: 28px;
  flex: 1;
  max-width: 1200px;
}
</style>
