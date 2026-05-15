<template>
  <AppModal :show="show" title="Registrar Gasto" @close="close">
    <form id="expense-form" @submit.prevent="submitForm">
      <FormField label="Fecha" :error="errors.date" required>
        <input type="date" v-model="form.date" class="form-control" />
      </FormField>

      <FormField label="Categoría" :error="errors.category" required>
        <select v-model="form.category" class="form-control">
          <option value="" disabled>Seleccione una categoría</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
      </FormField>

      <FormField label="Monto" :error="errors.amount" required>
        <input type="number" v-model.number="form.amount" class="form-control" step="0.01" min="0.01" />
      </FormField>

      <FormField label="Cuenta de Pago" :error="errors.cashAccountId" required>
        <select v-model="form.cashAccountId" class="form-control" :disabled="loadingAccounts">
          <option value="" disabled>Seleccione una cuenta</option>
          <option v-for="acc in cashAccounts" :key="acc.id" :value="acc.id">
            {{ acc.name }} (Saldo: {{ formatCurrency(acc.currentBalance) }})
          </option>
        </select>
      </FormField>

      <FormField label="Descripción" :error="errors.description" required>
        <textarea v-model="form.description" class="form-control" rows="3" placeholder="Detalle del gasto..."></textarea>
      </FormField>
      
      <div v-if="submitError" class="alert alert-danger mt-3">
        {{ submitError }}
      </div>
    </form>

    <template #footer>
      <button type="button" class="btn btn-secondary" @click="close" :disabled="isSubmitting">Cancelar</button>
      <button type="submit" form="expense-form" class="btn btn-primary" :disabled="isSubmitting">
        {{ isSubmitting ? 'Guardando...' : 'Guardar Gasto' }}
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import cashAccountService from '@/services/cashAccountService'
import expenseService from '@/services/expenseService'
import { formatCurrency, toDateInputValue, toUtcDateStart } from '@/utils/format'

const props = defineProps({
  show: Boolean
})

const emit = defineEmits(['close', 'saved'])

const categories = ['Marketing', 'Packaging', 'Envíos', 'Alquiler', 'Servicios', 'Herramientas', 'Otro']

const cashAccounts = ref([])
const loadingAccounts = ref(false)

const form = reactive({
  date: toDateInputValue(),
  category: '',
  amount: null,
  cashAccountId: '',
  description: ''
})

const errors = reactive({})
const submitError = ref('')
const isSubmitting = ref(false)

async function loadCashAccounts() {
  loadingAccounts.value = true
  try {
    const res = await cashAccountService.getAll()
    cashAccounts.value = res.data
  } catch (e) {
    console.error('Error al cargar cuentas', e)
  } finally {
    loadingAccounts.value = false
  }
}

onMounted(() => {
  loadCashAccounts()
})

function validate() {
  Object.keys(errors).forEach(k => delete errors[k])
  let isValid = true

  if (!form.date) { errors.date = 'La fecha es obligatoria'; isValid = false }
  if (!form.category) { errors.category = 'Seleccione una categoría'; isValid = false }
  if (!form.amount || form.amount <= 0) { errors.amount = 'El monto debe ser mayor a 0'; isValid = false }
  if (!form.cashAccountId) { errors.cashAccountId = 'Seleccione una cuenta de pago'; isValid = false }
  if (!form.description.trim()) { errors.description = 'La descripción es obligatoria'; isValid = false }

  return isValid
}

async function submitForm() {
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await expenseService.create({
      date: toUtcDateStart(form.date),
      amount: form.amount,
      category: form.category,
      description: form.description,
      cashAccountId: form.cashAccountId
    })
    
    // Reset form
    form.category = ''
    form.amount = null
    form.description = ''
    form.cashAccountId = ''
    
    emit('saved')
    emit('close')
  } catch (e) {
    submitError.value = e.response?.data?.error || 'Ocurrió un error al registrar el gasto'
  } finally {
    isSubmitting.value = false
  }
}

function close() {
  emit('close')
}
</script>

<style scoped>
.form-control {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-family: inherit;
  font-size: 0.95rem;
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

.alert {
  padding: 10px;
  border-radius: 6px;
  font-size: 0.9rem;
}

.alert-danger {
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
  border: 1px solid #f8b4b4;
}

.mt-3 {
  margin-top: 1rem;
}
</style>
