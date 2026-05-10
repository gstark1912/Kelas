<template>
  <div class="table-container">
    <div class="table-scroll">
      <table>
        <thead>
          <tr>
            <th
              v-for="col in columns"
              :key="col.key"
              :class="col.class || ''"
            >
              {{ col.label }}
            </th>
            <th v-if="$slots.actions"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="loading">
            <td :colspan="totalColumns" class="table-status">
              <span class="table-loading">Cargando...</span>
            </td>
          </tr>
          <tr v-else-if="!data || data.length === 0">
            <td :colspan="totalColumns" class="table-status">
              <div class="empty-state">
                <div class="empty-icon">📭</div>
                No hay datos
              </div>
            </td>
          </tr>
          <template v-else>
            <tr v-for="(row, index) in data" :key="index">
              <td v-for="col in columns" :key="col.key" :class="col.class || ''">
                <slot :name="`cell-${col.key}`" :row="row" :value="row[col.key]">
                  {{ row[col.key] }}
                </slot>
              </td>
              <td v-if="$slots.actions">
                <slot name="actions" :row="row"></slot>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  columns: {
    type: Array,
    required: true
  },
  data: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const slots = defineSlots()

const totalColumns = computed(() => {
  return props.columns.length + (slots.actions ? 1 : 0)
})
</script>

<style scoped>
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

:deep(.num) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
</style>
