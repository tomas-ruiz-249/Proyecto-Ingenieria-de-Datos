// Sample data
let currentUser = {
    username: null,
    firstName: "Juan",
    lastName: "Pérez",
    email: "juan.perez@email.com"
};

let articles = [
    {
        id: 1,
        title: "Avances en Inteligencia Artificial transforman la industria",
        content: "Los últimos desarrollos en IA están revolucionando múltiples sectores. La implementación de algoritmos de aprendizaje automático ha permitido automatizar procesos complejos, mejorando la eficiencia y reduciendo costos operativos. Empresas de tecnología lideran esta transformación digital que promete cambiar la forma en que trabajamos y vivimos.",
        source: "TechNews",
        category: "tecnologia",
        date: "2024-01-15",
        url: "https://technews.com/ai-industry-transformation",
        keywords: ["inteligencia artificial", "tecnología", "industria"],
        isFavorite: false,
        isDiscarded: false,
        scrapingResultId: 1
    },
    {
        id: 2,
        title: "Nueva política económica genera debate",
        content: "El gobierno anuncia medidas económicas que dividen opiniones entre expertos y ciudadanos. Las nuevas regulaciones fiscales buscan estimular el crecimiento económico, pero algunos analistas expresan preocupación por su impacto a largo plazo en la inflación y el empleo.",
        source: "Noticias Económicas",
        category: "economia",
        date: "2024-01-14",
        url: "https://noticiaseconomicas.com/nueva-politica-economica",
        keywords: ["política", "economía", "gobierno"],
        isFavorite: true,
        isDiscarded: false,
        scrapingResultId: 2
    },
    {
        id: 3,
        title: "Descubrimiento médico promete nuevos tratamientos",
        content: "Investigadores desarrollan terapia innovadora para enfermedades neurodegenerativas. El nuevo tratamiento, basado en terapia génica, ha mostrado resultados prometedores en ensayos clínicos, ofreciendo esperanza a millones de pacientes que sufren de condiciones como Alzheimer y Parkinson.",
        source: "Salud Hoy",
        category: "salud",
        date: "2024-01-13",
        url: "https://saludhoy.com/descubrimiento-medico-tratamientos",
        keywords: ["medicina", "investigación", "tratamiento"],
        isFavorite: false,
        isDiscarded: false,
        scrapingResultId: 3
    }
];

let notifications = [
    {
        id: 1,
        title: "Scraping completado exitosamente",
        message: "Se encontraron 15 nuevos artículos de TechNews",
        type: "success",
        date: "2024-01-15 10:30",
        isRead: false,
        scrapingResultId: 1
    },
    {
        id: 2,
        title: "Error en scraping",
        message: "No se pudo acceder a la fuente Noticias Económicas",
        type: "error",
        date: "2024-01-14 16:45",
        isRead: false,
        scrapingResultId: 2
    },
    {
        id: 3,
        title: "Nuevos artículos disponibles",
        message: "8 artículos agregados desde Salud Hoy",
        type: "info",
        date: "2024-01-15 09:15",
        isRead: true,
        scrapingResultId: 3
    },
    {
        id: 4,
        title: "Fuente desactivada",
        message: "La fuente 'Deportes Hoy' ha sido desactivada por errores repetidos",
        type: "warning",
        date: "2024-01-13 14:20",
        isRead: false,
        scrapingResultId: null
    }
];

let sources = [
    { id: 1, name: "TechNews", url: "https://technews.com", active: true },
    { id: 2, name: "Noticias Económicas", url: "https://noticiaseconomicas.com", active: true },
    { id: 3, name: "Salud Hoy", url: "https://saludhoy.com", active: true }
];

let scrapingHistory = [
    { id: 1, date: "2024-01-15 10:30", status: "exitoso", articlesFound: 15, source: "TechNews" },
    { id: 2, date: "2024-01-15 09:15", status: "exitoso", articlesFound: 8, source: "Salud Hoy" },
    { id: 3, date: "2024-01-14 16:45", status: "fallido", articlesFound: 0, source: "Noticias Económicas" }
];

// DOM elements
const loginScreen = document.getElementById('loginScreen');
const registerScreen = document.getElementById('registerScreen');
const dashboard = document.getElementById('dashboard');
const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');

// Initialize app
function init() {
    setupEventListeners();
    updateStats();
    populateSourceFilters();
    renderArticles();
    renderSources();
    renderScrapingHistory();
    renderNotifications();
    updateNotificationCount();
}

function renderNotifications(filteredNotifications = null) {
    const notificationsToRender = filteredNotifications || notifications;
    const container = document.getElementById('notificationsList');
    
    if (notificationsToRender.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No hay notificaciones</div>';
        return;
    }

    container.innerHTML = notificationsToRender.map(notification => {
        const typeColors = {
            success: 'border-green-500 bg-green-50',
            error: 'border-red-500 bg-red-50',
            warning: 'border-yellow-500 bg-yellow-50',
            info: 'border-blue-500 bg-blue-50'
        };
        
        const typeIcons = {
            success: '✅',
            error: '❌',
            warning: '⚠️',
            info: 'ℹ️'
        };

        return `
            <div class="border-l-4 p-4 rounded-lg ${typeColors[notification.type]} ${!notification.isRead ? 'font-medium' : ''} fade-in">
                <div class="flex justify-between items-start">
                    <div class="flex-1">
                        <div class="flex items-center space-x-2 mb-1">
                            <span>${typeIcons[notification.type]}</span>
                            <h4 class="text-sm font-medium text-gray-900">${notification.title}</h4>
                            ${!notification.isRead ? '<span class="bg-red-500 text-white text-xs px-2 py-1 rounded-full">Nueva</span>' : ''}
                        </div>
                        <p class="text-sm text-gray-600 mb-2">${notification.message}</p>
                        <div class="text-xs text-gray-500">${notification.date}</div>
                    </div>
                    <div class="flex space-x-1 ml-4">
                        <button onclick="toggleNotificationRead(${notification.id})" class="p-1 text-gray-400 hover:text-blue-500" title="${notification.isRead ? 'Marcar como no leída' : 'Marcar como leída'}">
                            ${notification.isRead ? '📧' : '📬'}
                        </button>
                        <button onclick="deleteNotification(${notification.id})" class="p-1 text-gray-400 hover:text-red-500" title="Eliminar notificación">
                            🗑️
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

function updateNotificationCount() {
    const unreadCount = notifications.filter(n => !n.isRead).length;
    const badge = document.getElementById('unreadCount');
    if (unreadCount > 0) {
        badge.textContent = unreadCount;
        badge.classList.remove('hidden');
    } else {
        badge.classList.add('hidden');
    }
}

function toggleNotificationRead(notificationId) {
    const notification = notifications.find(n => n.id === notificationId);
    if (notification) {
        notification.isRead = !notification.isRead;
        renderNotifications();
        updateNotificationCount();
        showNotification(`Notificación marcada como ${notification.isRead ? 'leída' : 'no leída'}`, 'info');
    }
}

function deleteNotification(notificationId) {
    const index = notifications.findIndex(n => n.id === notificationId);
    if (index !== -1) {
        notifications.splice(index, 1);
        renderNotifications();
        updateNotificationCount();
        showNotification('Notificación eliminada', 'info');
    }
}

function markAllNotificationsRead() {
    notifications.forEach(n => n.isRead = true);
    renderNotifications();
    updateNotificationCount();
    showNotification('Todas las notificaciones marcadas como leídas', 'success');
}

function clearAllNotifications() {
    notifications.length = 0;
    renderNotifications();
    updateNotificationCount();
    showNotification('Todas las notificaciones eliminadas', 'info');
}

function applyNotificationFilters() {
    const statusFilter = document.getElementById('notificationStatusFilter').value;
    const readFilter = document.getElementById('notificationReadFilter').value;

    let filtered = notifications;

    if (statusFilter) {
        filtered = filtered.filter(n => n.type === statusFilter);
    }
    if (readFilter === 'read') {
        filtered = filtered.filter(n => n.isRead);
    } else if (readFilter === 'unread') {
        filtered = filtered.filter(n => !n.isRead);
    }

    renderNotifications(filtered);
    showNotification(`${filtered.length} notificaciones encontradas`, 'info');
}

function createNotification(title, message, type = 'info', scrapingResultId = null) {
    const newNotification = {
        id: notifications.length + 1,
        title: title,
        message: message,
        type: type,
        date: new Date().toLocaleString('es-ES'),
        isRead: false,
        scrapingResultId: scrapingResultId
    };
    
    notifications.unshift(newNotification);
    updateNotificationCount();
    
    // Show toast notification
    showNotification(title, type);
}

function setupEventListeners() {
    // Auth forms
    loginForm.addEventListener('submit', handleLogin);
    registerForm.addEventListener('submit', handleRegister);
    document.getElementById('showRegister').addEventListener('click', () => {
        loginScreen.classList.add('hidden');
        registerScreen.classList.remove('hidden');
    });
    document.getElementById('showLogin').addEventListener('click', () => {
        registerScreen.classList.add('hidden');
        loginScreen.classList.remove('hidden');
    });

    // User menu
    document.getElementById('userMenuBtn').addEventListener('click', toggleUserMenu);
    document.getElementById('logoutBtn').addEventListener('click', logout);
    document.getElementById('editProfileBtn').addEventListener('click', showEditProfileModal);
    document.getElementById('changePasswordBtn').addEventListener('click', showChangePasswordModal);
    document.getElementById('deleteAccountBtn').addEventListener('click', showDeleteAccountModal);

    // Tabs
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', () => switchTab(btn.dataset.tab));
    });

    // Filters
    document.getElementById('applyFilters').addEventListener('click', applyFilters);
    document.getElementById('clearFilters').addEventListener('click', clearFilters);

    // Sources
    document.getElementById('addSourceForm').addEventListener('submit', addSource);

    // Scraping
    document.getElementById('startScraping').addEventListener('click', startScraping);
    document.getElementById('clearOldLogs').addEventListener('click', clearOldLogs);

    // Notifications
    document.getElementById('markAllRead').addEventListener('click', markAllNotificationsRead);
    document.getElementById('clearAllNotifications').addEventListener('click', clearAllNotifications);
    document.getElementById('applyNotificationFilters').addEventListener('click', applyNotificationFilters);

    // Modals
    document.getElementById('cancelPasswordChange').addEventListener('click', hideChangePasswordModal);
    document.getElementById('changePasswordForm').addEventListener('submit', changePassword);
    document.getElementById('cancelDeleteAccount').addEventListener('click', hideDeleteAccountModal);
    document.getElementById('confirmDeleteAccount').addEventListener('click', deleteAccount);
    document.getElementById('cancelProfileEdit').addEventListener('click', hideEditProfileModal);
    document.getElementById('editProfileForm').addEventListener('submit', saveProfile);
    document.getElementById('closeArticleDetail').addEventListener('click', hideArticleDetailModal);
    document.getElementById('openArticleLink').addEventListener('click', openArticleLinkFromModal);
    document.getElementById('toggleArticleFavorite').addEventListener('click', toggleArticleFavoriteFromModal);

    // Close modals on outside click
    document.addEventListener('click', (e) => {
        if (e.target.classList.contains('fixed')) {
            hideAllModals();
        }
        if (!e.target.closest('#userMenuBtn') && !e.target.closest('#userMenu')) {
            document.getElementById('userMenu').classList.add('hidden');
        }
    });
}

function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    // Simulate login
    if (username && password) {
        currentUser.username = username;
        document.getElementById('currentUser').textContent = `${currentUser.firstName} ${currentUser.lastName}`;
        loginScreen.classList.add('hidden');
        dashboard.classList.remove('hidden');
        updateNotificationCount();
        showNotification('Sesión iniciada correctamente', 'success');
    } else {
        showNotification('Credenciales inválidas', 'error');
    }
}

function handleRegister(e) {
    e.preventDefault();
    const username = document.getElementById('newUsername').value;
    const password = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    
    if (password !== confirmPassword) {
        showNotification('Las contraseñas no coinciden', 'error');
        return;
    }
    
    // Simulate registration
    showNotification('Cuenta creada exitosamente', 'success');
    registerScreen.classList.add('hidden');
    loginScreen.classList.remove('hidden');
}

function logout() {
    currentUser = null;
    dashboard.classList.add('hidden');
    loginScreen.classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
    showNotification('Sesión cerrada', 'info');
}

function toggleUserMenu() {
    document.getElementById('userMenu').classList.toggle('hidden');
}

function switchTab(tabName) {
    // Update tab buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active', 'border-blue-500', 'text-blue-600');
        btn.classList.add('border-transparent', 'text-gray-500');
    });
    document.querySelector(`[data-tab="${tabName}"]`).classList.add('active', 'border-blue-500', 'text-blue-600');
    document.querySelector(`[data-tab="${tabName}"]`).classList.remove('border-transparent', 'text-gray-500');

    // Show/hide tab content
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.add('hidden');
    });
    document.getElementById(`${tabName}Tab`).classList.remove('hidden');

    // Load tab-specific content
    if (tabName === 'favorites') {
        renderFavorites();
    } else if (tabName === 'notifications') {
        renderNotifications();
    }
}

function renderArticles(filteredArticles = null) {
    const articlesToRender = filteredArticles || articles.filter(a => !a.isDiscarded);
    const container = document.getElementById('articlesList');
    
    if (articlesToRender.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No se encontraron artículos</div>';
        return;
    }

    container.innerHTML = articlesToRender.map(article => `
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 fade-in">
            <div class="flex justify-between items-start mb-3">
                <div class="flex-1">
                    <h3 class="text-lg font-medium text-gray-900 mb-2 cursor-pointer hover:text-blue-600" onclick="showArticleDetail(${article.id})">${article.title}</h3>
                    <p class="text-gray-600 text-sm mb-3">${article.content.substring(0, 150)}...</p>
                    <div class="flex items-center space-x-4 text-sm text-gray-500">
                        <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded-full">${article.source}</span>
                        <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full">${article.category}</span>
                        <span>${article.date}</span>
                    </div>
                    <div class="mt-2">
                        <div class="flex flex-wrap gap-1">
                            ${article.keywords.map(keyword => `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">${keyword}</span>`).join('')}
                        </div>
                    </div>
                </div>
                <div class="flex flex-col space-y-2 ml-4">
                    <button onclick="showArticleDetail(${article.id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-blue-500" title="Ver artículo completo">
                        👁️
                    </button>
                    <button onclick="openArticleLink(${article.id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-green-500" title="Abrir enlace">
                        🔗
                    </button>
                    <button onclick="toggleFavorite(${article.id})" class="p-2 rounded-lg hover:bg-gray-100 ${article.isFavorite ? 'text-red-500' : 'text-gray-400'}" title="Favorito">
                        ${article.isFavorite ? '❤️' : '🤍'}
                    </button>
                    <button onclick="discardArticle(${article.id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-red-500" title="Descartar">
                        🗑️
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function renderFavorites() {
    const favorites = articles.filter(a => a.isFavorite && !a.isDiscarded);
    const container = document.getElementById('favoritesList');
    
    if (favorites.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No tienes artículos favoritos</div>';
        return;
    }

    container.innerHTML = favorites.map(article => `
        <div class="bg-gray-50 rounded-lg p-4 fade-in">
            <div class="flex justify-between items-start">
                <div class="flex-1">
                    <h4 class="font-medium text-gray-900 mb-1">${article.title}</h4>
                    <div class="flex items-center space-x-2 text-sm text-gray-500">
                        <span>${article.source}</span>
                        <span>•</span>
                        <span>${article.date}</span>
                    </div>
                </div>
                <button onclick="toggleFavorite(${article.id})" class="p-1 text-red-500 hover:text-red-700">
                    ❤️
                </button>
            </div>
        </div>
    `).join('');
}

function renderSources() {
    const container = document.getElementById('sourcesList');
    // Sort sources alphabetically by name
    const sortedSources = [...sources].sort((a, b) => a.name.localeCompare(b.name));
    
    container.innerHTML = sortedSources.map(source => `
        <div class="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
            <div class="flex-1">
                <h4 class="font-medium text-gray-900">${source.name}</h4>
                <p class="text-sm text-gray-600">${source.url}</p>
            </div>
            <div class="flex items-center space-x-2">
                <span class="px-2 py-1 text-xs rounded-full ${source.active ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                    ${source.active ? 'Activa' : 'Inactiva'}
                </span>
                <button onclick="toggleSource(${source.id})" class="p-2 text-gray-400 hover:text-gray-600">
                    ${source.active ? '⏸️' : '▶️'}
                </button>
                <button onclick="removeSource(${source.id})" class="p-2 text-gray-400 hover:text-red-500">
                    🗑️
                </button>
            </div>
        </div>
    `).join('');
}

function renderScrapingHistory() {
    const container = document.getElementById('scrapingHistory');
    container.innerHTML = scrapingHistory.map(entry => `
        <div class="p-3 bg-gray-50 rounded-lg">
            <div class="flex justify-between items-center">
                <div>
                    <div class="font-medium text-sm">${entry.source}</div>
                    <div class="text-xs text-gray-500">${entry.date}</div>
                </div>
                <div class="text-right">
                    <span class="px-2 py-1 text-xs rounded-full ${entry.status === 'exitoso' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                        ${entry.status}
                    </span>
                    <div class="text-xs text-gray-500 mt-1">${entry.articlesFound} artículos</div>
                </div>
            </div>
        </div>
    `).join('');
}

function populateSourceFilters() {
    const select = document.getElementById('sourceFilter');
    select.innerHTML = '<option value="">Todas las fuentes</option>' + 
        sources.map(source => `<option value="${source.name}">${source.name}</option>`).join('');
}

function updateStats() {
    document.getElementById('totalArticles').textContent = `${articles.filter(a => !a.isDiscarded).length} artículos`;
    document.getElementById('totalSources').textContent = `${sources.filter(s => s.active).length} fuentes`;
    
    // Update active sources count in scraping tab
    const activeSourcesElement = document.getElementById('activeSources');
    if (activeSourcesElement) {
        const activeCount = sources.filter(s => s.active).length;
        activeSourcesElement.textContent = `${activeCount} fuentes activas`;
    }
}

function toggleFavorite(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article) {
        article.isFavorite = !article.isFavorite;
        renderArticles();
        renderFavorites();
        showNotification(article.isFavorite ? 'Agregado a favoritos' : 'Removido de favoritos', 'info');
    }
}

function discardArticle(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article) {
        article.isDiscarded = true;
        renderArticles();
        updateStats();
        showNotification('Artículo descartado', 'info');
    }
}

function applyFilters() {
    const titleFilter = document.getElementById('titleFilter').value.toLowerCase();
    const keywordFilter = document.getElementById('keywordFilter').value.toLowerCase();
    const categoryFilter = document.getElementById('categoryFilter').value;
    const sourceFilter = document.getElementById('sourceFilter').value;
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;

    let filtered = articles.filter(article => !article.isDiscarded);

    if (titleFilter) {
        filtered = filtered.filter(a => a.title.toLowerCase().includes(titleFilter));
    }
    if (keywordFilter) {
        filtered = filtered.filter(a => a.keywords.some(k => k.toLowerCase().includes(keywordFilter)));
    }
    if (categoryFilter) {
        filtered = filtered.filter(a => a.category === categoryFilter);
    }
    if (sourceFilter) {
        filtered = filtered.filter(a => a.source === sourceFilter);
    }
    if (dateFrom) {
        filtered = filtered.filter(a => a.date >= dateFrom);
    }
    if (dateTo) {
        filtered = filtered.filter(a => a.date <= dateTo);
    }

    renderArticles(filtered);
    showNotification(`${filtered.length} artículos encontrados`, 'info');
}

function clearFilters() {
    document.getElementById('titleFilter').value = '';
    document.getElementById('keywordFilter').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('sourceFilter').value = '';
    document.getElementById('dateFrom').value = '';
    document.getElementById('dateTo').value = '';
    renderArticles();
    showNotification('Filtros limpiados', 'info');
}

function addSource(e) {
    e.preventDefault();
    const name = document.getElementById('sourceName').value;
    const url = document.getElementById('sourceUrl').value;
    
    const newSource = {
        id: sources.length + 1,
        name: name,
        url: url,
        active: true
    };
    
    sources.push(newSource);
    renderSources();
    populateSourceFilters();
    updateStats();
    document.getElementById('addSourceForm').reset();
    showNotification('Fuente agregada exitosamente', 'success');
}

function toggleSource(sourceId) {
    const source = sources.find(s => s.id === sourceId);
    if (source) {
        source.active = !source.active;
        renderSources();
        updateStats();
        showNotification(`Fuente ${source.active ? 'activada' : 'desactivada'}`, 'info');
    }
}

function removeSource(sourceId) {
    const index = sources.findIndex(s => s.id === sourceId);
    if (index !== -1) {
        sources.splice(index, 1);
        renderSources();
        populateSourceFilters();
        updateStats();
        showNotification('Fuente eliminada', 'info');
    }
}

function startScraping() {
    const button = document.getElementById('startScraping');
    const status = document.getElementById('scrapingStatus');
    
    button.disabled = true;
    button.textContent = '🔄 Scraping en progreso...';
    status.textContent = 'Extrayendo artículos...';
    
    // Create notification for scraping start
    createNotification('Scraping iniciado', 'Proceso de extracción de artículos en curso...', 'info');
    
    // Simulate scraping process
    setTimeout(() => {
        const newArticlesCount = Math.floor(Math.random() * 10) + 1;
        const isSuccess = Math.random() > 0.2; // 80% success rate
        
        const newEntry = {
            id: scrapingHistory.length + 1,
            date: new Date().toLocaleString('es-ES'),
            status: isSuccess ? 'exitoso' : 'fallido',
            articlesFound: isSuccess ? newArticlesCount : 0,
            source: 'Múltiples fuentes'
        };
        
        scrapingHistory.unshift(newEntry);
        
        if (isSuccess) {
            // Add new articles to simulate scraping results
            const newArticleIds = addNewScrapedArticles(newArticlesCount, newEntry.id);
            
            status.textContent = 'Scraping completado. Revisa los nuevos artículos.';
            
            // Update displays after scraping
            renderScrapingHistory();
            renderArticles();
            updateStats();
            
            button.disabled = false;
            button.textContent = '🚀 Iniciar Scraping';
            status.textContent = 'Listo para iniciar';
            
            // Show review modal for new articles
            showArticleReviewModal(newArticleIds);
            
            // Create completion notification
            createNotification('Scraping completado', 
                `${newArticlesCount} nuevos artículos encontrados. Revísalos para decidir cuáles conservar.`, 
                'success', newEntry.id);
        } else {
            renderScrapingHistory();
            updateStats();
            
            button.disabled = false;
            button.textContent = '🚀 Iniciar Scraping';
            status.textContent = 'Listo para iniciar';
            
            createNotification('Error en scraping', 'No se pudieron extraer artículos de las fuentes', 'error', newEntry.id);
        }
    }, 3000);
}

function addNewScrapedArticles(count, scrapingResultId) {
    const sampleTitles = [
        "Nuevos avances en energía renovable",
        "Cambios en la política fiscal nacional",
        "Descubrimiento científico revolucionario",
        "Innovaciones en transporte público",
        "Reformas en el sistema educativo",
        "Desarrollo de nuevas tecnologías médicas",
        "Impacto del cambio climático en la agricultura",
        "Tendencias en el mercado inmobiliario",
        "Avances en inteligencia artificial aplicada",
        "Nuevas regulaciones ambientales"
    ];
    
    const categories = ["tecnologia", "economia", "salud", "politica", "ciencia"];
    const sources = ["TechNews", "Noticias Económicas", "Salud Hoy", "Ciencia Diaria", "Política Actual"];
    const newArticleIds = [];
    
    for (let i = 0; i < count; i++) {
        const newArticle = {
            id: articles.length + 1,
            title: sampleTitles[Math.floor(Math.random() * sampleTitles.length)],
            content: "Contenido del artículo extraído durante el proceso de scraping. Este texto representa el contenido completo del artículo que fue procesado automáticamente.",
            source: sources[Math.floor(Math.random() * sources.length)],
            category: categories[Math.floor(Math.random() * categories.length)],
            date: new Date().toISOString().split('T')[0],
            url: `https://example.com/article-${articles.length + 1}`,
            keywords: ["scraping", "automatico", "nuevo"],
            isFavorite: false,
            isDiscarded: false,
            scrapingResultId: scrapingResultId,
            isNew: true // Mark as new for review
        };
        
        articles.push(newArticle);
        newArticleIds.push(newArticle.id);
    }
    
    return newArticleIds;
}

function showArticleReviewModal(newArticleIds) {
    const modal = document.createElement('div');
    modal.id = 'articleReviewModal';
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
    
    const newArticles = articles.filter(a => newArticleIds.includes(a.id));
    
    modal.innerHTML = `
        <div class="bg-white rounded-lg p-6 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <div class="flex justify-between items-center mb-6">
                <h3 class="text-xl font-bold text-gray-900">📋 Revisar Nuevos Artículos</h3>
                <button id="closeReviewModal" class="text-gray-400 hover:text-gray-600">
                    <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                </button>
            </div>
            
            <div class="mb-4 p-4 bg-blue-50 rounded-lg">
                <p class="text-blue-800">Se encontraron <strong>${newArticles.length} nuevos artículos</strong>. Revisa cada uno y decide si conservarlo o descartarlo.</p>
            </div>
            
            <div class="space-y-4 mb-6" id="reviewArticlesList">
                ${newArticles.map(article => `
                    <div class="border border-gray-200 rounded-lg p-4" data-article-id="${article.id}">
                        <div class="flex justify-between items-start">
                            <div class="flex-1">
                                <h4 class="font-medium text-gray-900 mb-2">${article.title}</h4>
                                <p class="text-gray-600 text-sm mb-3">${article.content.substring(0, 200)}...</p>
                                <div class="flex items-center space-x-4 text-sm text-gray-500 mb-3">
                                    <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded-full">${article.source}</span>
                                    <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full">${article.category}</span>
                                    <span>${article.date}</span>
                                </div>
                                <div class="flex flex-wrap gap-1">
                                    ${article.keywords.map(keyword => `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">${keyword}</span>`).join('')}
                                </div>
                            </div>
                            <div class="flex flex-col space-y-2 ml-4">
                                <button onclick="keepArticleFromReview(${article.id})" class="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors text-sm">
                                    ✅ Conservar
                                </button>
                                <button onclick="discardArticleFromReview(${article.id})" class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors text-sm">
                                    🗑️ Descartar
                                </button>
                            </div>
                        </div>
                    </div>
                `).join('')}
            </div>
            
            <div class="flex justify-between items-center pt-4 border-t border-gray-200">
                <div class="flex space-x-2">
                    <button id="keepAllArticles" class="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors">
                        ✅ Conservar Todos
                    </button>
                    <button id="discardAllArticles" class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors">
                        🗑️ Descartar Todos
                    </button>
                </div>
                <button id="finishReview" class="bg-blue-600 text-white px-6 py-2 rounded-lg hover:bg-blue-700 transition-colors">
                    Finalizar Revisión
                </button>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Add event listeners
    document.getElementById('closeReviewModal').addEventListener('click', closeArticleReviewModal);
    document.getElementById('finishReview').addEventListener('click', closeArticleReviewModal);
    document.getElementById('keepAllArticles').addEventListener('click', () => keepAllFromReview(newArticleIds));
    document.getElementById('discardAllArticles').addEventListener('click', () => discardAllFromReview(newArticleIds));
}

function keepArticleFromReview(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article) {
        article.isNew = false;
        const articleElement = document.querySelector(`[data-article-id="${articleId}"]`);
        if (articleElement) {
            articleElement.style.backgroundColor = '#f0fdf4';
            articleElement.style.borderColor = '#22c55e';
        }
        showNotification('Artículo conservado', 'success');
    }
}

function discardArticleFromReview(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article) {
        article.isDiscarded = true;
        article.isNew = false;
        const articleElement = document.querySelector(`[data-article-id="${articleId}"]`);
        if (articleElement) {
            articleElement.style.backgroundColor = '#fef2f2';
            articleElement.style.borderColor = '#ef4444';
            articleElement.style.opacity = '0.6';
        }
        showNotification('Artículo descartado', 'info');
    }
}

function keepAllFromReview(articleIds) {
    articleIds.forEach(id => {
        const article = articles.find(a => a.id === id);
        if (article && !article.isDiscarded) {
            article.isNew = false;
            const articleElement = document.querySelector(`[data-article-id="${id}"]`);
            if (articleElement) {
                articleElement.style.backgroundColor = '#f0fdf4';
                articleElement.style.borderColor = '#22c55e';
            }
        }
    });
    showNotification('Todos los artículos conservados', 'success');
}

function discardAllFromReview(articleIds) {
    articleIds.forEach(id => {
        const article = articles.find(a => a.id === id);
        if (article) {
            article.isDiscarded = true;
            article.isNew = false;
            const articleElement = document.querySelector(`[data-article-id="${id}"]`);
            if (articleElement) {
                articleElement.style.backgroundColor = '#fef2f2';
                articleElement.style.borderColor = '#ef4444';
                articleElement.style.opacity = '0.6';
            }
        }
    });
    showNotification('Todos los artículos descartados', 'info');
}

function closeArticleReviewModal() {
    const modal = document.getElementById('articleReviewModal');
    if (modal) {
        modal.remove();
        renderArticles();
        updateStats();
        
        const keptCount = articles.filter(a => !a.isDiscarded && !a.isNew).length - articles.filter(a => !a.isDiscarded && !a.isNew && !a.scrapingResultId).length;
        const discardedCount = articles.filter(a => a.isDiscarded && a.scrapingResultId === scrapingHistory[0]?.id).length;
        
        createNotification('Revisión completada', 
            `Artículos procesados: ${keptCount} conservados, ${discardedCount} descartados`, 
            'info');
    }
}

function clearOldLogs() {
    const oldLogsCount = scrapingHistory.length;
    scrapingHistory.length = Math.min(scrapingHistory.length, 5); // Keep only last 5 entries
    renderScrapingHistory();
    
    // Also clear related notifications
    const removedLogs = oldLogsCount - scrapingHistory.length;
    if (removedLogs > 0) {
        showNotification(`${removedLogs} logs antiguos eliminados`, 'info');
    }
}

function showEditProfileModal() {
    document.getElementById('firstName').value = currentUser.firstName;
    document.getElementById('lastName').value = currentUser.lastName;
    document.getElementById('email').value = currentUser.email;
    document.getElementById('editProfileModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

function hideEditProfileModal() {
    document.getElementById('editProfileModal').classList.add('hidden');
}

function saveProfile(e) {
    e.preventDefault();
    const newFirstName = document.getElementById('firstName').value;
    const newLastName = document.getElementById('lastName').value;
    const newEmail = document.getElementById('email').value;
    
    // Simulate email uniqueness check
    const emailExists = newEmail !== currentUser.email && Math.random() < 0.1; // 10% chance of duplicate
    
    if (emailExists) {
        showNotification('Este correo ya está en uso', 'error');
        return;
    }
    
    currentUser.firstName = newFirstName;
    currentUser.lastName = newLastName;
    currentUser.email = newEmail;
    
    document.getElementById('currentUser').textContent = `${currentUser.firstName} ${currentUser.lastName}`;
    
    hideEditProfileModal();
    showNotification('Perfil actualizado exitosamente', 'success');
}

function showArticleDetail(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (!article) return;
    
    document.getElementById('articleDetailTitle').textContent = article.title;
    document.getElementById('articleDetailContent').innerHTML = `
        <p class="text-gray-700 leading-relaxed mb-4">${article.content}</p>
        <div class="bg-gray-50 p-4 rounded-lg">
            <h4 class="font-medium text-gray-900 mb-2">Palabras clave:</h4>
            <div class="flex flex-wrap gap-2">
                ${article.keywords.map(keyword => `<span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm">${keyword}</span>`).join('')}
            </div>
        </div>
    `;
    
    document.getElementById('articleDetailMeta').innerHTML = `
        <span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full">${article.source}</span>
        <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full">${article.category}</span>
        <span>${article.date}</span>
    `;
    
    // Store current article ID for modal actions
    document.getElementById('articleDetailModal').dataset.articleId = articleId;
    
    // Update favorite button
    const favoriteBtn = document.getElementById('toggleArticleFavorite');
    favoriteBtn.innerHTML = article.isFavorite ? '❤️ Quitar de Favoritos' : '🤍 Agregar a Favoritos';
    
    document.getElementById('articleDetailModal').classList.remove('hidden');
}

function hideArticleDetailModal() {
    document.getElementById('articleDetailModal').classList.add('hidden');
}

function openArticleLinkFromModal() {
    const articleId = document.getElementById('articleDetailModal').dataset.articleId;
    const article = articles.find(a => a.id == articleId);
    if (article && article.url) {
        window.open(article.url, '_blank', 'noopener,noreferrer');
        showNotification('Abriendo enlace del artículo', 'info');
    } else {
        showNotification('Enlace no disponible', 'error');
    }
}

function openArticleLink(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article && article.url) {
        window.open(article.url, '_blank', 'noopener,noreferrer');
        showNotification('Abriendo enlace del artículo', 'info');
    } else {
        showNotification('Enlace no disponible', 'error');
    }
}

function toggleArticleFavoriteFromModal() {
    const articleId = parseInt(document.getElementById('articleDetailModal').dataset.articleId);
    toggleFavorite(articleId);
    
    // Update modal button
    const article = articles.find(a => a.id === articleId);
    const favoriteBtn = document.getElementById('toggleArticleFavorite');
    favoriteBtn.innerHTML = article.isFavorite ? '❤️ Quitar de Favoritos' : '🤍 Agregar a Favoritos';
}

function discardArticle(articleId) {
    const article = articles.find(a => a.id === articleId);
    if (article) {
        article.isDiscarded = true;
        
        // Remove associated source if no other articles use it
        const sourceInUse = articles.some(a => a.source === article.source && !a.isDiscarded && a.id !== articleId);
        if (!sourceInUse) {
            const sourceIndex = sources.findIndex(s => s.name === article.source);
            if (sourceIndex !== -1) {
                sources.splice(sourceIndex, 1);
                populateSourceFilters();
                renderSources();
            }
        }
        
        renderArticles();
        updateStats();
        showNotification('Artículo descartado', 'info');
    }
}

function removeSource(sourceId) {
    const source = sources.find(s => s.id === sourceId);
    if (source) {
        // Remove all articles from this source
        articles.forEach(article => {
            if (article.source === source.name) {
                article.isDiscarded = true;
            }
        });
        
        const index = sources.findIndex(s => s.id === sourceId);
        sources.splice(index, 1);
        renderSources();
        populateSourceFilters();
        renderArticles();
        updateStats();
        showNotification('Fuente y artículos asociados eliminados', 'info');
    }
}

function deleteAccount() {
    // Simulate account deletion - remove all user data
    articles.length = 0;
    notifications.length = 0;
    scrapingHistory.length = 0;
    sources.length = 0;
    
    hideDeleteAccountModal();
    showNotification('Cuenta y todos los datos asociados eliminados', 'info');
    logout();
}

function showChangePasswordModal() {
    document.getElementById('changePasswordModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

function hideChangePasswordModal() {
    document.getElementById('changePasswordModal').classList.add('hidden');
}

function changePassword(e) {
    e.preventDefault();
    // Simulate password change
    hideChangePasswordModal();
    showNotification('Contraseña actualizada exitosamente', 'success');
    document.getElementById('changePasswordForm').reset();
}

function showDeleteAccountModal() {
    document.getElementById('deleteAccountModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

function hideDeleteAccountModal() {
    document.getElementById('deleteAccountModal').classList.add('hidden');
}

function deleteAccount() {
    // Simulate account deletion
    hideDeleteAccountModal();
    showNotification('Cuenta eliminada exitosamente', 'info');
    logout();
}

function hideAllModals() {
    document.getElementById('changePasswordModal').classList.add('hidden');
    document.getElementById('deleteAccountModal').classList.add('hidden');
    document.getElementById('editProfileModal').classList.add('hidden');
    document.getElementById('articleDetailModal').classList.add('hidden');
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification bg-white border-l-4 p-4 rounded-lg shadow-lg max-w-sm ${
        type === 'success' ? 'border-green-500' : 
        type === 'error' ? 'border-red-500' : 
        type === 'warning' ? 'border-yellow-500' : 'border-blue-500'
    }`;
    
    const icon = type === 'success' ? '✅' : type === 'error' ? '❌' : type === 'warning' ? '⚠️' : 'ℹ️';
    
    notification.innerHTML = `
        <div class="flex items-center">
            <span class="mr-2">${icon}</span>
            <span class="text-gray-800">${message}</span>
        </div>
    `;
    
    document.getElementById('notifications').appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 4000);
}

// Initialize the app
init();