// script.js

// ------------------ Elementos principales ------------------
const loginSection = document.getElementById('login-section');
const dashboardSection = document.getElementById('dashboard-section');
const loginForm = document.getElementById('loginForm');
const devModeBtn = document.getElementById('devModeBtn');
const logoutBtn = document.getElementById('logoutBtn');
const sidebarItems = document.querySelectorAll('.sidebar-item');
const panels = document.querySelectorAll('.section');
const welcomeUser = document.getElementById('welcomeUser');

// Si quieres, cambia aquí el nombre que aparece en el header al entrar en dev mode:
const DEV_USERNAME = 'Desarrollador (Dev Mode)';

// ------------------ Navegación básica entre login y dashboard ------------------
if (loginForm) {
  loginForm.addEventListener('submit', (e) => {
    e.preventDefault();
    // Puedes usar los valores si quieres: email.value, password.value
    showDashboard();
  });
}

if (devModeBtn) {
  devModeBtn.addEventListener('click', () => {
    showDashboard(true); // true = modo dev
  });
}

if (logoutBtn) {
  logoutBtn.addEventListener('click', function() {
    // modal de confirmación simple
    const logoutModal = document.createElement('div');
    logoutModal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center';
    logoutModal.innerHTML = `
      <div class="glass-effect rounded-xl p-6 max-w-md w-full mx-4">
          <h3 class="text-lg font-semibold text-white mb-4">Cerrar sesión</h3>
          <p class="text-white text-opacity-80 mb-6">¿Estás seguro de que deseas cerrar sesión?</p>
          <div class="flex space-x-3">
              <button id="confirmLogout" class="bg-red-500 hover:bg-red-600 text-white px-4 py-2 rounded-lg transition-all duration-200">Sí, cerrar sesión</button>
              <button id="cancelLogout" class="bg-white bg-opacity-20 text-white px-4 py-2 rounded-lg hover:bg-opacity-30 transition-all duration-200">Cancelar</button>
          </div>
      </div>
    `;
    document.body.appendChild(logoutModal);
    document.getElementById('cancelLogout').addEventListener('click', () => {
      document.body.removeChild(logoutModal);
    });
    document.getElementById('confirmLogout').addEventListener('click', () => {
      document.body.removeChild(logoutModal);
      // vuelve al login
      dashboardSection.classList.add('hidden');
      loginSection.classList.remove('hidden');
    });
  });
}

function showDashboard(dev=false) {
  loginSection.classList.add('hidden');
  dashboardSection.classList.remove('hidden');
  if (dev && welcomeUser) welcomeUser.textContent = `Modo Dev — ${DEV_USERNAME}`;
}

// ------------------ Sidebar: cambiar secciones ------------------
sidebarItems.forEach(item => {
  item.addEventListener('click', function() {
    sidebarItems.forEach(si => si.classList.remove('active'));
    this.classList.add('active');
    // ocultar todas las secciones/panels
    panels.forEach(p => p.classList.add('hidden'));
    const section = this.getAttribute('data-section');
    const el = document.getElementById(section + '-section');
    if (el) el.classList.remove('hidden');
  });
});

// ------------------ Formularios: handler genérico (muestras de notificación) ------------------
document.querySelectorAll('form').forEach(form => {
  form.addEventListener('submit', function(e) {
    // No bloquear todos los formularios (algunos pueden ser reales), pero evitamos comportamiento por defecto para demo
    e.preventDefault();
    const msg = document.createElement('div');
    msg.className = 'fixed-message glass-effect p-3 text-white';
    msg.style.border = '1px solid rgba(255,255,255,0.1)';
    msg.textContent = 'Datos procesados (demo)';
    document.body.appendChild(msg);
    setTimeout(() => msg.remove(), 2500);
  });
});

// ------------------ Manejo de botones del dashboard (ver, fav, eliminar) ------------------
document.addEventListener('click', function(e) {
  // Ver artículo
  if (e.target && e.target.classList.contains('view-article')) {
    const modal = document.createElement('div');
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4';
    modal.innerHTML = `
      <div class="glass-effect rounded-xl p-6 max-w-2xl w-full max-h-96 overflow-y-auto">
        <h3 class="text-lg font-semibold text-white mb-4">Vista del Artículo</h3>
        <p class="text-white text-opacity-80 mb-4">Aquí se mostraría el contenido completo del artículo seleccionado...</p>
        <button class="bg-white bg-opacity-20 text-white px-4 py-2 rounded-lg hover:bg-opacity-30 transition-all duration-200">Cerrar</button>
      </div>
    `;
    document.body.appendChild(modal);
    modal.querySelector('button').addEventListener('click', () => modal.remove());
  }

  // Favorito
  if (e.target && e.target.classList.contains('favorite-btn')) {
    e.target.textContent = '★';
    e.target.style.backgroundColor = '#fbbf24';
  }

  // Eliminar artículo (animación)
  if (e.target && (e.target.classList.contains('delete-article') || e.target.textContent.includes('Eliminar'))) {
    const glass = e.target.closest('.glass-effect');
    if (glass) {
      glass.style.opacity = '0.5';
      setTimeout(() => { if (glass.parentElement) glass.remove(); }, 300);
    }
  }
});

// ------------------ Scraping (simulación) ------------------
let scrapingInterval;
let isScrapingActive = false;
let articlesCount = 0;

const startScrapingBtn = document.getElementById('startScrapingBtn');
const stopScrapingBtn = document.getElementById('stopScrapingBtn');
const scrapingStatus = document.getElementById('scrapingStatus');
const articlesCountDisplay = document.getElementById('articlesCount');
const currentUrlDisplay = document.getElementById('currentUrl');
const addUrlBtn = document.getElementById('addUrlBtn');
const scrapingUrlInput = document.getElementById('scrapingUrl');
const urlsList = document.getElementById('urlsList');

if (startScrapingBtn) {
  startScrapingBtn.addEventListener('click', function() {
    if (!isScrapingActive) {
      isScrapingActive = true;
      articlesCount = 0;
      startScrapingBtn.disabled = true;
      startScrapingBtn.classList.add('opacity-50');
      stopScrapingBtn.disabled = false;
      stopScrapingBtn.classList.remove('opacity-50');
      scrapingStatus.classList.remove('hidden');

      scrapingInterval = setInterval(() => {
        articlesCount += Math.floor(Math.random() * 3) + 1;
        if (articlesCountDisplay) articlesCountDisplay.textContent = articlesCount;
        const urls = ['https://cortesuprema.gob.pe', 'https://tc.gob.pe', 'https://gacetajuridica.com'];
        const randomUrl = urls[Math.floor(Math.random() * urls.length)];
        if (currentUrlDisplay) currentUrlDisplay.textContent = `Procesando: ${randomUrl}`;
      }, 2000);
    }
  });
}

if (stopScrapingBtn) {
  stopScrapingBtn.addEventListener('click', function() {
    if (isScrapingActive) {
      isScrapingActive = false;
      clearInterval(scrapingInterval);
      startScrapingBtn.disabled = false;
      startScrapingBtn.classList.remove('opacity-50');
      stopScrapingBtn.disabled = true;
      stopScrapingBtn.classList.add('opacity-50');
      scrapingStatus.classList.add('hidden');

      const successMsg = document.createElement('div');
      successMsg.className = 'fixed-message glass-effect p-3 text-white';
      successMsg.textContent = `Scraping detenido. Total: ${articlesCount} artículos extraídos`;
      document.body.appendChild(successMsg);
      setTimeout(() => successMsg.remove(), 4000);
    }
  });
}

if (addUrlBtn) {
  addUrlBtn.addEventListener('click', function() {
    const url = scrapingUrlInput.value.trim();
    if (url && url.startsWith('http')) {
      const urlItem = document.createElement('div');
      urlItem.className = 'flex items-center justify-between p-3 bg-white bg-opacity-10 rounded-lg';
      urlItem.innerHTML = `
        <div class="flex items-center space-x-3">
          <div class="w-2 h-2 bg-green-400 rounded-full"></div>
          <span class="text-white text-sm">${url}</span>
        </div>
        <div class="flex space-x-2">
          <span class="text-green-400 text-xs bg-green-500 bg-opacity-20 px-2 py-1 rounded">Activa</span>
          <button class="text-red-400 hover:text-red-300 text-sm url-delete-btn">Eliminar</button>
        </div>
      `;
      urlsList.appendChild(urlItem);
      scrapingUrlInput.value = '';
      const successMsg = document.createElement('div');
      successMsg.className = 'fixed-message glass-effect p-3 text-white';
      successMsg.textContent = 'URL agregada exitosamente';
      document.body.appendChild(successMsg);
      setTimeout(() => successMsg.remove(), 3000);
    } else {
      const errorMsg = document.createElement('div');
      errorMsg.className = 'fixed-message glass-effect p-3 text-white';
      errorMsg.textContent = 'Por favor ingresa una URL válida';
      document.body.appendChild(errorMsg);
      setTimeout(() => errorMsg.remove(), 3000);
    }
  });
}

if (urlsList) {
  urlsList.addEventListener('click', function(e) {
    if (e.target.classList.contains('url-delete-btn')) {
      e.target.closest('.flex').remove();
    }
  });
}

// ------------------ Delete account modal (configuración) ------------------
const deleteAccountBtn = document.getElementById('deleteAccountBtn');
const deleteModal = document.getElementById('deleteModal');
if (deleteAccountBtn && deleteModal) {
  deleteAccountBtn.addEventListener('click', function() {
    deleteModal.classList.remove('hidden');
  });

  const cancelDelete = document.getElementById('cancelDelete');
  const confirmDelete = document.getElementById('confirmDelete');

  if (cancelDelete) cancelDelete.addEventListener('click', () => deleteModal.classList.add('hidden'));
  if (confirmDelete) confirmDelete.addEventListener('click', () => {
    deleteModal.classList.add('hidden');
    document.body.innerHTML = `
      <div class="gradient-bg min-h-screen flex items-center justify-center">
          <div class="glass-effect rounded-xl p-8 text-center">
              <h2 class="text-2xl font-bold text-white mb-4">Cuenta eliminada</h2>
              <p class="text-white text-opacity-80">Tu cuenta ha sido eliminada exitosamente</p>
          </div>
      </div>
    `;
  });
}

// ------------------ Pequeños extras: cerrar modales al Escape ------------------
document.addEventListener('keydown', function(e) {
  if (e.key === 'Escape') {
    document.querySelectorAll('.fixed.inset-0').forEach(m => m.remove());
  }
});
