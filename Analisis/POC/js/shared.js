// ===== SHARED NAVIGATION & UTILITIES =====

function buildSidebar(activePage) {
  const pages = [
    { id: 'dashboard', label: 'Dashboard', icon: '📊', href: 'dashboard.html' },
    { id: 'productos', label: 'Productos', icon: '🏷️', href: 'productos.html' },
    { id: 'materias-primas', label: 'Materias Primas', icon: '🧱', href: 'materias-primas.html' },
    { id: 'produccion', label: 'Producción', icon: '⚙️', href: 'produccion.html' },
    { id: 'ventas', label: 'Ventas', icon: '💰', href: 'ventas.html' },
    { id: 'gastos', label: 'Gastos', icon: '📋', href: 'gastos.html' },
    { id: 'caja', label: 'Caja', icon: '🏦', href: 'caja.html' },
  ];

  return `
    <div class="sidebar-logo"><img src="kelas.jpg" alt="Kelas" class="sidebar-logo-img">Kelas<span>.</span></div>
    <nav class="sidebar-nav">
      <div class="sidebar-section-label">Principal</div>
      ${pages.slice(0, 1).map(p => `
        <a href="${p.href}" class="${p.id === activePage ? 'active' : ''}">
          <span class="icon">${p.icon}</span> ${p.label}
        </a>
      `).join('')}
      <div class="sidebar-section-label">Operaciones</div>
      ${pages.slice(1).map(p => `
        <a href="${p.href}" class="${p.id === activePage ? 'active' : ''}">
          <span class="icon">${p.icon}</span> ${p.label}
        </a>
      `).join('')}
    </nav>
  `;
}

function buildTopbar(title, context) {
  return `
    <div class="topbar-left">
      <span class="topbar-title">${title}</span>
      <span class="topbar-context">${context || ''}</span>
    </div>
    <div class="topbar-right">
      <span class="topbar-context">Abril 2026</span>
    </div>
  `;
}

function initPage(pageId, title, context) {
  document.querySelector('.sidebar').innerHTML = buildSidebar(pageId);
  document.querySelector('.topbar').innerHTML = buildTopbar(title, context);
}

function showToast(message) {
  let toast = document.getElementById('toast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'toast';
    toast.className = 'toast';
    document.body.appendChild(toast);
  }
  toast.textContent = message;
  toast.classList.add('show');
  setTimeout(() => toast.classList.remove('show'), 2500);
}

function openModal(id) {
  document.getElementById(id).classList.add('active');
}

function closeModal(id) {
  document.getElementById(id).classList.remove('active');
}

function formatMoney(n) {
  return '$' + Number(n).toLocaleString('es-AR');
}

// Close modals on overlay click
document.addEventListener('click', function (e) {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('active');
  }
});
