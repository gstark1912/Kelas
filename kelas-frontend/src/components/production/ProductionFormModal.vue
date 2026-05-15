<template>
  <AppModal :show="show" title="Nueva Producción" @close="handleClose">
    <form @submit.prevent="handleSubmit">
      <FormField label="Producto" :error="formErrors.productId" :required="true">
        <select v-model="form.productId" :disabled="submitting || confirmingInsufficientStock">
          <option value="" disabled>Seleccionar...</option>
          <option v-for="product in products" :key="product.id" :value="product.id">
            {{ product.name }}
          </option>
        </select>
      </FormField>

      <div class="form-row">
        <FormField label="Cantidad" :error="formErrors.quantity" :required="true">
          <input
            v-model.number="form.quantity"
            type="number"
            min="0"
            step="any"
            :disabled="submitting || confirmingInsufficientStock"
          />
        </FormField>

        <FormField label="Fecha" :error="formErrors.date" :required="true">
          <input
            v-model="form.date"
            type="date"
            :disabled="submitting || confirmingInsufficientStock"
          />
        </FormField>
      </div>

      <FormField label="Notas">
        <textarea
          v-model="form.notes"
          rows="3"
          placeholder="Opcional"
          :disabled="submitting || confirmingInsufficientStock"
        ></textarea>
      </FormField>

      <div v-if="insufficientItems.length" class="warning-box">
        <h3>Stock insuficiente</h3>
        <p>Podés cancelar o continuar igual. Si continuás, el stock de MP puede quedar negativo.</p>
        <div class="warning-table">
          <div class="warning-row warning-head">
            <span>MP</span>
            <span>Necesario</span>
            <span>Disponible</span>
            <span>Faltante</span>
          </div>
          <div v-for="item in insufficientItems" :key="item.rawMaterialId" class="warning-row">
            <span>{{ item.rawMaterialName }}</span>
            <span>{{ formatNumber(item.requiredQuantity) }}</span>
            <span>{{ formatNumber(item.availableQuantity) }}</span>
            <span>{{ formatNumber(item.missingQuantity) }}</span>
          </div>
        </div>
      </div>

      <p v-if="formError" class="form-general-error">{{ formError }}</p>
    </form>

    <template #footer>
      <button class="btn" :disabled="submitting" @click="handleClose">
        Cancelar
      </button>
      <button
        v-if="insufficientItems.length"
        class="btn btn-primary"
        :disabled="submitting"
        @click="confirmAndSubmit"
      >
        <span v-if="submitting">Registrando...</span>
        <span v-else>Continuar igual</span>
      </button>
      <button
        v-else
        class="btn btn-primary"
        :disabled="submitting || !isFormValid"
        @click="handleSubmit"
      >
        <span v-if="submitting">Registrando...</span>
        <span v-else>Registrar</span>
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import productionService from '@/services/productionService'
import { toDateInputValue, toUtcDateStart } from '@/utils/format'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  products: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['close', 'created'])

const submitting = ref(false)
const confirmingInsufficientStock = ref(false)
const formError = ref('')
const formErrors = ref({ productId: '', quantity: '', date: '' })
const insufficientItems = ref([])
const form = ref(createEmptyForm())

const isFormValid = computed(() => {
  return form.value.productId && form.value.quantity > 0 && form.value.date
})

watch(() => props.show, (show) => {
  if (show) resetForm()
})

function createEmptyForm() {
  return {
    productId: '',
    quantity: null,
    date: toDateInputValue(),
    notes: ''
  }
}

function resetForm() {
  form.value = createEmptyForm()
  formError.value = ''
  formErrors.value = { productId: '', quantity: '', date: '' }
  insufficientItems.value = []
  confirmingInsufficientStock.value = false
  submitting.value = false
}

function validateForm() {
  formErrors.value = { productId: '', quantity: '', date: '' }

  if (!form.value.productId) formErrors.value.productId = 'El producto es obligatorio.'
  if (!form.value.quantity || form.value.quantity <= 0) formErrors.value.quantity = 'La cantidad debe ser mayor a 0.'
  if (!form.value.date) formErrors.value.date = 'La fecha es obligatoria.'

  return !formErrors.value.productId && !formErrors.value.quantity && !formErrors.value.date
}

function buildPayload(confirmInsufficientStock = false) {
  return {
    productId: form.value.productId,
    quantity: Number(form.value.quantity),
    date: toUtcDateStart(form.value.date),
    notes: form.value.notes?.trim() || null,
    confirmInsufficientStock
  }
}

async function handleSubmit() {
  insufficientItems.value = []
  await submit(false)
}

async function confirmAndSubmit() {
  await submit(true)
}

async function submit(confirmInsufficientStock) {
  if (!validateForm() || submitting.value) return

  formError.value = ''
  submitting.value = true

  try {
    const response = await productionService.create(buildPayload(confirmInsufficientStock))
    emit('created', response.data)
    resetForm()
  } catch (err) {
    const data = err.response?.data
    if (data?.code === 'INSUFFICIENT_STOCK' && Array.isArray(data.items)) {
      insufficientItems.value = data.items
      confirmingInsufficientStock.value = true
      formError.value = ''
    } else {
      formError.value = data?.error || 'No se pudo registrar la producción.'
    }
  } finally {
    submitting.value = false
  }
}

function handleClose() {
  if (submitting.value) return
  resetForm()
  emit('close')
}

function formatNumber(value) {
  if (value == null) return '0'
  return Number(value).toLocaleString('es-AR')
}
</script>

<style scoped>
.warning-box {
  border: 1px solid var(--color-warning-bg, #fef3c7);
  background: #fffbeb;
  border-radius: var(--radius-lg, 8px);
  padding: 12px;
  margin-bottom: 14px;
}

.warning-box h3 {
  margin: 0 0 4px;
  font-size: 0.9rem;
  color: var(--color-warning, #b45309);
}

.warning-box p {
  margin: 0 0 10px;
  font-size: 0.82rem;
  color: var(--color-text-secondary, #6b6b76);
}

.warning-table {
  display: grid;
  gap: 4px;
  font-size: 0.8rem;
}

.warning-row {
  display: grid;
  grid-template-columns: 1.4fr repeat(3, minmax(70px, 0.7fr));
  gap: 8px;
  align-items: center;
}

.warning-row span:not(:first-child) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.warning-head {
  font-weight: 700;
  color: var(--color-text-secondary, #6b6b76);
}
</style>
