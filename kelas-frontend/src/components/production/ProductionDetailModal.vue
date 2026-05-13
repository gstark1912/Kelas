<template>
  <AppModal :show="show" title="Detalle de Producción" @close="$emit('close')">
    <div v-if="batch" class="detail">
      <div class="summary-grid">
        <div>
          <span>Producto</span>
          <strong>{{ batch.productName }}</strong>
        </div>
        <div>
          <span>Fecha</span>
          <strong>{{ formatDate(batch.date) }}</strong>
        </div>
        <div>
          <span>Cantidad</span>
          <strong>{{ formatNumber(batch.quantity) }}</strong>
        </div>
        <div>
          <span>Costo total</span>
          <strong>{{ formatCurrency(batch.totalCost) }}</strong>
        </div>
        <div>
          <span>Costo unitario</span>
          <strong>{{ formatCurrency(batch.unitCost) }}</strong>
        </div>
      </div>

      <div v-if="batch.notes" class="notes">
        {{ batch.notes }}
      </div>

      <h3>Ingredientes</h3>
      <div class="ingredients-table">
        <div class="ingredients-row ingredients-head">
          <span>Materia Prima</span>
          <span>Cantidad</span>
          <span>Precio</span>
          <span>Costo</span>
        </div>
        <div v-for="item in batch.ingredients" :key="item.rawMaterialId" class="ingredients-row">
          <span>{{ item.rawMaterialName }}</span>
          <span>{{ formatNumber(item.quantityUsed) }}</span>
          <span>{{ formatCurrency(item.pricePerUnit) }}</span>
          <span>{{ formatCurrency(item.cost) }}</span>
        </div>
      </div>
    </div>

    <template #footer>
      <button class="btn" @click="$emit('close')">Cerrar</button>
    </template>
  </AppModal>
</template>

<script setup>
import AppModal from '@/components/common/AppModal.vue'

defineProps({
  show: {
    type: Boolean,
    default: false
  },
  batch: {
    type: Object,
    default: null
  }
})

defineEmits(['close'])

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
</script>

<style scoped>
.detail {
  display: grid;
  gap: 16px;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.summary-grid div {
  display: grid;
  gap: 3px;
}

.summary-grid span {
  font-size: 0.75rem;
  text-transform: uppercase;
  color: var(--color-text-secondary, #6b6b76);
  font-weight: 600;
}

.summary-grid strong {
  font-size: 0.95rem;
  color: var(--color-text, #1a1a1a);
}

.notes {
  padding: 10px 12px;
  border-radius: var(--radius, 6px);
  background: var(--color-bg-secondary, #f7f7f8);
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.86rem;
}

h3 {
  margin: 0;
  font-size: 0.95rem;
}

.ingredients-table {
  display: grid;
  gap: 6px;
  font-size: 0.84rem;
}

.ingredients-row {
  display: grid;
  grid-template-columns: 1.5fr repeat(3, minmax(72px, 0.75fr));
  gap: 8px;
  padding: 6px 0;
  border-bottom: 1px solid var(--color-border-light, #efefef);
}

.ingredients-row span:not(:first-child) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.ingredients-head {
  font-weight: 700;
  color: var(--color-text-secondary, #6b6b76);
  text-transform: uppercase;
  font-size: 0.72rem;
}

.btn {
  padding: 7px 14px;
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  border: 1px solid var(--color-border, #e5e5e7);
  background: var(--color-bg, #ffffff);
  color: var(--color-text, #1a1a1a);
  cursor: pointer;
}
</style>
