<template>
  <div class="products-view">
    <div class="page-header">
      <h1>Productos</h1>
      <button class="btn btn-primary" @click="openCreateModal">
        + Nuevo Producto
      </button>
    </div>

    <!-- Filters -->
    <div class="filters-bar">
      <input
        v-model="searchInput"
        type="text"
        placeholder="Buscar producto..."
        style="width: 240px"
        @input="onSearchInput"
      />
    </div>

    <!-- Table -->
    <div class="table-container">
      <div class="table-scroll">
        <table>
          <thead>
            <tr>
              <th
                v-for="col in columns"
                :key="col.key"
                :class="[col.class || '', col.sortable ? 'sortable' : '']"
                @click="col.sortable ? toggleSort(col.key) : null"
              >
                {{ col.label }}
                <span v-if="col.sortable" class="sort-icon">
                  {{ sortKey === col.key ? (sortDir === 'asc' ? '↑' : '↓') : '↕' }}
                </span>
              </th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
              <td :colspan="columns.length + 1" class="table-status">
                <span class="table-loading">Cargando...</span>
              </td>
            </tr>
            <tr v-else-if="!sortedProducts.length">
              <td :colspan="columns.length + 1" class="table-status">
                <div class="empty-state">
                  <div class="empty-icon">📭</div>
                  No hay productos
                </div>
              </td>
            </tr>
            <template v-else>
              <tr v-for="product in sortedProducts" :key="product.id">
                <td><span class="fw-600">{{ product.name }}</span></td>
                <td class="text-muted">{{ product.description || '—' }}</td>
                <td class="num">{{ formatCurrency(product.listPrice) }}</td>
                <td class="num">{{ formatCurrency(product.estimatedCost) }}</td>
                <td class="num" :class="marginCellClass(product.marginAlert)">
                  {{ formatPercent(product.margin) }}
                </td>
                <td class="num">{{ product.minMargin != null ? formatPercent(product.minMargin) : '—' }}</td>
                <td class="num">{{ formatNumber(product.currentStock) }}</td>
                <td class="num">{{ product.estimatedHours != null ? formatNumber(product.estimatedHours) : '—' }}</td>
                <td>
                  <div class="action-buttons">
                    <button class="btn btn-sm" @click="openRecipeModal(product)">Receta</button>
                    <button class="btn btn-sm btn-primary" @click="openEditModal(product)">Editar</button>
                    <button class="btn btn-sm btn-danger" @click="openHideConfirm(product)">Ocultar</button>
                  </div>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Product Form Modal -->
    <ProductFormModal
      :show="showFormModal"
      :product="selectedProduct"
      @close="closeFormModal"
      @saved="onProductSaved"
    />

    <!-- Recipe Modal -->
    <RecipeModal
      :show="showRecipeModal"
      :product="selectedProductDetail"
      @close="closeRecipeModal"
      @recipe-updated="onRecipeUpdated"
    />

    <!-- Confirm Hide Dialog -->
    <ConfirmDialog
      :show="showHideConfirm"
      title="Ocultar Producto"
      :message="`¿Ocultar el producto ${selectedProduct?.name ?? ''}? No aparecerá en los listados de producción ni venta.`"
      confirm-text="Ocultar"
      variant="danger"
      @confirm="confirmHide"
      @cancel="closeHideConfirm"
    />

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="notification" class="toast" :class="notificationVariant">
        {{ notification }}
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import productService from '@/services/productService'
import ProductFormModal from '@/components/products/ProductFormModal.vue'
import RecipeModal from '@/components/products/RecipeModal.vue'
import ConfirmDialog from '@/components/common/ConfirmDialog.vue'

// Table columns definition
const columns = [
  { key: 'name', label: 'Producto', sortable: true },
  { key: 'description', label: 'Descripción', sortable: false },
  { key: 'listPrice', label: 'Precio Lista', class: 'num', sortable: true },
  { key: 'estimatedCost', label: 'Costo Est.', class: 'num', sortable: true },
  { key: 'margin', label: 'Margen %', class: 'num', sortable: true },
  { key: 'minMargin', label: 'Margen Mín.', class: 'num', sortable: true },
  { key: 'currentStock', label: 'Stock', class: 'num', sortable: true },
  { key: 'estimatedHours', label: 'Hs. Prod.', class: 'num', sortable: true }
]

// State
const products = ref([])
const loading = ref(false)
const searchInput = ref('')
let searchTimeout = null

// Sorting
const sortKey = ref('')
const sortDir = ref('asc')

// Modals
const showFormModal = ref(false)
const selectedProduct = ref(null)

const showRecipeModal = ref(false)
const selectedProductDetail = ref(null)

const showHideConfirm = ref(false)

// Notification
const notification = ref('')
const notificationVariant = ref('')
let notificationTimeout = null

// Computed: sorted products
const sortedProducts = computed(() => {
  if (!sortKey.value) return products.value

  return [...products.value].sort((a, b) => {
    const aVal = a[sortKey.value]
    const bVal = b[sortKey.value]

    if (aVal == null && bVal == null) return 0
    if (aVal == null) return 1
    if (bVal == null) return -1

    if (typeof aVal === 'string') {
      const cmp = aVal.localeCompare(bVal, 'es-AR', { sensitivity: 'base' })
      return sortDir.value === 'asc' ? cmp : -cmp
    }

    return sortDir.value === 'asc' ? aVal - bVal : bVal - aVal
  })
})

// Methods
function toggleSort(key) {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortKey.value = key
    sortDir.value = 'asc'
  }
}

function marginCellClass(alert) {
  if (alert === 'danger') return 'margin-danger'
  if (alert === 'warning') return 'margin-warning'
  return ''
}

function formatCurrency(value) {
  if (value == null) return '$0'
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(value)
}

function formatPercent(value) {
  if (value == null) return '0.0%'
  return `${Number(value).toFixed(1)}%`
}

function formatNumber(value) {
  if (value == null) return '0'
  return Number(value).toLocaleString('es-AR')
}

function onSearchInput() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    fetchProducts()
  }, 300)
}

async function fetchProducts() {
  loading.value = true
  try {
    const filters = {}
    if (searchInput.value.trim()) filters.search = searchInput.value.trim()
    const response = await productService.getAll(filters)
    products.value = response.data
  } catch {
    products.value = []
  } finally {
    loading.value = false
  }
}

// Form modal
function openCreateModal() {
  selectedProduct.value = null
  showFormModal.value = true
}

function openEditModal(product) {
  selectedProduct.value = product
  showFormModal.value = true
}

function closeFormModal() {
  showFormModal.value = false
  selectedProduct.value = null
}

async function onProductSaved() {
  showNotification('✓ Producto guardado exitosamente', '')
  await fetchProducts()
}

// Recipe modal
async function openRecipeModal(product) {
  // Fetch full detail to get expanded recipe
  try {
    const response = await productService.getById(product.id)
    selectedProductDetail.value = response.data
  } catch {
    selectedProductDetail.value = product
  }
  showRecipeModal.value = true
}

function closeRecipeModal() {
  showRecipeModal.value = false
  selectedProductDetail.value = null
}

async function onRecipeUpdated() {
  showNotification('✓ Receta actualizada exitosamente', '')
  await fetchProducts()
}

// Hide confirm
function openHideConfirm(product) {
  selectedProduct.value = product
  showHideConfirm.value = true
}

function closeHideConfirm() {
  showHideConfirm.value = false
  selectedProduct.value = null
}

async function confirmHide() {
  if (!selectedProduct.value) return
  const productId = selectedProduct.value.id
  showHideConfirm.value = false

  try {
    const response = await productService.updateVisibility(productId, false)
    if (response.data?.warning) {
      showNotification(`⚠ ${response.data.warning}`, 'toast-warning')
    } else {
      showNotification('✓ Producto ocultado', '')
    }
    // Remove from table
    products.value = products.value.filter(p => p.id !== productId)
  } catch (err) {
    showNotification('Error al ocultar el producto', 'toast-error')
  } finally {
    selectedProduct.value = null
  }
}

function showNotification(message, variant = '') {
  notification.value = message
  notificationVariant.value = variant
  if (notificationTimeout) clearTimeout(notificationTimeout)
  notificationTimeout = setTimeout(() => {
    notification.value = ''
    notificationVariant.value = ''
  }, 3000)
}

// Lifecycle
onMounted(() => {
  fetchProducts()
})
</script>

<style scoped>
.products-view {
  font-family: var(--font, -apple-system, BlinkMacSystemFont, 'Inter', 'Segoe UI', sans-serif);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.page-header h1 {
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: -0.3px;
  margin: 0;
}

/* Filters */
.filters-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.filters-bar input {
  padding: 6px 10px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-family: var(--font, inherit);
  color: var(--color-text, #1a1a1a);
  background: var(--color-bg, #ffffff);
}

.filters-bar input:focus {
  outline: none;
  border-color: var(--color-primary, #5b5bd6);
}

/* Table */
.table-container {
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius-lg, 8px);
  overflow: hidden;
  background: var(--color-bg, #ffffff);
}

.table-scroll {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.88rem;
}

thead th {
  text-align: left;
  padding: 10px 16px;
  font-weight: 600;
  font-size: 0.78rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  color: var(--color-text-secondary, #6b6b76);
  background: var(--color-bg-secondary, #f7f7f8);
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  white-space: nowrap;
}

thead th.sortable {
  cursor: pointer;
  user-select: none;
}

thead th.sortable:hover {
  background: #efefef;
}

.sort-icon {
  margin-left: 4px;
  font-size: 0.7rem;
  opacity: 0.6;
}

tbody td {
  padding: 10px 16px;
  border-bottom: 1px solid var(--color-border-light, #efefef);
  vertical-align: middle;
}

tbody tr:last-child td {
  border-bottom: none;
}

tbody tr:hover {
  background: #fafafb;
}

.table-status {
  text-align: center;
  padding: 40px 20px;
  color: var(--color-text-muted, #9b9ba7);
}

.table-loading {
  display: inline-block;
  animation: pulse 1.5s ease-in-out infinite;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.empty-icon {
  font-size: 2rem;
}

/* Margin alert colors */
.margin-danger {
  background: var(--color-negative-bg, #fde8e8);
  color: var(--color-negative, #c53030);
  font-weight: 600;
}

.margin-warning {
  background: var(--color-warning-bg, #fef3c7);
  color: var(--color-warning, #b45309);
  font-weight: 600;
}

/* Utilities */
.num {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.fw-600 {
  font-weight: 600;
}

.text-muted {
  color: var(--color-text-secondary, #6b6b76);
  font-size: 0.85rem;
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

.btn-danger {
  background: var(--color-negative, #c53030);
  color: #fff;
  border-color: var(--color-negative, #c53030);
}

.btn-danger:hover:not(:disabled) {
  background: #a82828;
  border-color: #a82828;
}

.btn-sm {
  padding: 4px 10px;
  font-size: 0.8rem;
}

.action-buttons {
  display: flex;
  gap: 6px;
  justify-content: flex-end;
}

/* Toast */
.toast {
  position: fixed;
  bottom: 24px;
  right: 24px;
  background: var(--color-text, #1a1a1a);
  color: #fff;
  padding: 12px 20px;
  border-radius: var(--radius, 6px);
  font-size: 0.85rem;
  font-weight: 500;
  box-shadow: var(--shadow-md, 0 4px 12px rgba(0, 0, 0, 0.08));
  z-index: 400;
}

.toast.toast-warning {
  background: var(--color-warning, #b45309);
}

.toast.toast-error {
  background: var(--color-negative, #c53030);
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
