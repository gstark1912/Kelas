<template>
  <AppModal
    :show="show"
    :title="product ? 'Editar Producto' : 'Nuevo Producto'"
    @close="handleClose"
  >
    <form @submit.prevent="handleSubmit">
      <FormField label="Nombre" :error="formErrors.name" :required="true">
        <input
          v-model="form.name"
          type="text"
          placeholder="Ej: Vela de Soja 200g"
          :disabled="submitting"
        />
      </FormField>

      <FormField label="Descripción" :error="formErrors.description">
        <textarea
          v-model="form.description"
          placeholder="Descripción opcional del producto"
          rows="2"
          :disabled="submitting"
        ></textarea>
      </FormField>

      <div class="form-row">
        <FormField label="Precio de Lista" :error="formErrors.listPrice" :required="true">
          <input
            v-model.number="form.listPrice"
            type="number"
            min="0.01"
            step="any"
            placeholder="Ej: 1500"
            :disabled="submitting"
          />
        </FormField>

        <FormField label="Horas Estimadas de Producción" :error="formErrors.estimatedHours">
          <input
            v-model.number="form.estimatedHours"
            type="number"
            min="0"
            step="any"
            placeholder="Ej: 2.5"
            :disabled="submitting"
          />
        </FormField>
      </div>

      <FormField label="Margen Mínimo Esperado (%)" :error="formErrors.minMargin">
        <input
          v-model.number="form.minMargin"
          type="number"
          min="0"
          max="100"
          step="any"
          placeholder="Ej: 30"
          :disabled="submitting"
        />
      </FormField>

      <p v-if="formError" class="form-general-error">{{ formError }}</p>
    </form>

    <template #footer>
      <button class="btn" @click="handleClose" :disabled="submitting">
        Cancelar
      </button>
      <button
        class="btn btn-primary"
        @click="handleSubmit"
        :disabled="submitting || !isFormValid"
      >
        <span v-if="submitting">Guardando...</span>
        <span v-else>{{ product ? 'Guardar' : 'Crear' }}</span>
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import productService from '@/services/productService'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  product: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close', 'saved'])

const submitting = ref(false)
const formError = ref('')
const formErrors = ref({
  name: '',
  description: '',
  listPrice: '',
  estimatedHours: '',
  minMargin: ''
})

const form = ref({
  name: '',
  description: '',
  listPrice: null,
  estimatedHours: null,
  minMargin: null
})

// Preload form when editing
watch(
  () => props.show,
  (visible) => {
    if (visible) {
      formError.value = ''
      formErrors.value = { name: '', description: '', listPrice: '', estimatedHours: '', minMargin: '' }
      if (props.product) {
        form.value = {
          name: props.product.name ?? '',
          description: props.product.description ?? '',
          listPrice: props.product.listPrice ?? null,
          estimatedHours: props.product.estimatedHours ?? null,
          minMargin: props.product.minMargin ?? null
        }
      } else {
        form.value = { name: '', description: '', listPrice: null, estimatedHours: null, minMargin: null }
      }
    }
  }
)

const isFormValid = computed(() => {
  return (
    form.value.name.trim() !== '' &&
    form.value.listPrice != null &&
    form.value.listPrice > 0
  )
})

function handleClose() {
  if (submitting.value) return
  emit('close')
}

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  formError.value = ''
  formErrors.value = { name: '', description: '', listPrice: '', estimatedHours: '', minMargin: '' }
  submitting.value = true

  const payload = {
    name: form.value.name.trim(),
    description: form.value.description?.trim() || null,
    listPrice: form.value.listPrice,
    estimatedHours: form.value.estimatedHours || null,
    minMargin: form.value.minMargin || null
  }

  try {
    if (props.product) {
      await productService.update(props.product.id, payload)
    } else {
      await productService.create(payload)
    }
    emit('saved')
    emit('close')
  } catch (err) {
    if (err.response?.status === 400) {
      formError.value = err.response.data?.error || 'Error de validación'
    } else {
      formError.value = 'Ocurrió un error inesperado. Intentá nuevamente.'
    }
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
</style>

