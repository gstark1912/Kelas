<template>
  <AppModal :show="show" title="Registrar Compra" @close="$emit('close')">
    <form @submit.prevent="handleSubmit">
      <FormField label="Materia Prima">
        <input
          :value="rawMaterial?.name"
          type="text"
          disabled
        />
      </FormField>

      <div class="form-row">
        <FormField label="Cantidad" :error="formErrors.quantity" :required="true">
          <input
            v-model.number="form.quantity"
            type="number"
            min="0"
            step="any"
            placeholder="Ej: 5"
            :disabled="submitting"
          />
        </FormField>

        <FormField label="Precio Total" :error="formErrors.totalPrice" :required="true">
          <input
            v-model.number="form.totalPrice"
            type="number"
            min="0"
            step="any"
            placeholder="Ej: 15000"
            :disabled="submitting"
          />
        </FormField>
      </div>

      <p v-if="pricePerUnitDisplay" class="price-per-unit">
        Precio por unidad: <strong>{{ pricePerUnitDisplay }}</strong>
      </p>

      <div class="form-row">
        <FormField label="Fecha" :error="formErrors.date" :required="true">
          <input
            v-model="form.date"
            type="date"
            :disabled="submitting"
          />
        </FormField>

        <FormField label="Proveedor">
          <input
            v-model="form.supplier"
            type="text"
            placeholder="Ej: Proveedor SA"
            :disabled="submitting"
          />
        </FormField>
      </div>

      <FormField label="Cuenta de Pago" :error="formErrors.cashAccountId" :required="true">
        <select v-model="form.cashAccountId" :disabled="submitting">
          <option value="" disabled>Seleccionar...</option>
          <option
            v-for="account in activeCashAccounts"
            :key="account.id"
            :value="account.id"
          >
            {{ account.name }}
          </option>
        </select>
      </FormField>

      <FormField label="Notas">
        <textarea
          v-model="form.notes"
          placeholder="Observaciones opcionales..."
          rows="3"
          :disabled="submitting"
        ></textarea>
      </FormField>

      <div class="checkbox-field">
        <label class="checkbox-label">
          <input
            v-model="form.skipPriceUpdate"
            type="checkbox"
            :disabled="submitting"
          />
          Excluir del cálculo de costo (compra con descuento especial)
        </label>
      </div>

      <p v-if="formError" class="form-general-error">{{ formError }}</p>
    </form>

    <template #footer>
      <button class="btn" @click="$emit('close')" :disabled="submitting">
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
import { ref, computed, watch, onMounted } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import purchaseService from '@/services/purchaseService'
import cashAccountService from '@/services/cashAccountService'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  rawMaterial: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close', 'purchase-created'])

// State
const cashAccounts = ref([])
const submitting = ref(false)
const formError = ref('')
const formErrors = ref({
  quantity: '',
  totalPrice: '',
  date: '',
  cashAccountId: ''
})

const form = ref({
  quantity: null,
  totalPrice: null,
  date: getTodayDate(),
  supplier: '',
  cashAccountId: '',
  notes: '',
  skipPriceUpdate: false
})

// Computed
const activeCashAccounts = computed(() => {
  return cashAccounts.value.filter(a => a.isActive)
})

const currencyFormatter = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'ARS',
  minimumFractionDigits: 2
})

const pricePerUnit = computed(() => {
  const q = form.value.quantity
  const p = form.value.totalPrice
  if (q > 0 && p > 0) {
    return p / q
  }
  return null
})

const pricePerUnitDisplay = computed(() => {
  if (pricePerUnit.value !== null) {
    return currencyFormatter.format(pricePerUnit.value)
  }
  return ''
})

const isFormValid = computed(() => {
  return (
    form.value.quantity > 0 &&
    form.value.totalPrice > 0 &&
    form.value.date !== '' &&
    form.value.cashAccountId !== ''
  )
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
    quantity: null,
    totalPrice: null,
    date: getTodayDate(),
    supplier: '',
    cashAccountId: '',
    notes: '',
    skipPriceUpdate: false
  }
  formError.value = ''
  formErrors.value = {
    quantity: '',
    totalPrice: '',
    date: '',
    cashAccountId: ''
  }
}

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  formError.value = ''
  formErrors.value = { quantity: '', totalPrice: '', date: '', cashAccountId: '' }
  submitting.value = true

  const payload = {
    rawMaterialId: props.rawMaterial?.id,
    quantity: form.value.quantity,
    totalPrice: form.value.totalPrice,
    date: form.value.date,
    supplier: form.value.supplier || null,
    cashAccountId: form.value.cashAccountId,
    notes: form.value.notes || null,
    skipPriceUpdate: form.value.skipPriceUpdate
  }

  try {
    await purchaseService.create(payload)
    emit('purchase-created')
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

// Lifecycle
onMounted(async () => {
  try {
    const response = await cashAccountService.getAll()
    cashAccounts.value = response.data
  } catch {
    cashAccounts.value = []
  }
})
</script>

<style scoped>
.form-row {
  display: flex;
  gap: 12px;
}

.form-row > * {
  flex: 1;
}

.price-per-unit {
  margin: -8px 0 16px;
  font-size: 0.82rem;
  color: var(--color-text-secondary, #6b6b76);
}

.price-per-unit strong {
  color: var(--color-primary, #5b5bd6);
}

.form-general-error {
  margin: 0;
  padding: 10px 14px;
  font-size: 0.82rem;
  color: var(--color-negative, #c53030);
  background: var(--color-negative-bg, #fde8e8);
  border-radius: var(--radius, 6px);
}

.checkbox-field {
  margin-bottom: 16px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.82rem;
  color: var(--color-text-secondary, #6b6b76);
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  width: auto;
  margin: 0;
  cursor: pointer;
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
</style>
