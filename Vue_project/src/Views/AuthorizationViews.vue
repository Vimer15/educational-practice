<template>
    <div class="auth-container">
        <div class="container">
            <div class="auth-card">
                <div class="header">
                    <div class="logo">
                        <i class="fas fa-shoe-prints"></i>
                        <span>ShoeShop</span>
                    </div>
                    <h1 class="title">Авторизация в систему</h1>
                    <p class="subtitle">Введите свои учетные данные</p>
                </div>
                
                <form class="auth-form" @submit.prevent="handleLogin">
                    <div class="input-group">
                        <label for="login">
                            <i class="fas fa-user icon"></i>
                            Логин
                        </label>
                        <input 
                            v-model="loginData.LoginUserImport"
                            type="text" 
                            id="login" 
                            placeholder="Введите логин"
                            required
                            :disabled="loading"
                            autocomplete="username">
                    </div>
                    
                    <div class="input-group">
                        <label for="password">
                            <i class="fas fa-key icon"></i>
                            Пароль
                        </label>
                        <input 
                            v-model="loginData.PasswordUserImport"
                            :type="showPassword ? 'text' : 'password'" 
                            id="password" 
                            placeholder="Введите пароль"
                            required
                            :disabled="loading"
                            autocomplete="current-password">
                        <div class="password-toggle" @click="togglePasswordVisibility">
                            <i :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"></i>
                        </div>
                    </div>
                    
                    <!-- Блок с демо-данными (раскомментируйте если нужно) -->
                    <!-- <div class="demo-credentials" v-if="!loading">
                        <div class="demo-title">Тестовые данные:</div>
                        <div class="demo-list">
                            <div class="demo-item">
                                <span>Пользователь:</span>
                                <code @click="fillDemo('user1')">user1 / password123</code>
                            </div>
                            <div class="demo-item">
                                <span>Администратор:</span>
                                <code @click="fillDemo('admin')">admin / admin123</code>
                            </div>
                        </div>
                    </div> -->
                    
                    <div v-if="errorMessage" class="error-message">
                        <i class="fas fa-exclamation-circle"></i>
                        {{ errorMessage }}
                    </div>
                    
                    <button type="submit" class="auth-button" :disabled="loading">
                        <i class="fas fa-sign-in-alt"></i>
                        <span v-if="!loading">Войти в систему</span>
                        <span v-else>
                            <i class="fas fa-spinner fa-spin"></i>
                            Авторизация...
                        </span>
                    </button>
                    
                    <button type="button" class="gost_auth-button" @click="loginAsGuest" :disabled="loading">
                        <i class="fas fa-user-friends"></i>
                        Войти как гость
                    </button>
                </form>
                
                <div class="footer" v-if="lastUser">
                    <div class="login-info">
                        <p>Последний вход: <strong>{{ lastUser }}</strong></p>
                    </div>
                </div>
                
                <div class="footer">
                    <p class="copyright">© 2024 ShoeShop. Все права защищены</p>
                    <p class="version">Версия 1.0.0</p>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import axios from 'axios'

export default {
    name: 'AuthorizationViews',
    data() {
        return {
            loginData: {
                LoginUserImport: '',
                PasswordUserImport: ''
            },
            loading: false,
            errorMessage: '',
            showPassword: false,
            lastUser: ''
        }
    },
    mounted() {
        const savedLogin = localStorage.getItem('lastLogin');
        if (savedLogin) {
            this.lastUser = savedLogin;
            this.loginData.LoginUserImport = savedLogin;
        }
        this.$nextTick(() => {
            const loginInput = document.getElementById('login');
            if (loginInput) {
                loginInput.focus();
            }
        });
    },
    methods: {
        async handleLogin() {
            this.loading = true;
            this.errorMessage = '';
            
            try {
                if (!this.loginData.LoginUserImport.trim()) {
                    throw new Error('Введите логин');
                }
                if (!this.loginData.PasswordUserImport.trim()) {
                    throw new Error('Введите пароль');
                }
                
                const response = await axios.post('http://localhost:5224/api/Users/Authorization', this.loginData, {
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    timeout: 10000
                });
                
                if (response.data) {
                    // Получаем данные пользователя из ответа
                    const userData = response.data;
                    
                    // Определяем роль пользователя
                    const userRole = this.determineUserRole(userData);
                    
                    // Создаем объект с полными данными пользователя
                    const userWithRole = {
                        ...userData,
                        role: userRole,
                        isAuthenticated: true,
                        loginTime: new Date().toISOString()
                    };
                    
                    // Сохраняем в localStorage
                    localStorage.setItem('user', JSON.stringify(userWithRole));
                    localStorage.setItem('lastLogin', this.loginData.LoginUserImport);
                    localStorage.setItem('authToken', response.data.token || 'demo-token-' + Date.now());
                    localStorage.setItem('userRole', userRole);
                    
                    // Сохраняем в sessionStorage
                    sessionStorage.setItem('sessionActive', 'true');
                    sessionStorage.setItem('currentUser', JSON.stringify(userWithRole));
                    
                    // Показываем успешное сообщение
                    if (this.$toast) {
                        this.$toast.success('Авторизация успешна!');
                    } else {
                        alert('Авторизация успешна!');
                    }
                    
                    // Переходим на каталог с данными пользователя
                    setTimeout(() => {
                        this.$router.push({
                            path: '/KatalogViews',
                            query: { 
                                userId: userData.id || userData.Id || userData.userId,
                                userRole: userRole,
                                userName: userData.LoginUserImport 
                            },
                            state: { 
                                userData: userWithRole 
                            }
                        });
                    }, 1000);
                }
            } catch (error) {
                console.error('Login error:', error);
                
                if (error.response) {
                    // Ошибка сервера
                    switch (error.response.status) {
                        case 400:
                            this.errorMessage = error.response.data || 'Неверный логин или пароль';
                            break;
                        case 401:
                            this.errorMessage = 'Неверные учетные данные';
                            break;
                        case 403:
                            this.errorMessage = 'Доступ запрещен';
                            break;
                        case 404:
                            this.errorMessage = 'Сервер авторизации не найден';
                            break;
                        case 500:
                            this.errorMessage = 'Ошибка на сервере. Попробуйте позже';
                            break;
                        default:
                            this.errorMessage = `Ошибка ${error.response.status}`;
                    }
                } else if (error.request) {
                    this.errorMessage = 'Нет соединения с сервером. Проверьте подключение';
                } else if (error.message.includes('timeout')) {
                    this.errorMessage = 'Время ожидания истекло. Попробуйте снова';
                } else {
                    this.errorMessage = error.message || 'Ошибка авторизации';
                }
                
                // Вибрация при ошибке (если поддерживается)
                if (navigator.vibrate) {
                    navigator.vibrate(200);
                }
            } finally {
                this.loading = false;
            }
        },
        
        // Метод для определения роли пользователя
        determineUserRole(userData) {
            // Проверяем наличие роли в ответе
            if (userData.role) return userData.role;
            if (userData.Role) return userData.Role;
            if (userData.roleId === 1) return 'admin';
            if (userData.RoleId === 1) return 'admin';
            
            // Проверяем по логину
            if (userData.LoginUserImport && userData.LoginUserImport.toLowerCase().includes('admin')) {
                return 'admin';
            }
            
            // По умолчанию
            return 'user';
        },
        
        // Метод для входа как гость
        loginAsGuest() {
            const guestData = {
                id: 'guest_' + Date.now(),
                LoginUserImport: 'Гость',
                role: 'guest',
                isGuest: true,
                isAuthenticated: false,
                loginTime: new Date().toISOString()
            };
            
            // Сохраняем данные гостя
            localStorage.setItem('user', JSON.stringify(guestData));
            localStorage.setItem('userRole', 'guest');
            sessionStorage.setItem('sessionActive', 'true');
            sessionStorage.setItem('currentUser', JSON.stringify(guestData));
            
            if (this.$toast) {
                this.$toast.info('Вход выполнен как гость');
            } else {
                alert('Вход выполнен как гость');
            }
            
            // Переходим в каталог
            setTimeout(() => {
                this.$router.push({
                    path: '/KatalogViews',
                    state: { 
                        userData: guestData 
                    }
                });
            }, 500);
        },
        
        togglePasswordVisibility() {
            this.showPassword = !this.showPassword;
        },
        
        fillDemo(login) {
            const demoCredentials = {
                'user1': { login: 'user1', password: 'password123' },
                'admin': { login: 'admin', password: 'admin123' }
            };
            
            if (demoCredentials[login]) {
                this.loginData.LoginUserImport = demoCredentials[login].login;
                this.loginData.PasswordUserImport = demoCredentials[login].password;
                this.$nextTick(() => {
                    const submitButton = document.querySelector('.auth-button');
                    if (submitButton) {
                        submitButton.focus();
                    }
                });
            }
        },
        
        handleKeyPress(event) {
            if (event.key === 'Enter' && !this.loading) {
                this.handleLogin();
            }
        }
    },
    created() {
        window.addEventListener('keypress', this.handleKeyPress);
    },
    beforeUnmount() {
        window.removeEventListener('keypress', this.handleKeyPress);
    }
}
</script>

<style scoped>
.auth-container {
    background: linear-gradient(135deg, #4ee253 0%, #2c9a00 100%);
    min-height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 20px;
    position: relative;
    overflow: hidden;
}

.auth-container::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" preserveAspectRatio="none"><path fill="rgba(255,255,255,0.05)" d="M0,0 L100,0 L100,100 Z"/></svg>');
    background-size: cover;
}

.container {
    width: 100%;
    max-width: 480px;
    display: flex;
    flex-direction: column;
    align-items: center;
    z-index: 1;
}

.auth-card {
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(10px);
    border-radius: 20px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    padding: 40px;
    width: 100%;
    border: 1px solid rgba(255, 255, 255, 0.2);
    transform: translateY(0);
    transition: transform 0.3s ease;
}

.auth-card:hover {
    transform: translateY(-5px);
}

.header {
    text-align: center;
    margin-bottom: 32px;
}

.logo {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    margin-bottom: 20px;
    font-size: 24px;
    color: #2c9a00;
    font-weight: 700;
}

.logo i {
    font-size: 28px;
    color: #2c9a00;
}

.title {
    font-size: 28px;
    color: #2c9a00;
    margin-bottom: 8px;
    font-weight: 700;
    background: linear-gradient(135deg, #2c9a00 0%, #4ee253 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.subtitle {
    font-size: 16px;
    color: #666;
    font-weight: 400;
}

.auth-form {
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.input-group {
    display: flex;
    flex-direction: column;
    gap: 8px;
    position: relative;
}

.input-group label {
    font-size: 14px;
    color: #34495e;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 8px;
}

.icon {
    color: #2c9a00;
    width: 16px;
}

.input-group input {
    padding: 14px 16px;
    border: 2px solid #e0e0e0;
    border-radius: 12px;
    font-size: 16px;
    transition: all 0.3s ease;
    outline: none;
    background: #f8f9fa;
    padding-right: 45px;
}

.input-group input:focus {
    border-color: #2c9a00;
    box-shadow: 0 0 0 4px rgba(44, 154, 0, 0.15);
    background: white;
}

.input-group input:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.input-group input::placeholder {
    color: #aaa;
}

.password-toggle {
    position: absolute;
    right: 15px;
    bottom: 14px;
    cursor: pointer;
    color: #2c9a00;
    font-size: 18px;
    transition: color 0.3s;
    z-index: 2;
}

.password-toggle:hover {
    color: #4ee253;
}

.demo-credentials {
    background: #f8f9fa;
    border-radius: 10px;
    padding: 15px;
    margin: 10px 0;
    border-left: 4px solid #2c9a00;
}

.demo-title {
    font-size: 13px;
    color: #666;
    margin-bottom: 8px;
    font-weight: 600;
}

.demo-list {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

.demo-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 13px;
}

.demo-item span {
    color: #777;
}

.demo-item code {
    background: #e0f2d0;
    color: #2c9a00;
    padding: 4px 8px;
    border-radius: 6px;
    cursor: pointer;
    transition: all 0.3s;
    font-weight: 600;
    border: 1px solid #b8e0a0;
}

.demo-item code:hover {
    background: #2c9a00;
    color: white;
    transform: translateY(-1px);
}

.error-message {
    color: #e74c3c;
    background-color: #fdf0ed;
    padding: 12px 16px;
    border-radius: 10px;
    font-size: 14px;
    text-align: center;
    border: 1px solid #fad1d1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    animation: shake 0.5s ease;
}

@keyframes shake {
    0%, 100% { transform: translateX(0); }
    25% { transform: translateX(-5px); }
    75% { transform: translateX(5px); }
}

.gost_auth-button {
    background: linear-gradient(135deg, #95a5a6 0%, #7f8c8d 100%);
    color: white;
    border: none;
    padding: 16px;
    border-radius: 12px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 10px;
    margin-top: 10px;
    position: relative;
    overflow: hidden;
}

.gost_auth-button:hover:not(:disabled) {
    background: linear-gradient(135deg, #7f8c8d 0%, #95a5a6 100%);
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(127, 140, 141, 0.4);
}

.gost_auth-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
}

.auth-button {
    background: linear-gradient(135deg, #2c9a00 0%, #4ee253 100%);
    color: white;
    border: none;
    padding: 16px;
    border-radius: 12px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 10px;
    margin-top: 10px;
    position: relative;
    overflow: hidden;
}

.auth-button::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    transition: 0.5s;
}

.auth-button:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(44, 154, 0, 0.4);
}

.auth-button:hover:not(:disabled)::before {
    left: 100%;
}

.auth-button:disabled {
    opacity: 0.7;
    cursor: not-allowed;
    transform: none !important;
}

.auth-button:active {
    transform: translateY(0);
}

.fa-spinner {
    animation: spin 1s linear infinite;
}

@keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
}

.login-info {
    text-align: center;
    margin-top: 15px;
    padding: 10px;
    background: #f0f7ff;
    border-radius: 8px;
    border: 1px solid #d1e9ff;
}

.login-info p {
    margin: 0;
    font-size: 13px;
    color: #2c3e50;
}

.login-info strong {
    color: #2c9a00;
}

.footer {
    margin-top: 30px;
    padding-top: 20px;
    border-top: 1px solid #eee;
    text-align: center;
}

.copyright {
    font-size: 12px;
    color: #95a5a6;
    margin-bottom: 5px;
}

.version {
    font-size: 11px;
    color: #bdc3c7;
}

/* Адаптивность */
@media (max-width: 520px) {
    .auth-card {
        padding: 30px 20px;
    }
    
    .title {
        font-size: 24px;
    }
}

/* Анимация появления */
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.auth-card {
    animation: fadeInUp 0.6s ease-out;
}

/* Пульсация для фокуса */
@keyframes pulse {
    0% { box-shadow: 0 0 0 0 rgba(44, 154, 0, 0.4); }
    70% { box-shadow: 0 0 0 10px rgba(44, 154, 0, 0); }
    100% { box-shadow: 0 0 0 0 rgba(44, 154, 0, 0); }
}

.input-group input:focus {
    animation: pulse 2s infinite;
}
</style>