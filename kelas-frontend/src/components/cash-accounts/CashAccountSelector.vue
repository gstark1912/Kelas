<template>
  <div class="cash-account-selector">
    <select
      :value="modelValue"
      :disabled="loading"
      @change="onSelect"
    >
      <option v-if="loading" value="" disabled>Cargando cuentas...</option>
      <option v-else-if="error" value="" disabled>Error al cargar cuentas</option>
      <template v-else>
        <option value="" disabled>Seleccionar cuenta</option>
        <option
          v-for="account in accounts"
          :key="account.id"
          :value="account.id"
        >
          {{ account.name }}
        </option>
      </template>
    </select>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import cashAccountService from '@/services/cashAccountService'

defineProps({
    modelValue: {
        type: String,
        default: ''
    }
})

const emit = defineEmits(['update:modelValue'])

const accounts = ref([])
const loading = ref(false)
const error = ref(false)

async function fetchAccounts() {
    loading.value = true
    error.value = false
    try {
        const response = await cashAccountService.getAll()
        accounts.value = response.data
    } catch {
        error.value = true
    } finally {
        loading.value = false
    }
}

function onSelect(event) {
    emit('update:modelValue', event.target.value)
}

onMounted(fetchAccounts)
</script>

<style scoped>
.cash-account-selector select {
    width: 100%;
    padding: 0.5rem;
    border: 1px solid var(--border-color, #ddd);
    border-radius: 4px;
    font-size: 0.875rem;
}
</style>
