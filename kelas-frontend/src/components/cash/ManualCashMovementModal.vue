<template>
  <AppModal :show="show" title="Nuevo Movimiento Manual" width="560px" @close="handleClose">
    <form class="cash-form" @submit.prevent="submit">
      <div v-if="error" class="form-alert">{{ error }}</div>

      <div class="form-row">
        <label class="form-group">
          <span>Tipo</span>
          <select v-model="form.type" class="form-control" required>
            <option value="Ingreso">Ingreso</option>
            <option value="Egreso">Egreso</option>
          </select>
        </label>

        <label class="form-group">
          <span>Cuenta</span>
          <select v-model="form.cashAccountId" class="form-control" required>
            <option value="">Seleccionar...</option>
            <option v-for="account in accounts" :key="account.id" :value="account.id">
              {{ account.name }} ({{ formatCurrency(account.currentBalance) }})
            </option>
          </select>
        </label>
      </div>

      <div class="form-row">
        <label class="form-group">
          <span>Monto</span>
          <input v-model.number="form.amount" class="form-control" type="number" min="0" step="0.01" required />
        </label>

        <label class="form-group">
          <span>Fecha</span>
          <input v-model="form.date" class="form-control" type="date" required />
        </label>
      </div>

      <label class="form-group">
        <span>Concepto</span>
        <select v-model="form.concept" class="form-control" required>
          <option value="">Seleccionar...</option>
          <option v-for="concept in concepts" :key="concept" :value="concept">{{ concept }}</option>
        </select>
      </label>

      <label class="form-group">
        <span>Notas</span>
        <textarea v-model="form.description" class="form-control" rows="3" placeholder="Detalle del movimiento..." />
      </label>
    </form>

    <template #footer>
      <button class="btn btn-secondary" type="button" :disabled="saving" @click="handleClose">Cancelar</button>
      <button class="btn btn-primary" type="button" :disabled="saving" @click="submit">
        {{ saving ? 'Registrando...' : 'Registrar' }}
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { reactive, ref, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import cashMovementService from '@/services/cashMovementService'
import { formatCurrency, toDateInputValue } from '@/utils/format'

const props = defineProps({
  show: { type: Boolean, default: false },
  accounts: { type: Array, default: () => [] }
})

const emit = defineEmits(['close', 'saved'])

const concepts = ['Retiro de efectivo', 'Aporte de capital', 'Ajuste de saldo', 'Transferencia', 'Otro']
const saving = ref(false)
const error = ref('')
const form = reactive(defaultForm())

watch(() => props.show, (show) => {
  if (show) reset()
})

function defaultForm() {
  return {
    type: 'Ingreso',
    cashAccountId: '',
    amount: null,
    concept: '',
    date: toDateInputValue(),
    description: ''
  }
}

function reset() {
  Object.assign(form, defaultForm())
  error.value = ''
}

function handleClose() {
  if (!saving.value) emit('close')
}

async function submit() {
  error.value = ''

  if (!form.cashAccountId || !form.amount || form.amount <= 0 || !form.concept || !form.date) {
    error.value = 'Completá los campos obligatorios.'
    return
  }

  const account = props.accounts.find(a => a.id === form.cashAccountId)
  if (form.type === 'Egreso' && account && account.currentBalance - form.amount < 0) {
    const confirmed = window.confirm('Este egreso dejará la cuenta con saldo negativo. ¿Querés continuar?')
    if (!confirmed) return
  }

  saving.value = true
  try {
    await cashMovementService.createManual({
      cashAccountId: form.cashAccountId,
      type: form.type,
      amount: form.amount,
      concept: form.concept,
      date: `${form.date}T00:00:00.000Z`,
      description: form.description
    })
    emit('saved')
  } catch (e) {
    error.value = e.response?.data?.error || 'No se pudo registrar el movimiento.'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.cash-form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 0.84rem;
  font-weight: 600;
  color: var(--color-text-secondary, #6b6b76);
}

.form-control {
  width: 100%;
  padding: 9px 11px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font: inherit;
  color: var(--color-text, #1a1a1a);
  background: #fff;
}

.form-alert {
  padding: 10px 12px;
  border-radius: 6px;
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
  font-size: 0.88rem;
}

.btn {
  padding: 8px 14px;
  border-radius: var(--radius, 6px);
  border: 1px solid transparent;
  font-weight: 600;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.65;
  cursor: wait;
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

@media (max-width: 640px) {
  .form-row {
    grid-template-columns: 1fr;
  }
}
</style>
