<template>
  <AppModal :show="show" title="Ajustar Stock" @close="handleClose">
    <template #default>
      <!-- Modal header info -->
      <div class="modal-info">
        <span class="modal-info-label">Materia Prima</span>
        <span class="modal-info-value">{{ rawMaterial?.name }}</span>
        <span class="modal-info-label">Stock Actual</span>
        <span class="modal-info-value">{{ rawMaterial?.currentQuantity ?? 0 }}</span>
      </div>

      <!-- Confirmation overlay for same stock -->
      <div v-if="showConfirm" class="confirm-overlay">
        <div class="confirm-box">
          <p class="confirm-message">El nuevo stock es igual al actual. ¿Desea continuar?</p>
          <div class="confirm-actions">
            <button class="btn" @click="showConfirm = false">Cancelar</button>
          </div>
        </div>
      </div>

      <form v-else @submit.prevent="handleSubmit">
        <FormField label="Nuevo Stock" :error="formErrors.quantity" :required="true">
          <input
            v-model.number="form.newStock"
            type="number"
            min="0"
            step="any"
            :placeholder="`Stock actual: ${rawMaterial?.currentQuantity ?? 0}`"
            :disabled="submitting"
          />
        </FormField>
        <p v-if="delta !== null" class="delta-preview" :class="delta >= 0 ? 'delta-positive' : 'delta-negative'">
          {{ delta >= 0 ? `+${delta}` : delta }} respecto al stock actual
        </p>

        <FormField label="Motivo" :error="formErrors.reason" :required="true">
          <select v-model="form.reason" :disabled="submitting">
            <option value="" disabled>Seleccionar motivo...</option>
            <option v-for="reason in validReasons" :key="reason" :value="reason">
              {{ reason }}
            </option>
          </select>
        </FormField>

        <FormField label="Fecha" :error="formErrors.date" :required="true">
          <input
            v-model="form.date"
            type="date"
            :disabled="submitting"
          />
        </FormField>

        <FormField label="Notas">
          <textarea
            v-model="form.notes"
            placeholder="Observaciones opcionales..."
            rows="3"
            :disabled="submitting"
          ></textarea>
        </FormField>

        <p v-if="formError" class="form-general-error">{{ formError }}</p>
      </form>
    </template>

    <template #footer>
      <button class="btn" @click="handleClose" :disabled="submitting">
        Cancelar
      </button>
      <button
        class="btn btn-primary"
        @click="handleSubmit"
        :disabled="submitting || !isFormValid"
      >
        <span v-if="submitting">Registrando...</span>
        <span v-else>Confirmar</span>
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import stockAdjustmentService from '@/services/stockAdjustmentService'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  rawMaterial: {
    type: Object,
    default: null
    // Expected shape: { id, name, currentQuantity }
  }
})

const emit = defineEmits(['close', 'adjustment-created'])

const validReasons = [
  'Vencimiento',
  'Rotura',
  'Pérdida',
  'Corrección de inventario',
  'Otro'
]

// State
const submitting = ref(false)
const showConfirm = ref(false)
const formError = ref('')
const formErrors = ref({
  quantity: '',
  reason: '',
  date: ''
})

const form = ref({
  newStock: null,
  reason: '',
  date: getTodayDate(),
  notes: ''
})

// Computed
const isFormValid = computed(() => {
  return (
    form.value.newStock !== null &&
    form.value.newStock >= 0 &&
    form.value.reason !== '' &&
    form.value.date !== ''
  )
})

const delta = computed(() => {
  if (form.value.newStock === null) return null
  const current = props.rawMaterial?.currentQuantity ?? 0
  return form.value.newStock - current
})

// Methods
function getTodayDate() {
  const now = new Date()
  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function resetForm() {
  form.value = {
    newStock: null,
    reason: '',
    date: getTodayDate(),
    notes: ''
  }
  formError.value = ''
  formErrors.value = { quantity: '', reason: '', date: '' }
  showConfirm.value = false
  submitting.value = false
}

function handleClose() {
  if (!submitting.value) {
    resetForm()
    emit('close')
  }
}

function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  formError.value = ''
  formErrors.value = { quantity: '', reason: '', date: '' }

  submitAdjustment()
}

async function submitAdjustment() {
  showConfirm.value = false
  submitting.value = true

  const payload = {
    rawMaterialId: props.rawMaterial?.id,
    newStock: form.value.newStock,
    reason: form.value.reason,
    date: form.value.date,
    notes: form.value.notes || null
  }

  try {
    await stockAdjustmentService.create(payload)
    emit('adjustment-created')
    resetForm()
    emit('close')
  } catch (err) {
    if (err.response && (err.response.status === 400 || err.response.status === 404)) {
      formError.value = err.response.data?.error || 'Error de validación'
    } else {
      formError.value = 'Ocurrió un error inesperado. Intentá nuevamente.'
    }
  } finally {
    submitting.value = false
  }
}

// Watchers
watch(() => props.show, (newVal) => {
  if (newVal) {
    resetForm()
  }
})
</script>

<style scoped>
.modal-info {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 4px 12px;
  margin-bottom: 20px;
  padding: 12px 14px;
  background: var(--color-bg-secondary, #f7f7f8);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
}

.modal-info-label {
  color: var(--color-text-muted, #9b9ba7);
  font-weight: 500;
}

.modal-info-value {
  color: var(--color-text, #1a1a1a);
  font-weight: 600;
}

.confirm-overlay {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px 0;
}

.confirm-box {
  text-align: center;
  max-width: 340px;
}

.confirm-message {
  font-size: 0.95rem;
  color: var(--color-text, #1a1a1a);
  margin-bottom: 20px;
  line-height: 1.5;
}

.confirm-actions {
  display: flex;
  gap: 10px;
  justify-content: center;
}

.form-general-error {
  margin: 0;
  padding: 10px 14px;
  font-size: 0.82rem;
  color: var(--color-negative, #c53030);
  background: var(--color-negative-bg, #fde8e8);
  border-radius: var(--radius, 6px);
}

.delta-preview {
  margin: -8px 0 16px;
  font-size: 0.82rem;
  font-weight: 600;
}

.delta-positive {
  color: var(--color-positive, #2d7a2d);
}

.delta-negative {
  color: var(--color-negative, #c53030);
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

.btn-warning {
  background: var(--color-warning, #d97706);
  color: #fff;
  border-color: var(--color-warning, #d97706);
}

.btn-warning:hover:not(:disabled) {
  background: var(--color-warning-hover, #b45309);
  border-color: var(--color-warning-hover, #b45309);
}
</style>
