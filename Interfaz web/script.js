// Sample data
let currentUser = {
    id: -1,
    nombres: "",
    apellidos: "",
    correo: ""
};

let articles = [];
let notifications = [];

let sources = [
    { id: 1, name: "El Espectador", url: "https://elespectador.com", active: true },
];

let scrapingHistory = [];

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

//done
async function renderNotifications(filteredNotifications = null) {
    if (filteredNotifications == null){
        try{
            response = await fetch(`/api/get-notifications?id=${currentUser.id}`,
                {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                }
            );
            var response = await response.json();
            notifications = response.notifList;
        }
        catch (error){
            console.log("error al obtener notificaciones", error)
        }
    }
    updateNotificationCount();
    const notificationsToRender = filteredNotifications || notifications;
    const container = document.getElementById('notificationsList');
    
    if (notificationsToRender.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No hay notificaciones</div>';
        return;
    }

    container.innerHTML = notificationsToRender.map(notification => {
        const typeColors = [
            'border-green-500 bg-green-50',
            'border-red-500 bg-red-50',
            'border-blue-500 bg-blue-50',
            'border-yellow-500 bg-yellow-50'
        ];
        
        const typeIcons = [
            '✅',
            '❌',
            'ℹ️',
            '⚠️'
        ];

        return `
            <div class="border-l-4 p-4 rounded-lg ${typeColors[notification.Tipo]} ${!notification.Leido ? 'font-medium' : ''} fade-in">
                <div class="flex justify-between items-start">
                    <div class="flex-1">
                        <div class="flex items-center space-x-2 mb-1">
                            <span>${typeIcons[notification.Tipo]}</span>
                            <h4 class="text-sm font-medium text-gray-900">${notification.Mensaje}</h4>
                            ${!notification.Leido ? '<span class="bg-red-500 text-white text-xs px-2 py-1 rounded-full">Nueva</span>' : ''}
                        </div>
                        <p class="text-sm text-gray-600 mb-2">${notification.Mensaje}</p>
                        <div class="text-xs text-gray-500"></div>
                    </div>
                    <div class="flex space-x-1 ml-4">
                        <button onclick="toggleNotificationRead(${notification.Id})" class="p-1 text-gray-400 hover:text-blue-500" title="${notification.Leido ? 'Marcar como no leída' : 'Marcar como leída'}">
                            ${notification.Leido ? '📧' : '📬'}
                        </button>
                        <button onclick="deleteNotification(${notification.Id})" class="p-1 text-gray-400 hover:text-red-500" title="Eliminar notificación">
                            🗑️
                        </button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

//done
function updateNotificationCount() {
    const unreadCount = notifications.filter(n => !n.Leido).length;
    const badge = document.getElementById('unreadCount');
    if (unreadCount > 0) {
        badge.textContent = unreadCount;
        badge.classList.remove('hidden');
    } else {
        badge.classList.add('hidden');
    }
}

//done
async function toggleNotificationRead(notificationId) {
    const notification = notifications.find(n => n.Id === notificationId);
    if (notification) {
		try{
            const response = await fetch('/api/update-notif-read',
                {
                    method: "PATCH",
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(notificationId)
                }
            );
		}
        catch(error){
            console.error('error toggling notification read status', error)
        }
        renderNotifications();
        showAlert(`Notificación marcada como ${notification.Leida ? 'leída' : 'no leída'}`, 'info');
    }
}

//done
async function deleteNotification(notificationId) {
    const index = notifications.findIndex(n => n.Id === notificationId);
    let success = true;
    if (index !== -1) {
		try{
            const response = await fetch(`/api/delete-notif?id=${notificationId}`,
                {
                    method: "DELETE",
                    headers: {
                        'Content-Type': 'application/json',
                    },
                }
            );
		}
        catch(error){
            success = false;
            console.error('error deleting notification', error)
        }
        if(success){
            notifications.splice(index, 1);
            showAlert('Notificación eliminada', 'info');
        }
        renderNotifications();
    }
}

//done
function markAllNotificationsRead() {
    notifications.forEach(n => {
        if(!n.Leido){
            toggleNotificationRead(n.Id);
        }
    });
    renderNotifications();
    showAlert('Todas las notificaciones marcadas como leídas', 'success');
}

//done
function clearAllNotifications() {
    notifications.forEach(n => deleteNotification(n.Id));
    renderNotifications();
    showAlert('Todas las notificaciones eliminadas', 'info');
}

//done
async function applyNotificationFilters() {
    const statusFilter = document.getElementById('notificationStatusFilter').value;
    const readFilter = document.getElementById('notificationReadFilter').value;
    
    if(!(statusFilter || readFilter)){
        renderNotifications();
        return;
    }
    
    let filtered = notifications;

    try{
        var response = await fetch(`/api/get-notifications?id=${currentUser.id}&type=${statusFilter}&read=${readFilter}`,
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        );
        response = await response.json();
        filtered = response.notifList;
    }
    catch(error){
        console.error('error filtrando notificaciones', error)
    }

    renderNotifications(filtered);
    showAlert(`${filtered.length} notificaciones encontradas`, 'info');
}


//done
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

// done
async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    var userId = -1;
    try{
        response = await fetch('/api/login',{
            method: "POST",
			headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                Correo: email,
                Contraseña: password
            })
        });
        userId = await response.json();
    }
    catch(error){
        console.error('error al iniciar sesion', error)
    }
    
    if (userId != -1 & userId != "") {
        try {
            response = await fetch(`/api/get-user?id=${userId}`,{
                method: "GET",
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            let userData = await response.json();
            currentUser.id = userData.Id;
            currentUser.nombres = userData.Nombres;
            currentUser.apellidos = userData.Apellidos;
            currentUser.correo = userData.Correo;
        } 
        catch (error) {
            console.error('error al obtener datos de usuario', error)
        }
        document.getElementById('currentUser').textContent = `${currentUser.nombres}`;
        loginScreen.classList.add('hidden');
        dashboard.classList.remove('hidden');
        updateNotificationCount();
        renderArticles();
        renderSources();
        renderScrapingHistory();
        renderNotifications();
        updateNotificationCount();
        updateStats();
        showAlert('Sesión iniciada correctamente', 'success');
    } else {
        showAlert('Credenciales inválidas', 'error');
    }
}

//done
async function handleRegister(e) {
    e.preventDefault();
    const names = document.getElementById('newNames').value;
    const lastNames = document.getElementById('newLastNames').value;
    const email = document.getElementById('newEmail').value;
    const password = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    
    if (password !== confirmPassword) {
        showAlert('Las contraseñas no coinciden', 'error');
        return;
    }
    let success = false;
    
    try {
        const response = await fetch('/api/register-user', {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                Nombres: names,
                Apellidos : lastNames,
                Correo: email,
                Contraseña: password
            })
        });
        success = response.json();

    } catch (error) {
        console.error('error al registrar usuario', error)
    }

    if(success){
        showAlert('Cuenta creada exitosamente', 'success');
        registerScreen.classList.add('hidden');
        loginScreen.classList.remove('hidden');
    }
    else{
        showAlert('Usuario no se pudo crear', 'error');
    }
}

//done
function logout() {
    currentUser.id = -1;
    currentUser.apellidos = "";
    currentUser.nombres = "";
    currentUser.correo = "";
    
    articles = [];
    scrapingHistory = [];
    sources = [];
    notifications = [];
    dashboard.classList.add('hidden');
    loginScreen.classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
    showAlert('Sesión cerrada', 'info');
}

//done
function toggleUserMenu() {
    document.getElementById('userMenu').classList.toggle('hidden');
}

//done
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

//done
async function renderArticles(filteredArticles = null) {
    if(filteredArticles == null){
        try {
            response = await fetch(`/api/get-articles?id=${currentUser.id}`,{
                method: "GET",
                headers: {
                    'Content-Type': 'application/json',
                },
            });
            let articleDetail = await response.json();
            articleDetail.forEach(a => a.Article.isDiscarded = false);
            articleDetail.forEach(a => a.Article.isNew = false);
            articleDetail.forEach(a => a.Article.Tema = a.Article.Tema.split(/[,]+/).filter(Boolean))
            articles = articleDetail;
        } catch (error) {
            console.error('error al mostrar articulos', error)
        }
        updateStats();
    }
    const articlesToRender = filteredArticles || articles.filter(a => !a.isDiscarded);
    const container = document.getElementById('articlesList');
    
    if (articlesToRender.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No se encontraron artículos</div>';
        return;
    }

    container.innerHTML = articlesToRender.map(a => `
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 fade-in">
            <div class="flex justify-between items-start mb-3">
                <div class="flex-1">
                    <h3 class="text-lg font-medium text-gray-900 mb-2 cursor-pointer hover:text-blue-600" onclick="showArticleDetail(${a.Article.Id})">${a.Article.Titular}</h3>
                    <p class="text-gray-600 text-sm mb-3">${a.Article.Cuerpo.substring(0, 150)}...</p>
                    <div class="flex items-center space-x-4 text-sm text-gray-500">
                        <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded-full">${a.Source.Nombre}</span>
                        <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full">${a.Source.Tipo}</span>
                        <span>${a.Article.Fecha}</span>
                    </div>
                    <div class="mt-2">
                        <div class="flex flex-wrap gap-1">
                            ${a.Article.Tema.map(keyword => `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">${keyword}</span>`).join('')}
                        </div>
                    </div>
                </div>
                <div class="flex flex-col space-y-2 ml-4">
                    <button onclick="showArticleDetail(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-blue-500" title="Ver artículo completo">
                        👁️
                    </button>
                    <button onclick="openArticleLink(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-green-500" title="Abrir enlace">
                        🔗
                    </button>
                    <button onclick="toggleFavorite(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 ${a.Article.Favorito ? 'text-red-500' : 'text-gray-400'}" title="Favorito">
                        ${a.Article.Favorito ? '❤️' : '🤍'}
                    </button>
                    <button onclick="discardArticle(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-red-500" title="Descartar">
                        🗑️
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}
//done
function renderFavorites() {
    const favorites = articles.filter(a => a.Article.Favorito);
    const container = document.getElementById('favoritesList');
    
    if (favorites.length === 0) {
        container.innerHTML = '<div class="text-center text-gray-500 py-8">No tienes artículos favoritos</div>';
        return;
    }

    container.innerHTML = favorites.map(a => `
        <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 fade-in">
            <div class="flex justify-between items-start mb-3">
                <div class="flex-1">
                    <h3 class="text-lg font-medium text-gray-900 mb-2 cursor-pointer hover:text-blue-600" onclick="showArticleDetail(${a.Article.Id})">${a.Article.Titular}</h3>
                    <p class="text-gray-600 text-sm mb-3">${a.Article.Cuerpo.substring(0, 150)}...</p>
                    <div class="flex items-center space-x-4 text-sm text-gray-500">
                        <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded-full">${a.Source.Nombre}</span>
                        <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full">${a.Source.Tipo}</span>
                        <span>${a.Article.Fecha}</span>
                    </div>
                    <div class="mt-2">
                        <div class="flex flex-wrap gap-1">
                            ${a.Article.Tema.map(keyword => `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">${keyword}</span>`).join('')}
                        </div>
                    </div>
                </div>
                <div class="flex flex-col space-y-2 ml-4">
                    <button onclick="showArticleDetail(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-blue-500" title="Ver artículo completo">
                        👁️
                    </button>
                    <button onclick="openArticleLink(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-green-500" title="Abrir enlace">
                        🔗
                    </button>
                    <button onclick="toggleFavorite(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 ${a.Article.Favorito ? 'text-red-500' : 'text-gray-400'}" title="Favorito">
                        ${a.Article.Favorito ? '❤️' : '🤍'}
                    </button>
                    <button onclick="discardArticle(${a.Article.Id})" class="p-2 rounded-lg hover:bg-gray-100 text-gray-400 hover:text-red-500" title="Descartar">
                        🗑️
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

//done
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

//done
async function renderScrapingHistory() {
    const container = document.getElementById('scrapingHistory');
    try{
        var response = await fetch(`/api/get-results?id=${currentUser.id}`,{
            method: "GET",
			headers: {
                'Content-Type': 'application/json',
            },
        });
        var results = await response.json();
        scrapingHistory = results;
    }
    catch(error){
        console.error('error al mostrar resultados de scraping')
    }
    const colors = ['bg-green-100 text-green-800','bg-red-100 text-red-800','bg-blue-100 text-blue-800']
    const texts = ["Exito", "Fallo", "En proceso"]

    container.innerHTML = scrapingHistory.map(entry => `
        <div class="p-3 bg-gray-50 rounded-lg">
            <div class="flex justify-between items-center">
                <div>
                    <div class="font-medium text-sm">${entry.Cantidad} articulos extraidos</div>
                    <div class="text-xs text-gray-500">${entry.FechaExtraccion}</div>
                </div>
                <div class="text-right">
                    <span class="px-2 py-1 text-xs rounded-full ${colors[entry.Estado]}">
                        ${texts[entry.Estado]}
                    </span>
                    <div class="text-xs text-gray-500 mt-1">${entry.Cantidad} artículos</div>
                </div>
            </div>
        </div>
    `).join('');
}

//done
function populateSourceFilters() {
    const select = document.getElementById('sourceFilter');
    select.innerHTML = '<option value="">Todas las fuentes</option>' + 
        sources.map(source => `<option value="${source.name}">${source.name}</option>`).join('');
}
//done
function updateStats() {
    document.getElementById('totalArticles').textContent = `${articles.length} artículos`;
    document.getElementById('totalSources').textContent = `${sources.filter(s => s.active).length} fuentes`;
    
    // Update active sources count in scraping tab
    const activeSourcesElement = document.getElementById('activeSources');
    if (activeSourcesElement) {
        const activeCount = sources.filter(s => s.active).length;
        activeSourcesElement.textContent = `${activeCount} fuentes activas`;
    }
}

//done
async function toggleFavorite(articleId) {
    const article = articles.find(a => a.Article.Id === articleId);
    if (article) {
        try {
            response = await fetch(`/api/update-article-fav`,{
                method: "PATCH",
                headers: {
                    'Content-Type': 'application/json',
                },
                body : JSON.stringify(articleId)
            });
            var success = await response.json();
            if(success){
                article.Article.Favorito = !article.Article.Favorito;
            }
        } catch (error) {
            console.error('error al asignar valor de favorito', error)
        }
        renderArticles();
        renderFavorites();
        showAlert(article.Article.Favorito ?  'Agregado a favoritos' : 'Removido de favoritos', 'info');
    }
    else{
        console.error(`article with id ${articleId} not found`);
    }
}


async function applyFilters() {
    const titleFilter = document.getElementById('titleFilter').value.toLowerCase();
    const keywordFilter = document.getElementById('keywordFilter').value.toLowerCase();
    const categoryFilter = document.getElementById('categoryFilter').value;
    const sourceFilter = document.getElementById('sourceFilter').value;
    const dateFrom = document.getElementById('dateFrom').value;
    const dateTo = document.getElementById('dateTo').value;

    let filtered = articles;

    try{
        const response = await fetch(
            `/api/get-articles?` + 
            `id=${currentUser.id}` +
            `&titular=${titleFilter}` +
            `&clave=${keywordFilter}` + 
            `&tema=${categoryFilter}` +
            `&fuente=${sourceFilter}` +
            `&fecha1=${dateFrom}` +
            `&fecha2=${dateTo}`,
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            }
        );
        let articleDetail = await response.json();
        articleDetail.forEach(a => a.Article.isDiscarded = false);
        articleDetail.forEach(a => a.Article.isNew = false);
        articleDetail.forEach(a => a.Article.Tema = a.Article.Tema.split(/[,]+/).filter(Boolean))
        filtered = articleDetail;
    }
    catch(error){
        console.error('error filtrando notificaciones', error)
    }

    renderArticles(filtered);
    showAlert(`${filtered.length} artículos encontrados`, 'info');
}


function clearFilters() {
    document.getElementById('titleFilter').value = '';
    document.getElementById('keywordFilter').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('sourceFilter').value = '';
    document.getElementById('dateFrom').value = '';
    document.getElementById('dateTo').value = '';
    renderArticles();
    showAlert('Filtros limpiados', 'info');
}

//done
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
    showAlert('Fuente agregada exitosamente', 'success');
}

//done
function toggleSource(sourceId) {
    const source = sources.find(s => s.id === sourceId);
    if (source) {
        source.active = !source.active;
        renderSources();
        updateStats();
        showAlert(`Fuente ${source.active ? 'activada' : 'desactivada'}`, 'info');
    }
}
//done
function removeSource(sourceId) {
    const index = sources.findIndex(s => s.id === sourceId);
    if (index !== -1) {
        sources.splice(index, 1);
        renderSources();
        populateSourceFilters();
        updateStats();
        showAlert('Fuente eliminada', 'info');
    }
}

//done
async function startScraping() {
    const button = document.getElementById('startScraping');
    const status = document.getElementById('scrapingStatus');
    
    button.disabled = true;
    button.textContent = '🔄 Scraping en progreso...';
    status.textContent = 'Extrayendo artículos...';
    
    // Create notification for scraping start
    showAlert('Scraping iniciado, Proceso de extracción de artículos en curso...', 'info');
    let isSuccess = true;
    
    let responseObj = Object();
    try {
        const urls = sources.map(obj => obj.url)
        console.log(JSON.stringify(urls));
        const response = await fetch(`/api/start-scraping?id=${currentUser.id}`, {
            method: "POST",
			headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(urls)
        });
        responseObj = await response.json();
        console.log(JSON.stringify(responseObj));
        renderScrapingHistory();
    }
    catch(error){
        console.error("error al iniciar scraping",error);
        isSuccess = false;
    }
    
    // Simulate scraping process
    setTimeout(() => {
        
        if (isSuccess) {
            // Add new articles to simulate scraping results
            const newArticleIds = addNewScrapedArticles(responseObj.articleList, responseObj.sourceList);
            
            status.textContent = 'Scraping completado. Revisa los nuevos artículos.';
            
            // Update displays after scraping
            renderArticles();
            renderNotifications();
            updateStats();
            
            button.disabled = false;
            button.textContent = '🚀 Iniciar Scraping';
            status.textContent = 'Listo para iniciar';
            
            // Show review modal for new articles
            showArticleReviewModal(newArticleIds);
            
            // Create completion notification
            showAlert(`Scraping completado ${newArticleIds.length} nuevos artículos encontrados. Revísalos para decidir cuáles conservar.`,
                'success');

        } else {
            renderScrapingHistory();
            renderNotifications();
            updateStats();
            
            button.disabled = false;
            button.textContent = '🚀 Iniciar Scraping';
            status.textContent = 'Listo para iniciar';
            
            showAlert('Error en scraping, No se pudieron extraer artículos de las fuentes', 'error');
        }
    }, 3000);
}

// done
function addNewScrapedArticles(articleList, sourceList) {
    const newArticleIds = [];
    for (let i = 0; i < articleList.length; i++) {

        let article = articleList[i];
        article.Tema = article.Tema.split(/[,]+/).filter(Boolean);

        let source = sourceList[i];
        
        const newArticle = {
            Article: article,
            Source: source,
            isNew: true,
            isDiscarded: false
        };
        
        articles.push(newArticle);
        newArticleIds.push(newArticle.Article.Id);
    }
    
    return newArticleIds;
}

//done
function showArticleReviewModal(newArticleIds) {
    const modal = document.createElement('div');
    modal.id = 'articleReviewModal';
    modal.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50';
    
    const newArticles = articles.filter(a => newArticleIds.includes(a.Article.Id));
    
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
                ${newArticles.map(a => `
                    <div class="border border-gray-200 rounded-lg p-4" data-article-id="${a.Article.Id}">
                        <div class="flex justify-between items-start">
                            <div class="flex-1">
                                <h4 class="font-medium text-gray-900 mb-2">${a.Article.Titular}</h4>
                                <p class="text-gray-600 text-sm mb-3">${a.Article.Cuerpo.substring(0, 200)}...</p>
                                <div class="flex items-center space-x-4 text-sm text-gray-500 mb-3">
                                    <span class="bg-blue-100 text-blue-800 px-2 py-1 rounded-full">${a.Source.Url}</span>
                                    <span class="bg-green-100 text-green-800 px-2 py-1 rounded-full">${a.Source.Tipo}</span>
                                    <span>${a.Article.Fecha}</span>
                                </div>
                                <div class="flex flex-wrap gap-1">
                                    ${a.Article.Tema.map(keyword => `<span class="bg-gray-100 text-gray-700 px-2 py-1 rounded text-xs">${keyword}</span>`).join('')}
                                </div>
                            </div>
                            <div class="flex flex-col space-y-2 ml-4">
                                <button onclick="keepArticleFromReview(${a.Article.Id})" class="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors text-sm">
                                    ✅ Conservar
                                </button>
                                <button onclick="discardArticleFromReview(${a.Article.Id})" class="bg-red-600 text-white px-4 py-2 rounded-lg hover:bg-red-700 transition-colors text-sm">
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
    keepAllFromReview(newArticleIds);
}

//done
function keepArticleFromReview(articleId) {
    const a = articles.find(a => a.Article.Id === articleId);
    if (a) {
        a.isDiscarded = false;
        const articleElement = document.querySelector(`[data-article-id="${articleId}"]`);
        if (articleElement) {
            articleElement.style.backgroundColor = '#f0fdf4';
            articleElement.style.borderColor = '#22c55e';
            articleElement.style.opacity = '1';
        }
        showAlert('Artículo conservado', 'success');
    }
}

//done
function discardArticleFromReview(articleId) {
    const a = articles.find(a => a.Article.Id === articleId);
    if (a) {
        a.isDiscarded = true;
        const articleElement = document.querySelector(`[data-article-id="${articleId}"]`);
        if (articleElement) {
            articleElement.style.backgroundColor = '#fef2f2';
            articleElement.style.borderColor = '#ef4444';
            articleElement.style.opacity = '0.6';
        }
        showAlert('Artículo descartado', 'info');
    }
}

//done
function keepAllFromReview(articleIds) {
    articleIds.forEach(id => {
        keepArticleFromReview(id)
    });
    showAlert('Todos los artículos conservados', 'success');
}

//done
function discardAllFromReview(articleIds) {
    articleIds.forEach(id => {
        discardArticleFromReview(id)
    });
    showAlert('Todos los artículos descartados', 'info');
}

//done
async function closeArticleReviewModal() {
    const modal = document.getElementById('articleReviewModal');
    if (modal) {
        modal.remove();
        renderArticles();
        
        // const keptCount = articles.filter(a => !a.isDiscarded && a.isNew).length - articles.filter(a => !a.isDiscarded && a.isNew && !a.Article.IdResultado).length;
        // const discardedCount = articles.filter(a => a.isDiscarded && a.Article.IdResultado === scrapingHistory[0]?.id).length;

		const latestScrapingResultId = scrapingHistory.length > 0 ? scrapingHistory[scrapingHistory.length - 1].Id : null;
        
        // Count articles from the latest scraping session that were kept vs discarded
        const sessionArticles = articles.filter(a => a.Article.IdResultado === latestScrapingResultId);
        const keptCount = sessionArticles.filter(a => !a.isDiscarded).length;
        const discardedCount = sessionArticles.filter(a => a.isDiscarded).length;
        articles.forEach(a => a.isNew = false);
        
        const discardedIds = sessionArticles.filter(a => a.isDiscarded).map(a => a.Article.Id);
        
        try{
            response = await fetch(`/api/discard-articles?id=${currentUser.id}`,
                {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(discardedIds)
                }
            );
        }
        catch(error){
            console.error('error descartando articulos', error);
        }
        
        console.log(discardedIds);
        
        renderArticles();
        showAlert(`Revisión completada
            Artículos procesados: ${keptCount} conservados, ${discardedCount} descartados`, 
            'info');
    }
}


//done
async function clearOldLogs() {
    const oldLogsCount = scrapingHistory.length;
    try{
        response = await fetch(`/api/delete-results?id=${currentUser.id}`,
            {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            }
        )
    }
    catch(error){
        console.error('error al eliminar registros', error)
    }
    renderScrapingHistory();
    renderArticles();
    renderNotifications();
    
    const removedLogs = oldLogsCount - scrapingHistory.length;
    if (removedLogs > 0) {
        showAlert(`${removedLogs} logs antiguos eliminados`, 'info');
    }
}

//done
function showEditProfileModal() {
    document.getElementById('nombres').value = currentUser.nombres;
    document.getElementById('apellidos').value = currentUser.apellidos;
    document.getElementById('email').value = currentUser.correo;
    document.getElementById('editProfileModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

//done
function hideEditProfileModal() {
    document.getElementById('editProfileModal').classList.add('hidden');
}

//done
async function saveProfile(e) {
    e.preventDefault();
    const newFirstName = document.getElementById('nombres').value;
    const newLastName = document.getElementById('apellidos').value;
    const newEmail = document.getElementById('editEmail').value;

    if(currentUser.nombres != newFirstName || currentUser.apellidos != newLastName){
        try{
            response = await fetch(`/api/edit-user?id=${currentUser.id}`,
                {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        Id : -1,
                        Nombres: newFirstName,
                        Apellidos : newLastName,
                        Correo : "",
                        Contraseña : ""
                    })
                }
            );
            const success = await response.json();
            if(success){
                currentUser.firstName = newFirstName;
                currentUser.lastName = newLastName;
                showAlert('Nombres y apellidos actualizados exitosamente', 'success');
                document.getElementById('currentUser').textContent = `${currentUser.firstName} ${currentUser.lastName}`;
            }
        }
        catch(error){
            console.error("error al cambiar nombre y apellido", error);
        }
    }

	if(newEmail != currentUser.correo){
		try{
			response = await fetch(`/api/edit-email?id=${currentUser.id}`,
				{
					method: 'PATCH',
					headers: {
						'Content-Type': 'application/json',
					},
					body: JSON.stringify(newEmail)
				}
			);
			const success = await response.json();
			if(success){
				currentUser.correo = newEmail;
				showAlert('Correo actualizado exitosamente', 'success');
			}
            else{
				showAlert('Este correo ya existe, escoge otro...', 'success');
            }
		}
		catch(error){
			console.error("error al cambiar correo", error);
		}
	}
    
    
    hideEditProfileModal();
}

//done
function showArticleDetail(articleId) {
    const a = articles.find(a => a.Article.Id == articleId);
    if (!a) return;
    
    document.getElementById('articleDetailTitle').textContent = a.Article.Titular;
    document.getElementById('articleDetailContent').innerHTML = `
        <p class="text-gray-700 leading-relaxed mb-4">${a.Article.Cuerpo}</p>
        <div class="bg-gray-50 p-4 rounded-lg">
            <h4 class="font-medium text-gray-900 mb-2">Palabras clave:</h4>
            <div class="flex flex-wrap gap-2">
                ${a.Article.Tema.map(keyword => `<span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full text-sm">${keyword}</span>`).join('')}
            </div>
        </div>
    `;
    
    document.getElementById('articleDetailMeta').innerHTML = `
        <span class="bg-blue-100 text-blue-800 px-3 py-1 rounded-full">${a.Source.Url}</span>
        <span class="bg-green-100 text-green-800 px-3 py-1 rounded-full">${a.Source.Tipo}</span>
        <span>${a.Article.Fecha}</span>
    `;
    
    // Store current article ID for modal actions
    document.getElementById('articleDetailModal').dataset.articleId = articleId;
    
    // Update favorite button
    const favoriteBtn = document.getElementById('toggleArticleFavorite');
    favoriteBtn.innerHTML = a.Article.Favorito ? '❤️ Quitar de Favoritos' : '🤍 Agregar a Favoritos';
    
    document.getElementById('articleDetailModal').classList.remove('hidden');
}

//done
function hideArticleDetailModal() {
    document.getElementById('articleDetailModal').classList.add('hidden');
}
// done
function openArticleLinkFromModal() {
    const articleId = document.getElementById('articleDetailModal').dataset.articleId;
    const a = articles.find(a => a.Article.Id == articleId);
    if (a && a.Source.Url) {
        window.open(a.Source.Url, '_blank', 'noopener,noreferrer');
        showAlert('Abriendo enlace del artículo', 'info');
    } else {
        showAlert('Enlace no disponible', 'error');
    }
}
// done
function openArticleLink(articleId) {
    const a = articles.find(a => a.Article.Id == articleId);
    if (a && a.Source.Url) {
        window.open(a.Source.Url, '_blank', 'noopener,noreferrer');
        showAlert('Abriendo enlace del artículo', 'info');
    } else {
        showAlert('Enlace no disponible', 'error');
    }
}

//done
function toggleArticleFavoriteFromModal() {
    const articleId = parseInt(document.getElementById('articleDetailModal').dataset.articleId);
    toggleFavorite(articleId);
    
    // Update modal button
    const a = articles.find(a => a.Article.Id === articleId);
    const favoriteBtn = document.getElementById('toggleArticleFavorite');
    favoriteBtn.innerHTML = a.Article.Favorito ? '❤️ Quitar de Favoritos' : '🤍 Agregar a Favoritos';
}

//done
async function discardArticle(articleId) {
    const a = articles.find(a => a.Article.Id === articleId);
    if (a) {
        try {
            response = await fetch(`/api/discard-articles?id=${currentUser.id}`,
                {
                    method: 'PATCH',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify([articleId])
                }
            );
        }
        catch(error){
            console.error('error al descartar articulo', error)
        }

        
        renderArticles();
        renderFavorites()
        updateStats();
        showAlert('Artículo descartado', 'info');
    }
}

//done
function removeSource(sourceId) {
    const source = sources.find(s => s.id === sourceId);
    if (source) {
        const index = sources.findIndex(s => s.id === sourceId);
        sources.splice(index, 1);
        renderSources();
        populateSourceFilters();
        renderArticles();
        updateStats();
        showAlert('Fuente eliminada', 'info');
    }
}

//done
async function deleteAccount() {
    // Simulate account deletion - remove all user data
    articles.length = 0;
    notifications.length = 0;
    scrapingHistory.length = 0;
    sources.length = 0;
    var success = false;
    try{
        const response = await fetch(`/api/delete-user?id=${currentUser.id}`,
			{
				method: 'DELETE',
				headers: {
					'Content-Type': 'application/json',
				},
		});
        success = await response.json();
    }
    catch(error){
        console.error('error eliminando usuario', error)
    }
    if (success){
        hideDeleteAccountModal();
        showAlert('Cuenta y todos los datos asociados eliminados', 'info');
        logout();
    }
}

//done
function showChangePasswordModal() {
    document.getElementById('changePasswordModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

//done
function hideChangePasswordModal() {
    document.getElementById('changePasswordModal').classList.add('hidden');
}

//done
async function changePassword(e) {
    e.preventDefault();
    // Simulate password change
    const oldPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPasswordChange').value;
    var userId = -1;
    try{
        response = await fetch('/api/login',{
            method: "POST",
			headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                Correo: currentUser.correo,
                Contraseña: oldPassword
            })
        });
        userId = await response.json();
    }
    catch(error){
        console.error('error verificando contraseña', error)
    }
    if(userId == -1 || userId == ""){
        showAlert('Contraseña actual incorrecta...', 'error');
        return;
    }
    if(oldPassword != newPassword){
        showAlert('Contraseña actualizada exitosamente', 'success');
        try{
            const response = await fetch(`/api/change-password?id=${currentUser.id}`,{
                method: "PATCH",
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(newPassword)
            });
            const success = await response.json();
            if(success){
                hideChangePasswordModal();
                return;
            }
            else{
                throw "backend did not respond properly";
            }
        }
        catch(error){
            console.error("error al cambiar contraseña", error)
        }
    }
    else{
        showAlert('Contraseña actual incorrecta...', 'error');
    }
    document.getElementById('changePasswordForm').reset();
}

//done
function showDeleteAccountModal() {
    document.getElementById('deleteAccountModal').classList.remove('hidden');
    document.getElementById('userMenu').classList.add('hidden');
}

//done
function hideDeleteAccountModal() {
    document.getElementById('deleteAccountModal').classList.add('hidden');
}

//done
function hideAllModals() {
    document.getElementById('changePasswordModal').classList.add('hidden');
    document.getElementById('deleteAccountModal').classList.add('hidden');
    document.getElementById('editProfileModal').classList.add('hidden');
    document.getElementById('articleDetailModal').classList.add('hidden');
}

//done
function showAlert(message, type = 'info') {
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