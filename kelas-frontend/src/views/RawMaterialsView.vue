<template>
  <div class="raw-materials-view">
    <div class="page-header">
      <h1>Materias Primas</h1>
      <button class="btn btn-primary" @click="openCreateModal">
        + Nueva Materia Prima
      </button>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <input
        v-model="searchInput"
        type="text"
        placeholder="Buscar materia prima..."
        style="width: 240px"
        @input="onSearchInput"
      />
      <select v-model="unitFilter" @change="fetchMaterials">
        <option value="">Todas las unidades</option>
        <option v-for="unit in validUnits" :key="unit" :value="unit">
          {{ unit }}
        </option>
      </select>
    </div>

    <!-- Table -->
    <DataTable :columns="columns" :data="materials" :loading="loading">
      <template #cell-name="{ value }">
        <span class="fw-600">{{ value }}</span>
      </template>
      <template #cell-currentQuantity="{ value }">
        <span class="num">{{ formatNumber(value) }}</span>
      </template>
      <template #cell-minStock="{ value }">
        <span class="num">{{ formatNumber(value) }}</span>
      </template>
      <template #cell-lastPricePerUnit="{ value }">
        <span class="num">{{ formatPrice(value) }}</span>
      </template>
      <template #cell-status="{ value }">
        <span class="badge" :class="statusBadgeClass(value)">{{ value }}</span>
      </template>
      <template #cell-lastPurchaseDate="{ value }">
        <span class="num">{{ value ? formatDate(value) : '—' }}</span>
      </template>
      <template #actions="{ row }">
        <div class="action-buttons">
          <button class="btn btn-sm btn-primary" @click="openPurchaseModal(row)">Comprar</button>
          <button class="btn btn-sm" @click="openMovementsModal(row)">Historial</button>
          <button class="btn btn-sm btn-primary" @click="openAdjustmentModal(row)">Ajustar</button>
          <button class="btn btn-sm" @click="openEditModal(row)">Editar</button>
        </div>
      </template>
    </DataTable>

    <!-- Create/Edit Modal -->
    <AppModal
      :show="showModal"
      :title="isEditing ? 'Editar Materia Prima' : 'Nueva Materia Prima'"
      @close="closeModal"
    >
      <form @submit.prevent="handleSubmit">
        <FormField label="Nombre" :error="formErrors.nombre" :required="true">
          <input
            v-model="form.nombre"
            type="text"
            placeholder="Ej: Cera de Soja"
            :disabled="submitting"
          />
        </FormField>

        <div class="form-row">
          <FormField label="Unidad de Medida" :error="formErrors.unidad" :required="true">
            <select v-model="form.unidad" :disabled="submitting">
              <option value="" disabled>Seleccionar...</option>
              <option v-for="unit in validUnits" :key="unit" :value="unit">
                {{ unit }}
              </option>
            </select>
          </FormField>

          <FormField label="Alerta Mínimo de Stock" :error="formErrors.minStock" :required="true">
            <input
              v-model.number="form.minStock"
              type="number"
              min="0"
              step="any"
              placeholder="Ej: 100"
              :disabled="submitting"
            />
          </FormField>
        </div>

        <p v-if="formError" class="form-general-error">{{ formError }}</p>
      </form>

      <template #footer>
        <button class="btn" @click="closeModal" :disabled="submitting">
          Cancelar
        </button>
        <button
          class="btn btn-primary"
          @click="handleSubmit"
          :disabled="submitting || !isFormValid"
        >
          <span v-if="submitting">Guardando...</span>
          <span v-else>{{ isEditing ? 'Guardar' : 'Crear' }}</span>
        </button>
      </template>
    </AppModal>

    <!-- Purchase Modal -->
    <PurchaseModal
      :show="showPurchaseModal"
      :raw-material="selectedRawMaterial"
      @close="closePurchaseModal"
      @purchase-created="onPurchaseCreated"
    />

    <!-- Adjustment Modal -->
    <AdjustmentModal
      :show="showAdjustmentModal"
      :raw-material="selectedAdjustmentMaterial"
      @close="closeAdjustmentModal"
      @adjustment-created="onAdjustmentCreated"
    />

    <!-- Stock Movements Modal -->
    <StockMovementsModal
      :show="showMovementsModal"
      :raw-material="selectedMovementsMaterial"
      @close="closeMovementsModal"
    />

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
import FormField from '@/components/common/FormField.vue'
import rawMaterialService from '@/services/rawMaterialService'
import PurchaseModal from '@/components/raw-materials/PurchaseModal.vue'
import AdjustmentModal from '@/components/raw-materials/AdjustmentModal.vue'
import StockMovementsModal from '@/components/raw-materials/StockMovementsModal.vue'

// Constants
const validUnits = ['gr', 'kg', 'ml', 'lt', 'unidad', 'cm']

const columns = [
  { key: 'name', label: 'Materia Prima' },
  { key: 'unit', label: 'Unidad' },
  { key: 'currentQuantity', label: 'Stock Actual', class: 'num' },
  { key: 'minStock', label: 'Alerta Mín.', class: 'num' },
  { key: 'lastPricePerUnit', label: 'Precio Vigente', class: 'num' },
  { key: 'status', label: 'Estado' },
  { key: 'lastPurchaseDate', label: 'Última Compra', class: 'num' }
]

// State
const materials = ref([])
const loading = ref(false)
const searchInput = ref('')
const unitFilter = ref('')
let searchTimeout = null

// Purchase modal state
const showPurchaseModal = ref(false)
const selectedRawMaterial = ref(null)

// Adjustment modal state
const showAdjustmentModal = ref(false)
const selectedAdjustmentMaterial = ref(null)

// Movements modal state
const showMovementsModal = ref(false)
const selectedMovementsMaterial = ref(null)

// Modal state
const showModal = ref(false)
const isEditing = ref(false)
const editingId = ref(null)
const submitting = ref(false)
const formError = ref('')
const formErrors = ref({ nombre: '', unidad: '', minStock: '' })
const form = ref({
  nombre: '',
  unidad: '',
  minStock: 0
})

// Notification
const notification = ref('')
let notificationTimeout = null

// Computed
const isFormValid = computed(() => {
  return (
    form.value.nombre.trim() !== '' &&
    form.value.unidad !== '' &&
    form.value.minStock >= 0
  )
})

// Methods
function statusBadgeClass(status) {
  switch (status) {
    case 'OK': return 'badge-positive'
    case 'Bajo': return 'badge-warning'
    case 'Sin stock': return 'badge-negative'
    default: return 'badge-neutral'
  }
}

function formatNumber(value) {
  if (value == null) return '0'
  return Number(value).toLocaleString('es-AR')
}

function formatPrice(value) {
  if (value == null || value === 0) return '$0'
  return `$${Number(value).toLocaleString('es-AR', { minimumFractionDigits: 2 })}`
}

function formatDate(dateStr) {
  const date = new Date(dateStr)
  return date.toLocaleDateString('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}

function onSearchInput() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    fetchMaterials()
  }, 300)
}

async function fetchMaterials() {
  loading.value = true
  try {
    const filters = {}
    if (searchInput.value.trim()) filters.search = searchInput.value.trim()
    if (unitFilter.value) filters.unit = unitFilter.value
    const response = await rawMaterialService.getAll(filters)
    materials.value = response.data
  } catch (err) {
    materials.value = []
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  isEditing.value = false
  editingId.value = null
  form.value = { nombre: '', unidad: '', minStock: 0 }
  formError.value = ''
  formErrors.value = { nombre: '', unidad: '', minStock: '' }
  showModal.value = true
}

function openEditModal(row) {
  isEditing.value = true
  editingId.value = row.id
  form.value = {
    nombre: row.name,
    unidad: row.unit,
    minStock: row.minStock
  }
  formError.value = ''
  formErrors.value = { nombre: '', unidad: '', minStock: '' }
  showModal.value = true
}

function closeModal() {
  if (submitting.value) return
  showModal.value = false
}

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  formError.value = ''
  formErrors.value = { nombre: '', unidad: '', minStock: '' }
  submitting.value = true

  const payload = {
    name: form.value.nombre.trim(),
    unit: form.value.unidad,
    minStock: form.value.minStock
  }

  try {
    if (isEditing.value) {
      await rawMaterialService.update(editingId.value, payload)
      showNotification('✓ Materia prima actualizada')
    } else {
      await rawMaterialService.create(payload)
      showNotification('✓ Materia prima creada — stock inicial: 0')
    }
    showModal.value = false
    await fetchMaterials()
  } catch (err) {
    if (err.response && err.response.status === 400) {
      formError.value = err.response.data?.error || 'Error de validación'
    } else {
      formError.value = 'Ocurrió un error inesperado. Intentá nuevamente.'
    }
  } finally {
    submitting.value = false
  }
}

function openPurchaseModal(row) {
  selectedRawMaterial.value = { id: row.id, name: row.name }
  showPurchaseModal.value = true
}

function closePurchaseModal() {
  showPurchaseModal.value = false
  selectedRawMaterial.value = null
}

async function onPurchaseCreated() {
  showNotification('✓ Compra registrada exitosamente')
  await fetchMaterials()
}

function openAdjustmentModal(row) {
  selectedAdjustmentMaterial.value = { id: row.id, name: row.name, currentQuantity: row.currentQuantity }
  showAdjustmentModal.value = true
}

function closeAdjustmentModal() {
  showAdjustmentModal.value = false
  selectedAdjustmentMaterial.value = null
}

async function onAdjustmentCreated() {
  showNotification('✓ Ajuste registrado exitosamente')
  await fetchMaterials()
}

function openMovementsModal(row) {
  selectedMovementsMaterial.value = { id: row.id, name: row.name }
  showMovementsModal.value = true
}

function closeMovementsModal() {
  showMovementsModal.value = false
  selectedMovementsMaterial.value = null
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
  fetchMaterials()
})
</script>

<style scoped>
.raw-materials-view {
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
  letter-spacing: -0.3px;
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

.filters-bar input:focus,
.filters-bar select:focus {
  outline: none;
  border-color: var(--color-primary, #5b5bd6);
}

/* Buttons */
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
  transition: all 0.1s;
  font-family: var(--font, inherit);
}

.btn:hover:not(:disabled) {
  background: var(--color-bg-secondary, #f7f7f8);
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

.btn-primary:hover:not(:disabled) {
  background: var(--color-primary-hover, #4a4ac4);
  border-color: var(--color-primary-hover, #4a4ac4);
}

.btn-sm {
  padding: 4px 10px;
  font-size: 0.8rem;
}

/* Action buttons */
.action-buttons {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

/* Badges */
.badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
}

.badge-positive {
  background: var(--color-positive-bg, #e8f5ea);
  color: var(--color-positive, #2d7a3a);
}

.badge-negative {
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
}

.badge-warning {
  background: var(--color-warning-bg, #fef3c7);
  color: var(--color-warning, #b45309);
}

.badge-neutral {
  background: var(--color-bg-secondary, #f7f7f8);
  color: var(--color-text-secondary, #6b6b76);
}

/* Utilities */
.fw-600 {
  font-weight: 600;
}

.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

/* Form row */
.form-row {
  display: flex;
  gap: 12px;
}

.form-row > * {
  flex: 1;
}

/* Form error */
.form-general-error {
  margin: 0;
  padding: 10px 14px;
  font-size: 0.82rem;
  color: var(--color-negative, #c53030);
  background: var(--color-negative-bg, #fde8e8);
  border-radius: var(--radius, 6px);
}

/* Toast */
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

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>
