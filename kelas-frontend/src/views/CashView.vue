<template>
  <div class="view-container">
    <div class="page-header">
      <div>
        <h1 class="page-title">Caja</h1>
        <p class="page-subtitle">Cuentas y movimientos</p>
      </div>
      <div class="page-actions">
        <button class="btn btn-secondary" @click="showTransferModal = true">Transferencia entre cuentas</button>
        <button class="btn btn-primary" @click="showManualModal = true">+ Nuevo Movimiento</button>
      </div>
    </div>

    <div v-if="error" class="page-alert">{{ error }}</div>

    <div class="kpi-row">
      <KpiCard
        v-for="account in accounts"
        :key="account.id"
        :title="account.name"
        :value="formatCurrency(account.currentBalance)"
        :subtitle="account.currentBalance < 0 ? 'Saldo negativo' : 'Cuenta activa'"
        :variant="account.currentBalance < 0 ? 'danger' : 'default'"
      />
      <KpiCard
        title="Saldo Total"
        :value="formatCurrency(summary.totalBalance)"
        :subtitle="`${accounts.length} cuentas activas`"
        :variant="summary.totalBalance < 0 ? 'danger' : 'success'"
      />
    </div>

    <div class="filters-container">
      <label class="filter-group">
        <span>Cuenta</span>
        <select v-model="filters.cashAccountId" class="form-control" @change="loadMovements">
          <option value="">Todas</option>
          <option v-for="account in accounts" :key="account.id" :value="account.id">{{ account.name }}</option>
        </select>
      </label>

      <label class="filter-group">
        <span>Tipo</span>
        <select v-model="filters.type" class="form-control" @change="loadMovements">
          <option value="">Ingresos y egresos</option>
          <option value="Ingreso">Ingreso</option>
          <option value="Egreso">Egreso</option>
        </select>
      </label>

      <label class="filter-group">
        <span>Concepto</span>
        <select v-model="filters.concept" class="form-control" @change="loadMovements">
          <option value="">Todos</option>
          <option v-for="concept in concepts" :key="concept" :value="concept">{{ concept }}</option>
        </select>
      </label>

      <label class="filter-group">
        <span>Desde</span>
        <input v-model="filters.dateFrom" class="form-control" type="date" @change="loadMovements" />
      </label>

      <label class="filter-group">
        <span>Hasta</span>
        <input v-model="filters.dateTo" class="form-control" type="date" @change="loadMovements" />
      </label>

      <button class="btn btn-secondary" @click="clearFilters">Limpiar</button>
    </div>

    <div class="totals-row">
      <div><span>Total ingresos:</span> <strong class="text-positive">{{ formatCurrency(movementResult.totalIncome) }}</strong></div>
      <div><span>Total egresos:</span> <strong class="text-negative">{{ formatCurrency(movementResult.totalExpense) }}</strong></div>
      <div><span>Neto período:</span> <strong :class="movementResult.netTotal < 0 ? 'text-negative' : 'text-positive'">{{ formatCurrency(movementResult.netTotal) }}</strong></div>
    </div>

    <DataTable :columns="columns" :data="movements" :loading="loadingMovements">
      <template #cell-date="{ value }">
        {{ formatDate(value) }}
      </template>
      <template #cell-type="{ value }">
        <span class="badge" :class="isIncome(value) ? 'badge-positive' : 'badge-negative'">
          {{ displayType(value) }}
        </span>
      </template>
      <template #cell-concept="{ value }">
        <span class="badge badge-neutral">{{ value }}</span>
      </template>
      <template #cell-origin="{ row }">
        <span v-if="isManual(row)" class="origin-muted">Manual</span>
        <router-link v-else-if="originLink(row)" :to="originLink(row)">{{ originLabel(row) }}</router-link>
        <span v-else>{{ originLabel(row) }}</span>
      </template>
      <template #cell-amount="{ row }">
        <span class="num" :class="isIncome(row.type) ? 'text-positive' : 'text-negative'">
          {{ isIncome(row.type) ? '+' : '-' }}{{ formatCurrency(row.amount) }}
        </span>
      </template>
    </DataTable>

    <ManualCashMovementModal
      :show="showManualModal"
      :accounts="accounts"
      @close="showManualModal = false"
      @saved="handleMovementSaved"
    />
    <CashTransferModal
      :show="showTransferModal"
      :accounts="accounts"
      @close="showTransferModal = false"
      @saved="handleMovementSaved"
    />
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import DataTable from '@/components/common/DataTable.vue'
import KpiCard from '@/components/common/KpiCard.vue'
import ManualCashMovementModal from '@/components/cash/ManualCashMovementModal.vue'
import CashTransferModal from '@/components/cash/CashTransferModal.vue'
import cashAccountService from '@/services/cashAccountService'
import cashMovementService from '@/services/cashMovementService'
import { formatCurrency, formatDate, toDateInputValue, toUtcDateEnd, toUtcDateStart } from '@/utils/format'

const concepts = ['Venta', 'Compra MP', 'Gasto', 'Aporte de capital', 'Retiro de efectivo', 'Transferencia', 'Ajuste de saldo', 'Otro']
const columns = [
  { key: 'date', label: 'Fecha' },
  { key: 'cashAccountName', label: 'Cuenta' },
  { key: 'type', label: 'Tipo' },
  { key: 'concept', label: 'Concepto' },
  { key: 'description', label: 'Descripción' },
  { key: 'origin', label: 'Origen' },
  { key: 'amount', label: 'Monto', class: 'num' }
]

const summary = ref({ accounts: [], totalBalance: 0 })
const movementResult = ref({ items: [], totalIncome: 0, totalExpense: 0, netTotal: 0 })
const loadingMovements = ref(false)
const error = ref('')
const showManualModal = ref(false)
const showTransferModal = ref(false)

const filters = reactive({
  cashAccountId: '',
  type: '',
  concept: '',
  dateFrom: toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1)),
  dateTo: toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0))
})

const accounts = computed(() => summary.value.accounts || [])
const movements = computed(() => movementResult.value.items || [])

async function loadSummary() {
  const res = await cashAccountService.getSummary()
  summary.value = res.data
}

async function loadMovements() {
  loadingMovements.value = true
  error.value = ''
  try {
    const params = {
      cashAccountId: filters.cashAccountId || undefined,
      type: filters.type || undefined,
      concept: filters.concept || undefined,
      dateFrom: filters.dateFrom ? toUtcDateStart(filters.dateFrom) : undefined,
      dateTo: filters.dateTo ? toUtcDateEnd(filters.dateTo) : undefined
    }
    const res = await cashMovementService.getAll(params)
    movementResult.value = res.data
  } catch (e) {
    error.value = e.response?.data?.error || 'No se pudieron cargar los movimientos.'
  } finally {
    loadingMovements.value = false
  }
}

async function loadData() {
  try {
    await Promise.all([loadSummary(), loadMovements()])
  } catch (e) {
    error.value = e.response?.data?.error || 'No se pudo cargar Caja.'
  }
}

function clearFilters() {
  filters.cashAccountId = ''
  filters.type = ''
  filters.concept = ''
  filters.dateFrom = toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1))
  filters.dateTo = toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0))
  loadMovements()
}

async function handleMovementSaved() {
  showManualModal.value = false
  showTransferModal.value = false
  await loadData()
}

function isIncome(type) {
  return type === 'Ingreso' || type === 'income'
}

function displayType(type) {
  return isIncome(type) ? 'Ingreso' : 'Egreso'
}

function isManual(row) {
  return (row.origin || '').toLowerCase() === 'manual'
}

function originLink(row) {
  if (!row.referenceType || !row.referenceId) return null
  const type = row.referenceType.toLowerCase()
  if (type === 'sale') return { path: '/sales', query: { ref: row.referenceId } }
  if (type === 'purchase') return { path: '/raw-materials', query: { ref: row.referenceId } }
  if (type === 'expense') return { path: '/expenses', query: { ref: row.referenceId } }
  return null
}

function originLabel(row) {
  if (!row.referenceType) return row.origin || '-'
  const labels = {
    Sale: 'Venta',
    Purchase: 'Compra',
    Expense: 'Gasto'
  }
  return labels[row.referenceType] || row.referenceType
}

onMounted(loadData)
</script>

<style scoped>
.view-container {
  padding: 24px 32px;
  max-width: 1280px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  align-items: flex-end;
  margin-bottom: 24px;
}

.page-title {
  font-size: 1.8rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-text, #1a1a1a);
}

.page-subtitle {
  color: var(--color-text-secondary, #6b6b76);
  margin: 4px 0 0;
}

.page-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.btn {
  padding: 8px 14px;
  border-radius: var(--radius, 6px);
  border: 1px solid transparent;
  font-weight: 600;
  cursor: pointer;
}

.btn-primary {
  background: var(--color-primary, #5b5bd6);
  color: #fff;
}

.btn-secondary {
  background: var(--color-bg-secondary, #f0f0f2);
  color: var(--color-text, #1a1a1a);
  border-color: var(--color-border, #e5e5e7);
}

.page-alert {
  padding: 12px 14px;
  margin-bottom: 18px;
  border-radius: 6px;
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
}

.kpi-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 18px;
  margin-bottom: 22px;
}

.filters-container {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  background: var(--color-bg, #ffffff);
  padding: 16px 18px;
  border-radius: var(--radius-lg, 8px);
  border: 1px solid var(--color-border, #e5e5e7);
  margin-bottom: 18px;
  align-items: flex-end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-secondary, #6b6b76);
}

.form-control {
  min-width: 150px;
  padding: 8px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font: inherit;
  background: #fff;
}

.totals-row {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
  margin-bottom: 18px;
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.9rem;
}

.text-positive {
  color: var(--color-positive, #059669);
}

.text-negative {
  color: var(--color-negative, #c53030);
}

.badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 600;
}

.badge-positive {
  background: #e7f8ef;
  color: #047857;
}

.badge-negative {
  background: #fde8e8;
  color: #b91c1c;
}

.badge-neutral {
  background: #f0f0f2;
  color: #4b5563;
}

.origin-muted {
  color: var(--color-text-muted, #9b9ba7);
}

.num {
  font-variant-numeric: tabular-nums;
  font-weight: 700;
}

@media (max-width: 720px) {
  .view-container {
    padding: 20px 16px;
  }

  .page-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .page-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .form-control {
    min-width: 100%;
  }

  .filter-group {
    width: 100%;
  }
}
</style>
