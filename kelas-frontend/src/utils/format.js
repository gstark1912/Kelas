export function formatCurrency(amount) {
  if (amount === undefined || amount === null) return '-';
  return new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    minimumFractionDigits: 0
  }).format(amount);
}

export function formatDate(dateStr) {
  if (!dateStr) return '-';
  const date = new Date(dateStr);
  return new Intl.DateTimeFormat('es-AR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    timeZone: 'UTC'
  }).format(date);
}

export function formatPercent(value) {
  if (value === undefined || value === null) return '-';
  return `${value.toFixed(1)}%`;
}

export function toDateInputValue(date = new Date()) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function toUtcDateStart(dateInputValue) {
  return `${dateInputValue}T00:00:00.000Z`;
}

export function toUtcDateEnd(dateInputValue) {
  return `${dateInputValue}T23:59:59.999Z`;
}
