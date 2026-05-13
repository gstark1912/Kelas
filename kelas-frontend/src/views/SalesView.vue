<template>
  <div class="sales-view">
    <div class="page-header">
      <h1>Ventas</h1>
      <button class="btn btn-primary" @click="openCreateModal">
        + Nueva Venta
      </button>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <select v-model="channelFilter" @change="fetchSales">
        <option value="">Todos los canales</option>
        <option v-for="c in channels" :key="c" :value="c">{{ c }}</option>
      </select>
      <select v-model="paymentMethodFilter" @change="fetchSales">
        <option value="">Todos los medios de pago</option>
        <option v-for="m in paymentMethods" :key="m" :value="m">{{ m }}</option>
      </select>
      <input v-model="dateFromFilter" type="date" @change="fetchSales" />
      <span class="text-muted">a</span>
      <input v-model="dateToFilter" type="date" @change="fetchSales" />
    </div>

    <!-- KPIs -->
    <div class="kpi-grid" style="grid-template-columns: repeat(5, 1fr);">
      <div class="kpi-card">
        <div class="kpi-label">Ventas</div>
        <div class="kpi-value">{{ kpis.count }}</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">Ingresos</div>
        <div class="kpi-value">{{ formatCurrency(kpis.income) }}</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">COGS</div>
        <div class="kpi-value">{{ formatCurrency(kpis.cogs) }}</div>
      </div>
      <div class="kpi-card">
        <div class="kpi-label">Costos Venta</div>
        <div class="kpi-value">{{ formatCurrency(kpis.sellingCosts) }}</div>
      </div>
      <div class="kpi-card" :class="kpis.profit >= 0 ? 'kpi-positive' : 'kpi-negative'">
        <div class="kpi-label">Ganancia Neta</div>
        <div class="kpi-value">{{ kpis.profit >= 0 ? '+' : '' }}{{ formatCurrency(kpis.profit) }}</div>
      </div>
    </div>

    <!-- Table -->
    <DataTable :columns="columns" :data="sales" :loading="loading">
      <template #cell-id="{ value }">
        <span class="text-muted">#{{ value ? value.substring(value.length - 4) : '—' }}</span>
      </template>
      <template #cell-date="{ value }">
        {{ formatDateShort(value) }}
      </template>
      <template #cell-channel="{ value }">
        <span class="badge badge-primary">{{ value }}</span>
      </template>
      <template #cell-itemsSummary="{ row }">
        {{ formatItemsSummary(row.items) }}
      </template>
      <template #cell-grossIncome="{ value }">
        <span class="num">{{ formatCurrency(value) }}</span>
      </template>
      <template #cell-totalCOGS="{ value }">
        <span class="num text-muted">{{ formatCurrency(value) }}</span>
      </template>
      <template #cell-sellingCosts="{ row }">
        <span class="num text-muted">{{ formatCurrency((row.taxCostAmount || 0) + (row.channelCostAmount || 0)) }}</span>
      </template>
      <template #cell-netProfit="{ value }">
        <span class="num fw-600" :class="value >= 0 ? 'text-positive' : 'text-danger'">
          {{ value >= 0 ? '+' : '' }}{{ formatCurrency(value) }}
        </span>
      </template>
      <template #cell-paymentMethod="{ value }">
        <span class="badge badge-neutral">{{ value }}</span>
      </template>
      <template #actions="{ row }">
        <button class="btn btn-sm" @click="openDetailModal(row)">Ver</button>
      </template>
    </DataTable>

    <!-- New Sale Modal -->
    <NewSaleModal
      :show="showCreateModal"
      @close="closeCreateModal"
      @sale-created="onSaleCreated"
    />

    <!-- Detail Modal -->
    <AppModal
      :show="showDetailModal"
      :title="selectedSale ? `Venta #${selectedSale.id.substring(selectedSale.id.length - 4)}` : 'Detalle de Venta'"
      @close="closeDetailModal"
    >
      <div v-if="selectedSale">
        <div class="flex justify-between mb-16" style="font-size: 0.88rem; padding: 16px 20px 0;">
          <div><span class="text-secondary">Fecha:</span> {{ formatDate(selectedSale.date) }}</div>
          <div><span class="text-secondary">Canal:</span> <span class="badge badge-primary">{{ selectedSale.channel }}</span></div>
          <div><span class="text-secondary">Pago:</span> <span class="badge badge-neutral">{{ selectedSale.paymentMethod }}</span></div>
        </div>

        <div style="padding: 0 20px;">
          <table style="font-size: 0.85rem; width: 100%; border-collapse: collapse;">
            <thead>
              <tr style="border-bottom: 1px solid var(--color-border);">
                <th style="text-align: left; padding: 8px 0;">Producto</th>
                <th class="num" style="padding: 8px 0;">Cant.</th>
                <th class="num" style="padding: 8px 0;">Precio Unit.</th>
                <th class="num" style="padding: 8px 0;">Subtotal</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in selectedSale.items" :key="item.productId" style="border-bottom: 1px solid var(--color-border);">
                <td style="padding: 8px 0;">{{ item.productName }}</td>
                <td class="num" style="padding: 8px 0;">{{ item.quantity }}</td>
                <td class="num" style="padding: 8px 0;">{{ formatCurrency(item.unitPrice) }}</td>
                <td class="num" style="padding: 8px 0;">{{ formatCurrency(item.subtotal) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="financial-summary-block">
          <div class="flex justify-between"><span>Subtotal:</span> <span>{{ formatCurrency(selectedSale.subtotalProductos) }}</span></div>
          <div class="flex justify-between"><span>Descuento ({{ selectedSale.discountPercent }}%):</span> <span>-{{ formatCurrency(selectedSale.discountAmount) }}</span></div>
          <div class="flex justify-between"><span>Envío ({{ selectedSale.shippingDetail || '' }}):</span> <span>{{ formatCurrency(selectedSale.shippingCost) }}</span></div>
          <div class="flex justify-between fw-600 border-top mt-8 pt-8">
            <span>Ingreso Bruto:</span> <span>{{ formatCurrency(selectedSale.grossIncome) }}</span>
          </div>
          <div class="flex justify-between text-muted mt-4"><span>COGS:</span> <span>{{ formatCurrency(selectedSale.totalCOGS) }}</span></div>
          <div class="flex justify-between mt-4"><span>Ganancia Bruta:</span> <span>{{ formatCurrency(selectedSale.grossProfit) }}</span></div>
          <div class="flex justify-between text-muted mt-4"><span>Costo Impositivo ({{ selectedSale.taxCostPercent }}%):</span> <span>{{ formatCurrency(selectedSale.taxCostAmount) }}</span></div>
          <div class="flex justify-between text-muted mt-4"><span>Costo Canal ({{ selectedSale.channelCostPercent }}%):</span> <span>{{ formatCurrency(selectedSale.channelCostAmount) }}</span></div>
          <div class="flex justify-between text-positive fw-600 mt-8 pt-8 border-top" style="font-size: 1rem;">
            <span>Ganancia Neta:</span> <span>+{{ formatCurrency(selectedSale.netProfit) }}</span>
          </div>
        </div>

        <div v-if="selectedSale.notes" style="padding: 12px 20px; font-size: 0.85rem; color: var(--color-text-secondary);">
          Notas: {{ selectedSale.notes }}
        </div>
      </div>
      <template #footer>
        <button class="btn" @click="closeDetailModal">Cerrar</button>
      </template>
    </AppModal>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="notification" class="toast show">
        {{ notification }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import DataTable from '@/components/common/DataTable.vue'
import AppModal from '@/components/common/AppModal.vue'
import NewSaleModal from '@/components/sales/NewSaleModal.vue'
import saleService from '@/services/saleService'

// Constants
const channels = ['Feria', 'Instagram', 'Tienda', 'Otro']
const paymentMethods = ['Efectivo', 'Transferencia', 'Mercado Pago']

const columns = [
  { key: 'id', label: '#' },
  { key: 'date', label: 'Fecha' },
  { key: 'channel', label: 'Canal' },
  { key: 'itemsSummary', label: 'Ítems' },
  { key: 'grossIncome', label: 'Ingreso Bruto', class: 'num' },
  { key: 'totalCOGS', label: 'COGS', class: 'num' },
  { key: 'sellingCosts', label: 'Costos Vta', class: 'num' },
  { key: 'netProfit', label: 'Ganancia', class: 'num' },
  { key: 'paymentMethod', label: 'Pago' }
]

// State
const sales = ref([])
const loading = ref(false)
const channelFilter = ref('')
const paymentMethodFilter = ref('')
const dateFromFilter = ref('')
const dateToFilter = ref('')

// Modal state
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const selectedSale = ref(null)

// Notification
const notification = ref('')
let notificationTimeout = null

// Computed (KPIs calculated on Frontend)
const kpis = computed(() => {
  const data = sales.value || []
  const count = data.length
  const income = data.reduce((acc, s) => acc + (s.grossIncome || 0), 0)
  const cogs = data.reduce((acc, s) => acc + (s.totalCOGS || 0), 0)
  const sellingCosts = data.reduce((acc, s) => acc + (s.taxCostAmount || 0) + (s.channelCostAmount || 0), 0)
  const profit = data.reduce((acc, s) => acc + (s.netProfit || 0), 0)

  return { count, income, cogs, sellingCosts, profit }
})

// Methods
function formatCurrency(amount) {
  if (amount == null) return '$ 0'
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(amount)
}

function formatDate(dateStr) {
  if (!dateStr) return '—'
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '—'
  return date.toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

function formatDateShort(dateStr) {
  if (!dateStr) return '—'
  const date = new Date(dateStr)
  if (isNaN(date.getTime())) return '—'
  return date.toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit'
  })
}

function formatItemsSummary(items) {
  if (!items || items.length === 0) return '—'
  return items.map(i => `${i.quantity}x ${i.productName ? i.productName.split(' ')[0] : 'Prod'}`).join(', ')
}

async function fetchSales() {
  loading.value = true
  try {
    const filters = {}
    if (channelFilter.value) filters.channel = channelFilter.value
    if (paymentMethodFilter.value) filters.paymentMethod = paymentMethodFilter.value
    if (dateFromFilter.value) filters.dateFrom = dateFromFilter.value
    if (dateToFilter.value) filters.dateTo = dateToFilter.value
    
    const response = await saleService.getAll(filters)
    sales.value = response.data || []
  } catch (err) {
    sales.value = []
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
}

function onSaleCreated() {
  showNotification('✓ Venta registrada exitosamente')
  fetchSales()
}

function openDetailModal(row) {
  selectedSale.value = row
  showDetailModal.value = true
}

function closeDetailModal() {
  showDetailModal.value = false
  selectedSale.value = null
}

function showNotification(message) {
  notification.value = message
  if (notificationTimeout) clearTimeout(notificationTimeout)
  notificationTimeout = setTimeout(() => {
    notification.value = ''
  }, 2500)
}

// Lifecycle
onMounted(() => {
  const now = new Date()
  const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
  dateFromFilter.value = firstDay.toISOString().split('T')[0]
  dateToFilter.value = now.toISOString().split('T')[0]
  
  fetchSales()
})
</script>

<style scoped>
.sales-view {
  font-family: var(--font, -apple-system, BlinkMacSystemFont, 'Inter', 'Segoe UI', sans-serif);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.page-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

/* Filters */
.filters-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.filters-bar select,
.filters-bar input {
  padding: 6px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  background: var(--color-bg, #ffffff);
}

/* KPI Grid */
.kpi-grid {
  display: grid;
  gap: 16px;
  margin-bottom: 24px;
}

.kpi-card {
  background: var(--color-bg, #ffffff);
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  padding: 16px;
}

.kpi-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-text-secondary, #6b6b76);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 4px;
}

.kpi-value {
  font-size: 1.25rem;
  font-weight: 700;
}

.kpi-positive .kpi-value {
  color: var(--color-positive, #2d7a3a);
}

.kpi-negative .kpi-value {
  color: var(--color-negative, #c53030);
}

/* Financial Summary Block (Modal Detail) */
.financial-summary-block {
  margin: 16px 20px;
  padding: 12px 16px;
  background: var(--color-bg-secondary, #f7f7f8);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
}

.toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background: #1a1a1a;
  color: #fff;
  padding: 12px 20px;
  border-radius: 6px;
  z-index: 1000;
}
</style>
