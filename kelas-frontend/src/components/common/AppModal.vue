<template>
  <Transition name="modal">
    <div v-if="show" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal" :style="{ width: width }">
        <div class="modal-header">
          <h2>{{ title }}</h2>
          <button class="modal-close" @click="$emit('close')" aria-label="Cerrar">✕</button>
        </div>
        <div class="modal-body" :style="{ padding: bodyPadding }">
          <slot></slot>
        </div>
        <div v-if="$slots.footer" class="modal-footer">
          <slot name="footer"></slot>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: ''
  },
  show: {
    type: Boolean,
    default: false
  },
  width: {
    type: String,
    default: '520px'
  },
  bodyPadding: {
    type: String,
    default: '20px 24px'
  }
})

defineEmits(['close'])
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  z-index: 1000;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 60px 20px;
}

.modal {
  background: var(--color-bg, #ffffff);
  border-radius: var(--radius-lg, 8px);
  box-shadow: var(--shadow-lg, 0 12px 32px rgba(0, 0, 0, 0.15));
  max-width: 100%;
  max-height: calc(100vh - 120px);
  display: flex;
  flex-direction: column;
  position: relative;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--color-border, #e5e5e7);
  flex-shrink: 0;
}

.modal-header h2 {
  font-size: 1.1rem;
  font-weight: 700;
  margin: 0;
  color: var(--color-text);
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.4rem;
  color: var(--color-text-muted, #9b9ba7);
  cursor: pointer;
  padding: 4px;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.1s;
}

.modal-close:hover {
  color: var(--color-text, #1a1a1a);
}

.modal-body {
  overflow-y: auto;
  flex: 1;
}

.modal-footer {
  padding: 14px 20px;
  border-top: 1px solid var(--color-border, #e5e5e7);
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  flex-shrink: 0;
}

/* Transition */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

/* Responsive */
@media (max-width: 640px) {
  .modal-overlay {
    padding: 0;
    align-items: flex-end;
  }
  
  .modal {
    width: 100% !important;
    max-height: 92vh;
    border-radius: 16px 16px 0 0;
  }
}
</style>
