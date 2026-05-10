<template>
  <div class="dashboard-view">
    <div class="page-header">
      <h1>Dashboard</h1>
    </div>

    <div class="empty-state">
      <div class="empty-icon">📊</div>
      <p>El dashboard estará disponible próximamente.</p>
      <p class="text-muted">Mientras tanto, podés gestionar tus materias primas desde el menú lateral.</p>
    </div>
  </div>
</template>

<script setup>
</script>

<style scoped>
.dashboard-view {
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

.empty-state {
  text-align: center;
  padding: 80px 20px;
  color: var(--color-text-secondary, #6b6b76);
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 16px;
}

.empty-state p {
  margin: 0 0 8px;
  font-size: 0.95rem;
}

.text-muted {
  color: var(--color-text-muted, #9b9ba7);
  font-size: 0.85rem;
}
</style>
