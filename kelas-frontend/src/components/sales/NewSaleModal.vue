<template>
  <AppModal
    :show="show"
    title="Nueva Venta"
    @close="handleClose"
    width="640px"
  >
    <div class="modal-body" v-if="form">
      <div class="form-row">
        <FormField label="Fecha" :required="true">
          <input v-model="form.date" type="date" :disabled="submitting" />
        </FormField>
        <FormField label="Canal" :required="true">
          <select v-model="form.channel" :disabled="submitting">
            <option value="">Seleccionar...</option>
            <option v-for="c in channels" :key="c" :value="c">{{ c }}</option>
          </select>
        </FormField>
        <FormField label="Medio de Pago" :required="true">
          <select v-model="form.paymentMethod" :disabled="submitting">
            <option value="">Seleccionar...</option>
            <option v-for="m in paymentMethods" :key="m" :value="m">{{ m }}</option>
          </select>
        </FormField>
      </div>

      <!-- Items -->
      <div class="section-label">Ítems de la venta:</div>
      <div class="items-table-container">
        <table class="items-table">
          <thead>
            <tr>
              <th>Producto</th>
              <th class="num" style="width: 80px">Cant.</th>
              <th class="num" style="width: 110px">Precio Unit.</th>
              <th class="num" style="width: 100px">Subtotal</th>
              <th style="width: 40px"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(item, index) in form.items" :key="index">
              <td>
                <select 
                  v-model="item.productId" 
                  @change="onProductChange(index)"
                  :disabled="submitting"
                >
                  <option value="" disabled>Seleccionar producto...</option>
                  <option v-for="p in products" :key="p.id" :value="p.id">
                    {{ p.name }} (Stock: {{ p.currentStock }})
                  </option>
                </select>
              </td>
              <td class="num">
                <input
                  v-model.number="item.quantity"
                  type="number"
                  min="1"
                  step="any"
                  :disabled="submitting"
                  class="input-qty"
                  :class="{ 'stock-warning': isStockInsufficient(item) }"
                />
              </td>
              <td class="num">
                <input
                  :value="item.unitPrice"
                  type="number"
                  readonly
                  class="input-readonly"
                />
              </td>
              <td class="num fw-600">{{ formatCurrency(item.quantity * item.unitPrice) }}</td>
              <td>
                <button 
                  class="btn-icon-danger" 
                  @click="removeItem(index)"
                  :disabled="submitting || form.items.length <= 1"
                >✕</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <button class="btn btn-sm mb-16" @click="addItem" :disabled="submitting">+ Agregar ítem</button>

      <div class="form-row">
        <FormField label="Descuento (%)">
          <input v-model.number="form.discountPercent" type="number" min="0" max="100" :disabled="submitting" />
        </FormField>
        <FormField label="Costo Impositivo (%)">
          <input v-model.number="form.taxCostPercent" type="number" min="0" max="100" :disabled="submitting" />
        </FormField>
        <FormField label="Costo Canal (%)">
          <input v-model.number="form.channelCostPercent" type="number" min="0" max="100" :disabled="submitting" />
        </FormField>
      </div>

      <div class="form-row">
        <FormField label="Envío ($)">
          <input v-model.number="form.shippingCost" type="number" min="0" placeholder="Monto fijo" :disabled="submitting" />
        </FormField>
        <FormField label="Detalle Envío">
          <input v-model="form.shippingDetail" type="text" placeholder="Ej: Correo Argentino" :disabled="submitting" />
        </FormField>
      </div>

      <FormField label="Notas">
        <textarea v-model="form.notes" placeholder="Notas sobre la venta..." :disabled="submitting"></textarea>
      </FormField>

      <!-- Financial Summary (POC Style) -->
      <div class="summary-box" v-if="calculations">
        <div class="flex justify-between">
          <span>Subtotal:</span> <strong>{{ formatCurrency(calculations.subtotal) }}</strong>
        </div>
        <div class="flex justify-between">
          <span>Descuento ({{ form.discountPercent }}%):</span> <span>-{{ formatCurrency(calculations.discountAmount) }}</span>
        </div>
        <div class="flex justify-between">
          <span>Envío:</span> <span>{{ formatCurrency(form.shippingCost) }}</span>
        </div>
        <div class="flex justify-between border-top mt-8 pt-8">
          <span class="fw-600">Ingreso Bruto:</span> <strong style="font-size: 1.05rem">{{ formatCurrency(calculations.grossIncome) }}</strong>
        </div>
        <div class="flex justify-between text-muted mt-4">
          <span>COGS estimado:</span> <span>{{ formatCurrency(calculations.totalCOGS) }}</span>
        </div>
        <div class="flex justify-between text-muted mt-4">
          <span>Costo impositivo ({{ form.taxCostPercent }}%):</span> <span>{{ formatCurrency(calculations.taxCostAmount) }}</span>
        </div>
        <div class="flex justify-between text-muted mt-4">
          <span>Costo canal ({{ form.channelCostPercent }}%):</span> <span>{{ formatCurrency(calculations.channelCostAmount) }}</span>
        </div>
        <div class="flex justify-between text-positive mt-4">
          <span class="fw-600">Ganancia neta estimada:</span> <strong>+{{ formatCurrency(calculations.netProfit) }}</strong>
        </div>
      </div>

      <p v-if="errorMessage" class="error-message">{{ errorMessage }}</p>
    </div>

    <template #footer>
      <button class="btn" @click="handleClose" :disabled="submitting">Cancelar</button>
      <button class="btn btn-primary" @click="handleSubmit" :disabled="submitting || !isFormValid">
        <span v-if="submitting">Registrando...</span>
        <span v-else>Registrar Venta</span>
      </button>
    </template>
  </AppModal>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import AppModal from '@/components/common/AppModal.vue'
import FormField from '@/components/common/FormField.vue'
import productService from '@/services/productService'
import cashAccountService from '@/services/cashAccountService'
import saleService from '@/services/saleService'
import { toDateInputValue, toUtcDateStart } from '@/utils/format'

const props = defineProps({
  show: Boolean
})

const emit = defineEmits(['close', 'sale-created'])

// Constants
const channels = ['Feria', 'Instagram', 'Tienda', 'Otro']
const paymentMethods = ['Efectivo', 'Transferencia', 'Mercado Pago']

// State
const products = ref([])
const cashAccounts = ref([])
const submitting = ref(false)
const errorMessage = ref('')

const initialForm = {
  date: toDateInputValue(),
  channel: '',
  paymentMethod: '',
  items: [
    { productId: '', quantity: 1, unitPrice: 0, currentStock: 0, unitCost: 0 }
  ],
  discountPercent: 0,
  taxCostPercent: 0,
  channelCostPercent: 0,
  shippingCost: 0,
  shippingDetail: '',
  notes: ''
}

const form = ref({ ...initialForm })

// Methods
function formatCurrency(amount) {
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(amount || 0)
}

async function fetchData() {
  try {
    const [prodRes, accRes] = await Promise.all([
      productService.getAll(),
      cashAccountService.getAll()
    ])
    products.value = prodRes.data || []
    cashAccounts.value = accRes.data || []
  } catch (err) {
    errorMessage.value = 'Error al cargar datos.'
  }
}

function addItem() {
  form.value.items.push({ productId: '', quantity: 1, unitPrice: 0, currentStock: 0, unitCost: 0 })
}

function removeItem(index) {
  form.value.items.splice(index, 1)
}

function onProductChange(index) {
  const item = form.value.items[index]
  const product = products.value.find(p => p.id === item.productId)
  if (product) {
    item.unitPrice = product.listPrice || 0
    item.currentStock = product.currentStock || 0
    item.unitCost = product.estimatedCost || 0
  }
}

function isStockInsufficient(item) {
  if (!item || !item.productId) return false
  return item.quantity > item.currentStock
}

function handleClose() {
  if (submitting.value) return
  emit('close')
}

// Calculations
const calculations = computed(() => {
  if (!form.value || !form.value.items) return null

  const subtotal = form.value.items.reduce((acc, i) => acc + (i.quantity * i.unitPrice), 0)
  const totalCOGS = form.value.items.reduce((acc, i) => acc + (i.quantity * i.unitCost), 0)
  
  const discountAmount = subtotal * (form.value.discountPercent / 100)
  const taxCostAmount = subtotal * (form.value.taxCostPercent / 100)
  const channelCostAmount = subtotal * (form.value.channelCostPercent / 100)
  
  const grossIncome = (subtotal - discountAmount) + form.value.shippingCost
  const grossProfit = grossIncome - totalCOGS - form.value.shippingCost
  const netProfit = grossProfit - taxCostAmount - channelCostAmount
  
  return {
    subtotal,
    totalCOGS,
    discountAmount,
    taxCostAmount,
    channelCostAmount,
    grossIncome,
    grossProfit,
    netProfit
  }
})

const isFormValid = computed(() => {
  if (!form.value) return false
  return (
    form.value.channel &&
    form.value.paymentMethod &&
    form.value.items &&
    form.value.items.length > 0 &&
    form.value.items.every(i => i.productId && i.quantity > 0)
  )
})

async function handleSubmit() {
  if (!isFormValid.value || submitting.value) return

  submitting.value = true
  errorMessage.value = ''

  let cashAccountId = ''
  const matchedAccount = cashAccounts.value.find(a => 
    a.name.toLowerCase().includes(form.value.paymentMethod.toLowerCase()) ||
    (form.value.paymentMethod === 'Mercado Pago' && a.name.toLowerCase().includes('mercado'))
  )
  
  if (matchedAccount) {
    cashAccountId = matchedAccount.id
  } else {
    cashAccountId = cashAccounts.value[0]?.id
  }

  if (!cashAccountId) {
    errorMessage.value = 'No se encontró una cuenta de caja válida para este medio de pago.'
    submitting.value = false
    return
  }

  const payload = {
    date: toUtcDateStart(form.value.date),
    channel: form.value.channel,
    paymentMethod: form.value.paymentMethod,
    cashAccountId: cashAccountId,
    items: form.value.items.map(i => ({
      productId: i.productId,
      quantity: i.quantity
    })),
    shippingCost: form.value.shippingCost,
    shippingDetail: form.value.shippingDetail,
    discountPercent: form.value.discountPercent,
    taxCostPercent: form.value.taxCostPercent,
    channelCostPercent: form.value.channelCostPercent,
    notes: form.value.notes
  }

  try {
    await saleService.create(payload)
    emit('sale-created')
    emit('close')
  } catch (err) {
    errorMessage.value = err.response?.data?.error || 'Error al registrar la venta.'
  } finally {
    submitting.value = false
  }
}

// Lifecycle
onMounted(() => {
  fetchData()
})

watch(() => props.show, (newVal) => {
  if (newVal) {
    form.value = {
      date: toDateInputValue(),
      channel: '',
      paymentMethod: '',
      items: [{ productId: '', quantity: 1, unitPrice: 0, currentStock: 0, unitCost: 0 }],
      discountPercent: 0,
      taxCostPercent: 0,
      channelCostPercent: 0,
      shippingCost: 0,
      shippingDetail: '',
      notes: ''
    }
    fetchData()
  }
})
</script>

<style scoped>
.modal-body {
  padding: 16px 20px;
}

.section-label {
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--color-text-secondary);
  margin-bottom: 8px;
}

.items-table-container {
  border: 1px solid var(--color-border);
  border-radius: var(--radius);
  overflow: hidden;
  margin-bottom: 12px;
}

.items-table {
  width: 100%;
  font-size: 0.85rem;
  border-collapse: collapse;
}

.items-table th {
  text-align: left;
  background: var(--color-bg-secondary);
  padding: 8px 12px;
  border-bottom: 1px solid var(--color-border);
}

.items-table td {
  padding: 6px 12px;
  border-bottom: 1px solid var(--color-border);
}

.items-table select {
  width: 100%;
  padding: 5px 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  background: #fff;
  font-family: inherit;
}

.input-qty {
  width: 60px;
  padding: 5px 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  text-align: right;
  font-family: inherit;
}

.input-qty.stock-warning {
  border-color: var(--color-warning);
  background-color: var(--color-warning-bg);
}

.input-readonly {
  width: 90px;
  padding: 5px 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  text-align: right;
  background: var(--color-bg-secondary);
  color: var(--color-text-secondary);
  font-family: inherit;
}

.summary-box {
  background: var(--color-bg-secondary);
  padding: 16px;
  border-radius: var(--radius);
  font-size: 0.88rem;
  margin-top: 16px;
}

.error-message {
  color: var(--color-negative);
  font-size: 0.85rem;
  margin-top: 12px;
  padding: 8px 12px;
  background: var(--color-negative-bg);
  border-radius: 4px;
}

.form-row {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
}

.form-row > * {
  flex: 1;
}

textarea {
  width: 100%;
  min-height: 60px;
  padding: 10px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  resize: vertical;
  font-family: inherit;
  font-size: 0.88rem;
}

input[type="text"],
input[type="number"],
input[type="date"],
select {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-family: inherit;
  font-size: 0.88rem;
  background: #fff;
}

input:focus, select:focus, textarea:focus {
  outline: none;
  border-color: var(--color-primary);
}
</style>
