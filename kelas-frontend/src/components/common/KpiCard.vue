<template>
  <div class="kpi-card" :class="[`variant-${variant}`]">
    <div class="kpi-header">
      <h3 class="kpi-title">{{ title }}</h3>
    </div>
    <div class="kpi-body">
      <div class="kpi-value">{{ value }}</div>
      <div v-if="subtitle" class="kpi-subtitle">{{ subtitle }}</div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  },
  value: {
    type: [String, Number],
    required: true
  },
  subtitle: {
    type: String,
    default: ''
  },
  variant: {
    type: String,
    default: 'default',
    validator: (v) => ['default', 'success', 'danger', 'warning', 'primary'].includes(v)
  }
})
</script>

<style scoped>
.kpi-card {
  background: var(--color-bg, #ffffff);
  border: 1px solid var(--color-border, #e5e5e7);
  border-radius: var(--radius-lg, 8px);
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.kpi-title {
  font-size: 0.82rem;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-text-muted, #9b9ba7);
  margin: 0;
  letter-spacing: 0.5px;
}

.kpi-value {
  font-size: 1.8rem;
  font-weight: 700;
  color: var(--color-text, #1a1a1a);
  line-height: 1.2;
}

.kpi-subtitle {
  font-size: 0.85rem;
  color: var(--color-text-secondary, #6b6b76);
  margin-top: 4px;
}

/* Variants */
.variant-success .kpi-value { color: var(--color-positive, #059669); }
.variant-danger .kpi-value { color: var(--color-negative, #c53030); }
.variant-primary .kpi-value { color: var(--color-primary, #5b5bd6); }
.variant-warning .kpi-value { color: #d97706; }
</style>
