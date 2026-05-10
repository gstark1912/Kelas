<template>
  <Transition name="modal">
    <div v-if="show" class="modal-overlay" @click.self="$emit('cancel')">
      <div class="dialog">
        <div class="dialog-header">
          <h2>{{ title }}</h2>
          <button class="dialog-close" @click="$emit('cancel')" aria-label="Cerrar">✕</button>
        </div>
        <div class="dialog-body">
          <p>{{ message }}</p>
        </div>
        <div class="dialog-footer">
          <button class="btn" @click="$emit('cancel')">Cancelar</button>
          <button
            class="btn"
            :class="confirmBtnClass"
            @click="$emit('confirm')"
          >
            {{ confirmText }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  show: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Confirmar'
  },
  message: {
    type: String,
    default: '¿Estás seguro?'
  },
  confirmText: {
    type: String,
    default: 'Confirmar'
  },
  variant: {
    type: String,
    default: 'danger' // 'danger' | 'warning'
  }
})

defineEmits(['confirm', 'cancel'])

const confirmBtnClass = computed(() => {
  if (props.variant === 'warning') return 'btn-warning'
  return 'btn-danger'
})
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.3);
  z-index: 300;
  display: flex;
  align-items: center;
  justify-content: center;
}

.dialog {
  background: var(--color-bg, #ffffff);
  border-radius: var(--radius-lg, 8px);
  box-shadow: var(--shadow-lg, 0 8px 24px rgba(0, 0, 0, 0.12));
  width: 420px;
  max-width: 90vw;
}

.dialog-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 18px 24px;
  border-bottom: 1px solid var(--color-border, #e5e5e7);
}

.dialog-header h2 {
  font-size: 1rem;
  font-weight: 600;
  margin: 0;
}

.dialog-close {
  background: none;
  border: none;
  font-size: 1.1rem;
  color: var(--color-text-muted, #9b9ba7);
  cursor: pointer;
  padding: 4px;
  line-height: 1;
}

.dialog-close:hover {
  color: var(--color-text, #1a1a1a);
}

.dialog-body {
  padding: 20px 24px;
}

.dialog-body p {
  margin: 0;
  font-size: 0.9rem;
  color: var(--color-text-secondary, #6b6b76);
  line-height: 1.5;
}

.dialog-footer {
  padding: 14px 24px;
  border-top: 1px solid var(--color-border, #e5e5e7);
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.btn {
  display: inline-flex;
  align-items: center;
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

.btn:hover {
  background: var(--color-bg-secondary, #f7f7f8);
}

.btn-danger {
  background: var(--color-negative, #c53030);
  color: #fff;
  border-color: var(--color-negative, #c53030);
}

.btn-danger:hover {
  background: #a82828;
  border-color: #a82828;
}

.btn-warning {
  background: var(--color-warning, #b45309);
  color: #fff;
  border-color: var(--color-warning, #b45309);
}

.btn-warning:hover {
  background: #9a4608;
  border-color: #9a4608;
}

/* Transition */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.15s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
