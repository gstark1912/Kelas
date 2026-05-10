<template>
  <div class="form-group" :class="{ 'has-error': !!error }">
    <label v-if="label">
      {{ label }}
      <span v-if="required" class="required-mark">*</span>
    </label>
    <div class="form-input">
      <slot></slot>
    </div>
    <p v-if="error" class="form-error">{{ error }}</p>
  </div>
</template>

<script setup>
defineProps({
  label: {
    type: String,
    default: ''
  },
  error: {
    type: String,
    default: ''
  },
  required: {
    type: Boolean,
    default: false
  }
})
</script>

<style scoped>
.form-group {
  margin-bottom: 16px;
}

.form-group label {
  display: block;
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--color-text-secondary, #6b6b76);
  margin-bottom: 5px;
}

.required-mark {
  color: var(--color-negative, #c53030);
  margin-left: 2px;
}

.form-input :deep(input),
.form-input :deep(select),
.form-input :deep(textarea) {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius, 6px);
  font-size: 0.88rem;
  font-family: var(--font, -apple-system, BlinkMacSystemFont, 'Inter', 'Segoe UI', sans-serif);
  color: var(--color-text, #1a1a1a);
  background: var(--color-bg, #ffffff);
  transition: border-color 0.15s;
  box-sizing: border-box;
}

.form-input :deep(input):focus,
.form-input :deep(select):focus,
.form-input :deep(textarea):focus {
  outline: none;
  border-color: var(--color-primary, #5b5bd6);
  box-shadow: 0 0 0 3px rgba(91, 91, 214, 0.1);
}

.has-error .form-input :deep(input),
.has-error .form-input :deep(select),
.has-error .form-input :deep(textarea) {
  border-color: var(--color-negative, #c53030);
}

.has-error .form-input :deep(input):focus,
.has-error .form-input :deep(select):focus,
.has-error .form-input :deep(textarea):focus {
  box-shadow: 0 0 0 3px rgba(197, 48, 48, 0.1);
}

.form-error {
  margin: 5px 0 0;
  font-size: 0.78rem;
  color: var(--color-negative, #c53030);
}
</style>
