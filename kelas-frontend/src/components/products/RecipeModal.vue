<template>
  <AppModal
    :show="show"
    :title="`Receta — ${product?.name ?? ''}`"
    @close="handleClose"
  >
    <!-- View mode -->
    <div v-if="!editMode">
      <div v-if="!product?.recipe?.length" class="empty-recipe">
        <p>Este producto no tiene receta definida.</p>
      </div>
      <table v-else class="recipe-table">
        <thead>
          <tr>
            <th>Materia Prima</th>
            <th class="num">Cantidad</th>
            <th>Unidad</th>
            <th class="num">Costo Unit.</th>
            <th class="num">Subtotal</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in product.recipe" :key="item.rawMaterialId">
            <td>{{ item.rawMaterialName }}</td>
            <td class="num">{{ formatNumber(item.quantity) }}</td>
            <td>{{ item.unit }}</td>
            <td class="num">{{ formatCurrency(item.pricePerUnit) }}</td>
            <td class="num">{{ formatCurrency(item.subtotal) }}</td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colspan="4" class="total-label">Costo Estimado Total</td>
            <td class="num total-value">{{ formatCurrency(totalCost) }}</td>
          </tr>
        </tfoot>
      </table>
    </div>

    <!-- Edit mode -->
    <div v-else>
      <!-- Current ingredients list -->
      <div v-if="editIngredients.length" class="edit-ingredients">
        <div
          v-for="(item, index) in editIngredients"
          :key="item.rawMaterialId"
          class="ingredient-row"
        >
          <span class="ingredient-name">{{ item.rawMaterialName }}</span>
          <span class="ingredient-qty">{{ formatNumber(item.quantity) }} {{ item.unit }}</span>
          <button class="btn btn-sm btn-remove" @click="removeIngredient(index)" :disabled="submitting">
            ✕
          </button>
        </div>
      </div>
      <p v-else class="empty-recipe">No hay ingredientes. Agregá al menos uno.</p>

      <!-- Add ingredient form -->
      <div class="add-ingredient-form">
        <div class="add-ingredient-row">
          <select v-model="newIngredient.rawMaterialId" :disabled="submitting || !availableRawMaterials.length">
            <option value="">Seleccionar materia prima...</option>
            <option
              v-for="rm in availableRawMaterials"
              :key="rm.id"
              :value="rm.id"
            >
              {{ rm.name }} ({{ rm.unit }})
            </option>
          </select>
          <input
            v-model.number="newIngredient.quantity"
            type="number"
            min="0.001"
            step="any"
            placeholder="Cantidad"
            :disabled="submitting"
          />
          <button
            class="btn btn-sm btn-primary"
            @click="addIngredient"
            :disabled="submitting || !canAddIngredient"
          >
            Agregar
          </button>
        </div>
      </div>

      <p v-if="editError" class="form-general-error">{{ editError }}</p>
    </div>

    <template #footer>
      <template v-if="!editMode">
        <button class="btn" @click="handleClose">Cerrar</button>
        <button class="btn btn-primary" @click="startEdit" :disabled="loadingRawMaterials">
          Editar Receta
        </button>
      </template>
      <template v-else>
        <button class="btn" @click="cancelEdit" :disabled="submitting">Cancelar</button>
        <button
          class="btn btn-primary"
          @click="saveRecipe"
          :disabled="submitting || !editIngredients.length"
        >
          <span v-if="submitting">Guardando...</span>
          <span v-else>Confirmar Cambios</span>
        </button>
      </template>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import productService from '@/services/productService'
import rawMaterialService from '@/services/rawMaterialService'

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

const emit = defineEmits(['close', 'recipe-updated'])

const editMode = ref(false)
const submitting = ref(false)
const loadingRawMaterials = ref(false)
const editError = ref('')
const allRawMaterials = ref([])
const editIngredients = ref([]) // { rawMaterialId, rawMaterialName, unit, quantity }

const newIngredient = ref({
  rawMaterialId: '',
  quantity: null
})

// Reset state when modal opens/closes
watch(
  () => props.show,
  (visible) => {
    if (visible) {
      editMode.value = false
      editError.value = ''
      editIngredients.value = []
      newIngredient.value = { rawMaterialId: '', quantity: null }
    }
  }
)

const totalCost = computed(() => {
  if (!props.product?.recipe) return 0
  return props.product.recipe.reduce((sum, item) => sum + (item.subtotal ?? 0), 0)
})

// Raw materials not yet in the edit list
const availableRawMaterials = computed(() => {
  const usedIds = new Set(editIngredients.value.map(i => i.rawMaterialId))
  return allRawMaterials.value.filter(rm => !usedIds.has(rm.id))
})

const canAddIngredient = computed(() => {
  return (
    newIngredient.value.rawMaterialId !== '' &&
    newIngredient.value.quantity != null &&
    newIngredient.value.quantity > 0
  )
})

async function startEdit() {
  loadingRawMaterials.value = true
  try {
    const response = await rawMaterialService.getAll()
    allRawMaterials.value = response.data
  } catch {
    allRawMaterials.value = []
  } finally {
    loadingRawMaterials.value = false
  }

  // Seed edit list from current recipe
  editIngredients.value = (props.product?.recipe ?? []).map(item => ({
    rawMaterialId: item.rawMaterialId,
    rawMaterialName: item.rawMaterialName,
    unit: item.unit,
    quantity: item.quantity
  }))

  newIngredient.value = { rawMaterialId: '', quantity: null }
  editError.value = ''
  editMode.value = true
}

function cancelEdit() {
  editMode.value = false
  editError.value = ''
}

function addIngredient() {
  if (!canAddIngredient.value) return

  const rm = allRawMaterials.value.find(r => r.id === newIngredient.value.rawMaterialId)
  if (!rm) return

  editIngredients.value.push({
    rawMaterialId: rm.id,
    rawMaterialName: rm.name,
    unit: rm.unit,
    quantity: newIngredient.value.quantity
  })

  newIngredient.value = { rawMaterialId: '', quantity: null }
}

function removeIngredient(index) {
  editIngredients.value.splice(index, 1)
}

async function saveRecipe() {
  if (!editIngredients.value.length || submitting.value) return

  editError.value = ''
  submitting.value = true

  const ingredients = editIngredients.value.map(i => ({
    rawMaterialId: i.rawMaterialId,
    quantity: i.quantity
  }))

  try {
    await productService.updateRecipe(props.product.id, ingredients)
    emit('recipe-updated')
    emit('close')
  } catch (err) {
    if (err.response?.status === 400) {
      editError.value = err.response.data?.error || 'Error de validación'
    } else {
      editError.value = 'Ocurrió un error inesperado. Intentá nuevamente.'
    }
  } finally {
    submitting.value = false
  }
}

function handleClose() {
  if (submitting.value) return
  emit('close')
}

function formatNumber(value) {
  if (value == null) return '0'
  return Number(value).toLocaleString('es-AR')
}

function formatCurrency(value) {
  if (value == null) return '$0'
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(value)
}
</script>

<style scoped>
.recipe-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.88rem;
}

.recipe-table th {
  text-align: left;
  padding: 8px 10px;
  font-weight: 600;
  font-size: 0.78rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: var(--color-text-secondary, #6b6b76);
  background: var(--color-bg-secondary, #f7f7f8);
  border-bottom: 1px solid var(--color-border, #e5e5e7);
}

.recipe-table td {
  padding: 8px 10px;
  border-bottom: 1px solid var(--color-border-light, #efefef);
  vertical-align: middle;
}

.recipe-table tfoot td {
  border-top: 2px solid var(--color-border, #e5e5e7);
  border-bottom: none;
  font-weight: 600;
}

.total-label {
  text-align: right;
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.85rem;
}

.total-value {
  font-size: 0.95rem;
}

.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.empty-recipe {
  color: var(--color-text-muted, #9b9ba7);
  font-size: 0.88rem;
  text-align: center;
  padding: 16px 0;
}

/* Edit mode */
.edit-ingredients {
  margin-bottom: 16px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  overflow: hidden;
}

.ingredient-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 12px;
  border-bottom: 1px solid var(--color-border-light, #efefef);
  font-size: 0.88rem;
}

.ingredient-row:last-child {
  border-bottom: none;
}

.ingredient-name {
  flex: 1;
  font-weight: 500;
}

.ingredient-qty {
  color: var(--color-text-secondary, #6b6b76);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.add-ingredient-form {
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  padding: 12px;
  background: var(--color-bg-secondary, #f7f7f8);
}

.add-ingredient-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.add-ingredient-row select {
  flex: 1;
  padding: 6px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-family: var(--font, inherit);
  background: var(--color-bg, #ffffff);
}

.add-ingredient-row input {
  width: 100px;
  padding: 6px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-family: var(--font, inherit);
}

.add-ingredient-row select:focus,
.add-ingredient-row input:focus {
  outline: none;
  border-color: var(--color-primary, #5b5bd6);
}

.form-general-error {
  margin: 12px 0 0;
  padding: 10px 14px;
  font-size: 0.82rem;
  color: var(--color-negative, #c53030);
  background: var(--color-negative-bg, #fde8e8);
  border-radius: var(--radius, 6px);
}

/* Buttons */
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

.btn-sm {
  padding: 4px 10px;
  font-size: 0.8rem;
}

.btn-remove {
  color: var(--color-negative, #c53030);
  border-color: transparent;
  background: transparent;
}

.btn-remove:hover:not(:disabled) {
  background: var(--color-negative-bg, #fde8e8);
}
</style>
