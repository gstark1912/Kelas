<template>
  <div class="view-container">
    <div class="page-header">
      <div>
        <h1 class="page-title">Gastos</h1>
        <p class="page-subtitle">Registro de gastos operativos no productivos</p>
      </div>
      <button class="btn btn-primary" @click="showModal = true">
        + Nuevo Gasto
      </button>
    </div>

    <!-- KPIs -->
    <div class="kpi-row">
      <KpiCard 
        title="Total Gastos" 
        :value="formatCurrency(kpis.totalAmount)" 
        :subtitle="`${kpis.totalCount} registros`"
        variant="danger"
      />
      <KpiCard 
        v-if="kpis.topCategory"
        title="Mayor Categoría" 
        :value="kpis.topCategory.category" 
        :subtitle="`${formatCurrency(kpis.topCategory.total)} (${kpis.topCategory.count} regs)`"
        variant="warning"
      />
      <KpiCard 
        v-else
        title="Mayor Categoría" 
        value="-" 
      />
    </div>

    <!-- Filtros simples (ya que FilterBar no está disponible) -->
    <div class="filters-container">
      <div class="filter-group">
        <label>Desde:</label>
        <input type="date" v-model="filters.dateFrom" class="form-control" @change="loadData" />
      </div>
      <div class="filter-group">
        <label>Hasta:</label>
        <input type="date" v-model="filters.dateTo" class="form-control" @change="loadData" />
      </div>
      <div class="filter-group">
        <label>Categoría:</label>
        <select v-model="filters.category" class="form-control" @change="loadData">
          <option value="">Todas</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
      </div>
      <div class="filter-actions">
        <button class="btn btn-secondary" @click="clearFilters">Limpiar</button>
      </div>
    </div>

    <div class="content-grid">
      <!-- Tabla -->
      <div class="table-section">
        <DataTable 
          :columns="columns" 
          :data="items" 
          :loading="loading"
        >
          <template #cell-date="{ value }">
            {{ formatDate(value) }}
          </template>
          <template #cell-amount="{ value }">
            <span class="num text-danger">{{ formatCurrency(value) }}</span>
          </template>
          <template #cell-category="{ value }">
            <span class="badge">{{ value }}</span>
          </template>
        </DataTable>
      </div>

      <!-- Gráfico -->
      <div class="chart-section">
        <div class="chart-card">
          <h3 class="chart-title">Gastos por Categoría</h3>
          <div class="chart-container" v-if="chartData">
            <Pie :data="chartData" :options="chartOptions" />
          </div>
          <div v-else class="empty-chart">
            No hay datos para graficar
          </div>
        </div>
      </div>
    </div>

    <!-- Modal -->
    <ExpenseModal 
      :show="showModal" 
      @close="showModal = false" 
      @saved="loadData" 
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'
import { Pie } from 'vue-chartjs'
import DataTable from '@/components/common/DataTable.vue'
import KpiCard from '@/components/common/KpiCard.vue'
import ExpenseModal from '@/components/expenses/ExpenseModal.vue'
import expenseService from '@/services/expenseService'
import { formatCurrency, formatDate, toDateInputValue, toUtcDateEnd, toUtcDateStart } from '@/utils/format'

ChartJS.register(ArcElement, Tooltip, Legend)

const categories = ['Marketing', 'Packaging', 'Envíos', 'Alquiler', 'Servicios', 'Herramientas', 'Otro']

const columns = [
  { key: 'date', label: 'Fecha' },
  { key: 'amount', label: 'Monto' },
  { key: 'category', label: 'Categoría' },
  { key: 'cashAccountName', label: 'Cuenta' },
  { key: 'description', label: 'Descripción' }
]

const items = ref([])
const kpis = ref({ totalAmount: 0, totalCount: 0, topCategory: null })
const loading = ref(false)
const showModal = ref(false)

const filters = reactive({
  dateFrom: toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1)),
  dateTo: toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)),
  category: ''
})

const chartData = ref(null)
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom',
    }
  }
}

async function loadData() {
  loading.value = true
  try {
    // Cargar tabla y KPIs
    const params = {
      category: filters.category || undefined,
      dateFrom: filters.dateFrom ? toUtcDateStart(filters.dateFrom) : undefined,
      dateTo: filters.dateTo ? toUtcDateEnd(filters.dateTo) : undefined
    }
    
    const res = await expenseService.getAll(params)
    items.value = res.data.items
    kpis.value = res.data.kpis

    // Cargar datos para gráfico si hay rango de fechas
    if (filters.dateFrom && filters.dateTo) {
      const chartRes = await expenseService.getByCategory(
        toUtcDateStart(filters.dateFrom),
        toUtcDateEnd(filters.dateTo)
      )
      
      if (chartRes.data && chartRes.data.length > 0) {
        chartData.value = {
          labels: chartRes.data.map(d => d.category),
          datasets: [
            {
              backgroundColor: ['#5b5bd6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#3b82f6', '#6b7280'],
              data: chartRes.data.map(d => d.total)
            }
          ]
        }
      } else {
        chartData.value = null
      }
    }
  } catch (e) {
    console.error('Error loading expenses:', e)
  } finally {
    loading.value = false
  }
}

function clearFilters() {
  filters.category = ''
  filters.dateFrom = toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth(), 1))
  filters.dateTo = toDateInputValue(new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0))
  loadData()
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.view-container {
  padding: 24px 32px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
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
  margin: 4px 0 0 0;
}

.btn {
  padding: 8px 16px;
  border-radius: var(--radius, 6px);
  font-weight: 500;
  cursor: pointer;
  border: 1px solid transparent;
}

.btn-primary {
  background: var(--color-primary, #5b5bd6);
  color: white;
}

.btn-secondary {
  background: var(--color-bg-secondary, #f0f0f2);
  color: var(--color-text, #1a1a1a);
  border-color: var(--color-border, #e5e5e7);
}

.kpi-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
  margin-bottom: 24px;
}

.filters-container {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  background: var(--color-bg, #ffffff);
  padding: 16px 20px;
  border-radius: var(--radius-lg, 8px);
  border: 1px solid var(--color-border, #e5e5e7);
  margin-bottom: 24px;
  align-items: flex-end;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.filter-group label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.form-control {
  padding: 8px 12px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-family: inherit;
  font-size: 0.9rem;
  min-width: 150px;
}

.content-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 24px;
}

.chart-card {
  background: var(--color-bg, #ffffff);
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius-lg, 8px);
  padding: 20px;
  height: 100%;
  display: flex;
  flex-direction: column;
}

.chart-title {
  font-size: 1rem;
  font-weight: 600;
  margin: 0 0 16px 0;
  color: var(--color-text);
}

.chart-container {
  flex: 1;
  min-height: 250px;
  position: relative;
}

.empty-chart {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  font-size: 0.9rem;
  background: #f9f9fa;
  border-radius: 8px;
}

.badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 12px;
  background: var(--color-bg-secondary, #f0f0f2);
  color: var(--color-text, #1a1a1a);
  font-size: 0.8rem;
  font-weight: 500;
}

.text-danger {
  color: var(--color-negative, #c53030);
  font-weight: 500;
}

@media (max-width: 992px) {
  .content-grid {
    grid-template-columns: 1fr;
  }
}
</style>
