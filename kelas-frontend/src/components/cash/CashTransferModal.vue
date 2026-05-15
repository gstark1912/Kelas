<template>
  <AppModal :show="show" title="Transferencia entre Cuentas" width="620px" @close="handleClose">
    <form class="cash-form" @submit.prevent="submit">
      <div v-if="error" class="form-alert">{{ error }}</div>

      <div class="form-row">
        <label class="form-group">
          <span>Cuenta origen</span>
          <select v-model="form.originCashAccountId" class="form-control" required>
            <option value="">Seleccionar...</option>
            <option v-for="account in accounts" :key="account.id" :value="account.id">
              {{ account.name }} ({{ formatCurrency(account.currentBalance) }})
            </option>
          </select>
        </label>

        <label class="form-group">
          <span>Cuenta destino</span>
          <select v-model="form.destinationCashAccountId" class="form-control" required>
            <option value="">Seleccionar...</option>
            <option v-for="account in accounts" :key="account.id" :value="account.id" :disabled="account.id === form.originCashAccountId">
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
        <span>Notas</span>
        <textarea v-model="form.description" class="form-control" rows="3" placeholder="Detalle opcional..." />
      </label>

      <div class="hint">Se generarán dos movimientos: egreso en origen e ingreso en destino.</div>
    </form>

    <template #footer>
      <button class="btn btn-secondary" type="button" :disabled="saving" @click="handleClose">Cancelar</button>
      <button class="btn btn-primary" type="button" :disabled="saving" @click="submit">
        {{ saving ? 'Transfiriendo...' : 'Transferir' }}
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

const saving = ref(false)
const error = ref('')
const form = reactive(defaultForm())

watch(() => props.show, (show) => {
  if (show) reset()
})

function defaultForm() {
  return {
    originCashAccountId: '',
    destinationCashAccountId: '',
    amount: null,
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

  if (!form.originCashAccountId || !form.destinationCashAccountId || !form.amount || form.amount <= 0 || !form.date) {
    error.value = 'Completá los campos obligatorios.'
    return
  }

  if (form.originCashAccountId === form.destinationCashAccountId) {
    error.value = 'La cuenta origen y destino no pueden ser la misma.'
    return
  }

  const origin = props.accounts.find(a => a.id === form.originCashAccountId)
  if (origin && origin.currentBalance - form.amount < 0) {
    const confirmed = window.confirm('Esta transferencia dejará la cuenta origen con saldo negativo. ¿Querés continuar?')
    if (!confirmed) return
  }

  saving.value = true
  try {
    await cashMovementService.createTransfer({
      originCashAccountId: form.originCashAccountId,
      destinationCashAccountId: form.destinationCashAccountId,
      amount: form.amount,
      date: `${form.date}T00:00:00.000Z`,
      description: form.description
    })
    emit('saved')
  } catch (e) {
    error.value = e.response?.data?.error || 'No se pudo registrar la transferencia.'
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

.hint {
  background: var(--color-bg-secondary, #f7f7f8);
  border-radius: 6px;
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.86rem;
  padding: 10px 12px;
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
