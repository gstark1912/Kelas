<template>
  <div class="production-view">
    <div class="page-header">
      <h1>Producción</h1>
      <button class="btn btn-primary" @click="openCreateModal">
        + Nueva Producción
      </button>
    </div>

    <div class="kpi-row">
      <div class="kpi-card">
        <span>Unidades Producidas</span>
        <strong>{{ formatNumber(kpis.totalUnitsProduced) }}</strong>
      </div>
      <div class="kpi-card">
        <span>Costo Total</span>
        <strong>{{ formatCurrency(kpis.totalCost) }}</strong>
      </div>
    </div>

    <div class="filters-bar">
      <select v-model="filters.productId" @change="fetchBatches">
        <option value="">Todos los productos</option>
        <option v-for="product in products" :key="product.id" :value="product.id">
          {{ product.name }}
        </option>
      </select>
      <input v-model="filters.dateFrom" type="date" @change="fetchBatches" />
      <input v-model="filters.dateTo" type="date" @change="fetchBatches" />
      <button class="btn btn-sm" :disabled="!hasFilters" @click="clearFilters">
        Limpiar
      </button>
    </div>

    <DataTable :columns="columns" :data="batches" :loading="loading">
      <template #cell-date="{ value }">
        <span class="num">{{ formatDate(value) }}</span>
      </template>
      <template #cell-productName="{ value }">
        <span class="fw-600">{{ value || '—' }}</span>
      </template>
      <template #cell-quantity="{ value }">
        <span class="num">{{ formatNumber(value) }}</span>
      </template>
      <template #cell-totalCost="{ value }">
        <span class="num">{{ formatCurrency(value) }}</span>
      </template>
      <template #cell-unitCost="{ value }">
        <span class="num">{{ formatCurrency(value) }}</span>
      </template>
      <template #cell-notes="{ value }">
        <span class="text-muted">{{ value || '—' }}</span>
      </template>
      <template #actions="{ row }">
        <div class="action-buttons">
          <button class="btn btn-sm" @click="openDetail(row)">Detalle</button>
        </div>
      </template>
    </DataTable>

    <ProductionFormModal
      :show="showCreateModal"
      :products="products"
      @close="closeCreateModal"
      @created="onProductionCreated"
    />

    <ProductionDetailModal
      :show="showDetailModal"
      :batch="selectedBatchDetail"
      @close="closeDetailModal"
    />

    <Transition name="toast">
      <div v-if="notification" class="toast" :class="notificationVariant">
        {{ notification }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import DataTable from '@/components/common/DataTable.vue'
import ProductionFormModal from '@/components/production/ProductionFormModal.vue'
import ProductionDetailModal from '@/components/production/ProductionDetailModal.vue'
import productionService from '@/services/productionService'
import productService from '@/services/productService'

const columns = [
  { key: 'date', label: 'Fecha', class: 'num' },
  { key: 'productName', label: 'Producto' },
  { key: 'quantity', label: 'Cantidad', class: 'num' },
  { key: 'totalCost', label: 'Costo Total', class: 'num' },
  { key: 'unitCost', label: 'Costo Unitario', class: 'num' },
  { key: 'notes', label: 'Notas' }
]

const batches = ref([])
const products = ref([])
const loading = ref(false)
const kpis = ref({ totalUnitsProduced: 0, totalCost: 0 })
const filters = ref({ productId: '', dateFrom: '', dateTo: '' })

const showCreateModal = ref(false)
const showDetailModal = ref(false)
const selectedBatchDetail = ref(null)
const notification = ref('')
const notificationVariant = ref('')
let notificationTimeout = null

const hasFilters = computed(() => {
  return filters.value.productId || filters.value.dateFrom || filters.value.dateTo
})

async function fetchProducts() {
  try {
    const response = await productService.getAll()
    products.value = response.data
  } catch {
    products.value = []
  }
}

async function fetchBatches() {
  loading.value = true
  try {
    const params = {}
    if (filters.value.productId) params.productId = filters.value.productId
    if (filters.value.dateFrom) params.dateFrom = filters.value.dateFrom
    if (filters.value.dateTo) params.dateTo = filters.value.dateTo

    const response = await productionService.getAll(params)
    batches.value = response.data.items || []
    kpis.value = response.data.kpis || { totalUnitsProduced: 0, totalCost: 0 }
  } catch {
    batches.value = []
    kpis.value = { totalUnitsProduced: 0, totalCost: 0 }
    showNotification('No se pudo cargar producción', 'toast-error')
  } finally {
    loading.value = false
  }
}

function clearFilters() {
  filters.value = { productId: '', dateFrom: '', dateTo: '' }
  fetchBatches()
}

function openCreateModal() {
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
}

async function onProductionCreated() {
  showCreateModal.value = false
  showNotification('✓ Producción registrada', '')
  await fetchBatches()
  await fetchProducts()
}

async function openDetail(row) {
  selectedBatchDetail.value = null
  showDetailModal.value = true
  try {
    const response = await productionService.getById(row.id)
    selectedBatchDetail.value = response.data
  } catch {
    showDetailModal.value = false
    showNotification('No se pudo cargar el detalle', 'toast-error')
  }
}

function closeDetailModal() {
  showDetailModal.value = false
  selectedBatchDetail.value = null
}

function showNotification(message, variant = '') {
  notification.value = message
  notificationVariant.value = variant
  if (notificationTimeout) clearTimeout(notificationTimeout)
  notificationTimeout = setTimeout(() => {
    notification.value = ''
    notificationVariant.value = ''
  }, 3000)
}

function formatCurrency(value) {
  if (value == null) return '$0'
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(value)
}

function formatNumber(value) {
  if (value == null) return '0'
  return Number(value).toLocaleString('es-AR')
}

function formatDate(value) {
  if (!value) return '—'
  return new Date(value).toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

onMounted(async () => {
  await fetchProducts()
  await fetchBatches()
})
</script>

<style scoped>
.production-view {
  font-family: var(--font, -apple-system, BlinkMacSystemFont, 'Inter', 'Segoe UI', sans-serif);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.page-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.kpi-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(180px, 1fr));
  gap: 12px;
  margin-bottom: 20px;
}

.kpi-card {
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius-lg, 8px);
  background: var(--color-bg, #ffffff);
  padding: 14px 16px;
  display: grid;
  gap: 4px;
}

.kpi-card span {
  font-size: 0.74rem;
  text-transform: uppercase;
  color: var(--color-text-secondary, #6b6b76);
  font-weight: 700;
  letter-spacing: 0.3px;
}

.kpi-card strong {
  font-size: 1.3rem;
  font-weight: 700;
  color: var(--color-text, #1a1a1a);
}

.filters-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.filters-bar input,
.filters-bar select {
  padding: 6px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-family: var(--font, inherit);
  color: var(--color-text, #1a1a1a);
  background: var(--color-bg, #ffffff);
}

.filters-bar select {
  min-width: 220px;
}

.btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 7px 14px;
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-weight: 500;
  border: 1px solid var(--color-border, #e5e5e7);
  background: var(--color-bg, #ffffff);
  color: var(--color-text, #1a1a1a);
  cursor: pointer;
  font-family: var(--font, inherit);
}

.btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.btn-primary {
  background: var(--color-primary, #5b5bd6);
  color: #fff;
  border-color: var(--color-primary, #5b5bd6);
}

.btn-sm {
  padding: 4px 10px;
  font-size: 0.8rem;
}

.action-buttons {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

.fw-600 {
  font-weight: 600;
}

.text-muted {
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.85rem;
}

.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background: var(--color-text, #1a1a1a);
  color: #fff;
  padding: 12px 20px;
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-weight: 500;
  box-shadow: var(--shadow-md, 0 4px 12px rgba(0, 0, 0, 0.08));
  z-index: 300;
}

.toast.toast-error {
  background: var(--color-negative, #c53030);
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

@media (max-width: 720px) {
  .page-header {
    align-items: flex-start;
    flex-direction: column;
    gap: 12px;
  }

  .kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>
