<template>
  <AppModal :show="show" :title="`Historial de Movimientos — ${item?.name ?? ''}`" @close="$emit('close')">
    <template #default>
      <!-- Loading state -->
      <div v-if="loading" class="loading-state">
        <span class="loading-spinner" aria-label="Cargando..."></span>
        <span>Cargando movimientos...</span>
      </div>

      <!-- Error state -->
      <p v-else-if="loadError" class="load-error">{{ loadError }}</p>

      <!-- Empty state -->
      <p v-else-if="movements.length === 0" class="empty-state">
        No hay movimientos registrados para {{ itemLabel }}.
      </p>

      <!-- Movements table -->
      <div v-else class="table-wrapper">
        <table class="movements-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Tipo</th>
              <th>Cantidad</th>
              <th>Motivo</th>
              <th>Notas</th>
              <th>Origen</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="movement in movements" :key="movement.id">
              <td class="col-date">{{ formatDate(movement.date) }}</td>
              <td class="col-type">{{ formatMovementType(movement.movementType) }}</td>
              <td
                class="col-quantity"
                :class="movement.quantity >= 0 ? 'quantity-positive' : 'quantity-negative'"
              >
                {{ movement.quantity >= 0 ? '+' : '' }}{{ movement.quantity }}
              </td>
              <td class="col-reason">
                <span v-if="isAdjustment(movement.movementType)">
                  {{ movement.adjustmentReason ?? '—' }}
                </span>
                <span v-else class="text-muted">—</span>
              </td>
              <td class="col-notes">{{ movement.notes ?? '—' }}</td>
              <td class="col-origin">{{ formatOrigin(movement) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <template #footer>
      <button class="btn" @click="$emit('close')">Cerrar</button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import stockAdjustmentService from '@/services/stockAdjustmentService'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  item: {
    type: Object,
    default: null
    // Expected shape: { id, name }
  },
  itemType: {
    type: String,
    default: 'RawMaterial'
  }
})

defineEmits(['close'])

// State
const movements = ref([])
const loading = ref(false)
const loadError = ref('')

// Movement type labels
const movementTypeLabels = {
  PurchaseEntry: 'Compra',
  AdjustmentIncrease: 'Ajuste (Ingreso)',
  AdjustmentDecrease: 'Ajuste (Egreso)',
  ProductionConsumption: 'Consumo Producción',
  ProductionOutput: 'Ingreso Producción',
  SaleDelivery: 'Entrega Venta',
  SaleOutput: 'Salida Venta',
  InitialStock: 'Stock Inicial'
}

const itemTypeLabels = {
  RawMaterial: 'esta materia prima',
  FinishedProduct: 'este producto'
}

const itemLabel = itemTypeLabels[props.itemType] ?? 'este ítem'

// Methods
function formatDate(dateStr) {
  if (!dateStr) return '—'
  return new Intl.DateTimeFormat('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(new Date(dateStr))
}

function formatMovementType(type) {
  return movementTypeLabels[type] ?? type
}

function isAdjustment(type) {
  return type === 'AdjustmentIncrease' || type === 'AdjustmentDecrease'
}

function formatOrigin(movement) {
  if (!movement.referenceType) return '—'
  const labels = {
    Purchase: 'Compra',
    Production: 'Producción',
    ProductionBatch: 'Producción',
    Sale: 'Venta'
  }
  const label = labels[movement.referenceType] ?? movement.referenceType
  return movement.referenceId ? `${label} #${movement.referenceId.slice(-6)}` : label
}

async function fetchMovements() {
  if (!props.item?.id) return

  loading.value = true
  loadError.value = ''
  movements.value = []

  try {
    const response = await stockAdjustmentService.getMovements(props.itemType, props.item.id)
    movements.value = response.data
  } catch {
    loadError.value = 'No se pudieron cargar los movimientos. Intentá nuevamente.'
  } finally {
    loading.value = false
  }
}

// Fetch on open
watch(() => props.show, (newVal) => {
  if (newVal) {
    fetchMovements()
  } else {
    movements.value = []
    loadError.value = ''
  }
})
</script>

<style scoped>
.loading-state {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 24px 0;
  color: var(--color-text-muted, #9b9ba7);
  font-size: 0.9rem;
}

.loading-spinner {
  display: inline-block;
  width: 18px;
  height: 18px;
  border: 2px solid var(--color-border, #e5e5e7);
  border-top-color: var(--color-primary, #5b5bd6);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.load-error {
  padding: 12px 14px;
  font-size: 0.85rem;
  color: var(--color-negative, #c53030);
  background: var(--color-negative-bg, #fde8e8);
  border-radius: var(--radius, 6px);
  margin: 0;
}

.empty-state {
  padding: 24px 0;
  text-align: center;
  color: var(--color-text-muted, #9b9ba7);
  font-size: 0.9rem;
  margin: 0;
}

.table-wrapper {
  overflow-x: auto;
}

.movements-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.83rem;
}

.movements-table th {
  text-align: left;
  padding: 8px 10px;
  font-weight: 600;
  color: var(--color-text-muted, #9b9ba7);
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  white-space: nowrap;
}

.movements-table td {
  padding: 9px 10px;
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  vertical-align: top;
}

.movements-table tr:last-child td {
  border-bottom: none;
}

.col-date {
  white-space: nowrap;
  color: var(--color-text-secondary, #6b6b76);
}

.col-type {
  white-space: nowrap;
}

.col-quantity {
  font-weight: 600;
  white-space: nowrap;
  text-align: right;
}

.quantity-positive {
  color: var(--color-positive, #2d7a2d);
}

.quantity-negative {
  color: var(--color-negative, #c53030);
}

.col-reason,
.col-notes,
.col-origin {
  color: var(--color-text-secondary, #6b6b76);
  max-width: 160px;
  word-break: break-word;
}

.text-muted {
  color: var(--color-text-muted, #9b9ba7);
}
</style>
