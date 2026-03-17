<template>
  <div class="orders-container">
    <!-- Шапка -->
    <div class="header">
      <div class="user-info">
        <h1>Управление заказами</h1>
        <div class="user-details" v-if="userData">
          <div class="user-fullname">
            <i class="fas fa-user-circle"></i>
            <span class="fio">{{ getUserFullName }}</span>
          </div>
          <div class="user-role" :class="getRoleClass">
            <i :class="getRoleIcon"></i>
            <span class="role-text">роль: {{ getRoleDisplay }}</span>
          </div>
        </div>
      </div>
      
      <div class="header-actions">
        <button class="back-btn" @click="goBack">
          <i class="fas fa-arrow-left"></i>
          Назад в каталог
        </button>
        <button class="logout-btn" @click="handleLogout">
          <i class="fas fa-sign-out-alt"></i>
          Выйти
        </button>
      </div>
    </div>

    <!-- Статистика -->
    <div class="stats-section">
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-shopping-cart"></i>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.totalOrders }}</span>
          <span class="stat-label">Всего заказов</span>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon new">
          <i class="fas fa-clock"></i>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.newOrders }}</span>
          <span class="stat-label">Новых</span>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon completed">
          <i class="fas fa-check-circle"></i>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.completedOrders }}</span>
          <span class="stat-label">Завершенных</span>
        </div>
      </div>
    </div>

    <!-- Фильтры -->
    <div class="filters-section">
      <div class="filters-container">
        <div class="filter-group">
          <label>
            <i class="fas fa-filter"></i> Статус:
          </label>
          <select v-model="statusFilter" @change="applyFilters" class="filter-select">
            <option value="">Все статусы</option>
            <option value="Новый">Новые</option>
            <option value="Завершен">Завершенные</option>
          </select>
        </div>

        <div class="filter-group">
          <label>
            <i class="fas fa-calendar"></i> Период:
          </label>
          <select v-model="dateFilter" @change="applyFilters" class="filter-select">
            <option value="">За всё время</option>
            <option value="today">Сегодня</option>
            <option value="week">Эта неделя</option>
            <option value="month">Этот месяц</option>
          </select>
        </div>

        <div class="filter-group">
          <label>
            <i class="fas fa-search"></i> Поиск:
          </label>
          <input 
            type="text" 
            v-model="searchQuery" 
            @input="applyFilters"
            placeholder="Поиск по коду заказа..."
            class="search-input"
          />
        </div>

        <button class="reset-filters-btn" @click="resetFilters">
          <i class="fas fa-undo"></i> Сбросить
        </button>
      </div>

      <div class="results-info">
        Найдено заказов: {{ filteredOrders.length }} из {{ orders.length }}
      </div>
    </div>

    <!-- Состояние загрузки -->
    <div v-if="loading" class="loading-state">
      <div class="spinner">
        <i class="fas fa-spinner fa-spin"></i>
      </div>
      <p>Загрузка заказов...</p>
    </div>

    <!-- Ошибка -->
    <div v-else-if="error" class="error-state">
      <i class="fas fa-exclamation-triangle"></i>
      <p>{{ error }}</p>
      <button @click="loadOrders" class="retry-btn">
        <i class="fas fa-redo"></i>
        Попробовать снова
      </button>
    </div>

    <!-- Таблица заказов -->
    <div v-else-if="filteredOrders.length > 0" class="orders-table-container">
      <table class="orders-table">
        <thead>
          <tr>
            <th>Код заказа</th>
            <th>Статус</th>
            <th>Дата заказа</th>
            <th>Дата доставки</th>
            <th>Пункт выдачи</th>
            <th>ID клиента</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          <tr 
            v-for="order in filteredOrders" 
            :key="order.idOrderImport"
            :class="{ 'completed-order': order.statusOrderImport === 'Завершен' }"
          >
            <td>{{ order.codeOrderImport }}</td>
            <td>
              <span :class="['status-badge', order.statusOrderImport === 'Новый' ? 'status-new' : 'status-completed']">
                {{ order.statusOrderImport }}
              </span>
            </td>
            <td>{{ formatDate(order.dateOrderImport) }}</td>
            <td>{{ formatDate(order.dateOfDeliveryOrderImport) }}</td>
            <td>{{ order.idPickupPointOrderImport }}</td>
            <td>{{ order.idClientOrderImport }}</td>
            <td>
              <button 
                v-if="order.statusOrderImport === 'Новый'"
                class="edit-btn"
                @click="openEditOrderModal(order)"
              >
                <i class="fas fa-edit"></i>
                Редактировать
              </button>
              <span v-else class="disabled-text">Недоступно</span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Если заказы не найдены -->
    <div v-else class="no-results">
      <i class="fas fa-shopping-cart"></i>
      <p>Заказы не найдены</p>
      <button @click="resetFilters" class="retry-btn">
        <i class="fas fa-undo"></i> Сбросить фильтры
      </button>
    </div>

    <!-- Модальное окно редактирования заказа -->
    <div v-if="showOrderModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>Редактирование заказа</h2>
          <button class="close-btn" @click="closeModal">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <form @submit.prevent="saveOrder" class="order-form">
          <!-- Код заказа (только для чтения) -->
          <div class="form-group">
            <label>Код заказа</label>
            <input 
              type="text" 
              v-model="currentOrder.codeOrderImport"
              readonly
              class="readonly-field"
            />
          </div>

          <!-- Статус -->
          <div class="form-group">
            <label>Статус</label>
            <select v-model="currentOrder.statusOrderImport" required>
              <option value="Новый">Новый</option>
              <option value="Завершен">Завершен</option>
            </select>
          </div>

          <!-- Дата заказа -->
          <div class="form-group">
            <label>Дата заказа</label>
            <input 
              type="date" 
              v-model="currentOrder.dateOrderImport"
              :min="minDate"
              :max="maxDate"
              required
            />
          </div>

          <!-- Дата доставки -->
          <div class="form-group">
            <label>Дата доставки</label>
            <input 
              type="date" 
              v-model="currentOrder.dateOfDeliveryOrderImport"
              :min="minDeliveryDate"
              :max="maxDate"
              required
            />
            <small class="field-hint">Минимальная дата: {{ minDeliveryDate }}</small>
          </div>

          <!-- Пункт выдачи -->
          <div class="form-group">
            <label>Пункт выдачи</label>
            <select v-model="currentOrder.idPickupPointOrderImport" required>
              <option value="">Выберите пункт выдачи</option>
              <option 
                v-for="point in pickupPoints" 
                :key="point.idPickupPoint" 
                :value="point.idPickupPoint"
              >
                {{ point.fullAddress }}
              </option>
            </select>
          </div>

          <!-- ID клиента (скрыто, но должно быть) -->
          <input type="hidden" v-model="currentOrder.idClientOrderImport" />

          <!-- Кнопки -->
          <div class="form-actions">
            <button type="button" class="cancel-btn" @click="closeModal">
              Отмена
            </button>
            <button type="submit" class="save-btn" :disabled="saving">
              <i class="fas fa-spinner fa-spin" v-if="saving"></i>
              <span v-else>Сохранить</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  name: 'OrdersView',
  
  data() {
    return {
      orders: [],
      filteredOrders: [],
      pickupPoints: [],
      loading: true,
      error: null,
      userData: null,
      
      // Фильтры
      statusFilter: '',
      dateFilter: '',
      searchQuery: '',
      
      // Статистика
      stats: {
        totalOrders: 0,
        newOrders: 0,
        completedOrders: 0
      },
      
      // Модальное окно
      showOrderModal: false,
      currentOrder: {
        idOrderImport: null,
        codeOrderImport: '',
        statusOrderImport: '',
        dateOrderImport: '',
        dateOfDeliveryOrderImport: '',
        idPickupPointOrderImport: '',
        idClientOrderImport: null
      },
      saving: false,
      
      // Ограничения по датам
      minDate: '',
      maxDate: ''
    }
  },
  
  computed: {
    // Формирование ФИО пользователя
    getUserFullName() {
      if (!this.userData) return 'Гость'
      
      const surname = this.userData.surnameUserImport || this.userData.SurnameUserImport || ''
      const name = this.userData.nameUserImport || this.userData.NameUserImport || ''
      const lastname = this.userData.lastnameUserImport || this.userData.LastnameUserImport || ''
      
      const fullNameParts = []
      if (surname) fullNameParts.push(surname)
      if (name) fullNameParts.push(name)
      if (lastname) fullNameParts.push(lastname)
      
      if (fullNameParts.length > 0) {
        return fullNameParts.join(' ')
      }
      
      if (this.userData.loginUserImport || this.userData.LoginUserImport) {
        return this.userData.loginUserImport || this.userData.LoginUserImport
      }
      
      return 'Пользователь'
    },
    
    // Получение роли пользователя
    getUserRole() {
      if (!this.userData) return 'guest'
      
      const role = this.userData.roleUserImport || 
                   this.userData.RoleUserImport || 
                   this.userData.role || 
                   this.userData.Role || 
                   'user'
      
      return role.toLowerCase()
    },
    
    // Отображение роли
    getRoleDisplay() {
      const role = this.getUserRole
      
      if (role === 'admin' || role === 'администратор') return 'Администратор'
      if (role === 'user' || role === 'пользователь') return 'Пользователь'
      if (role === 'guest' || role === 'гость') return 'Гость'
      
      return role
    },
    
    // Класс для стилизации роли
    getRoleClass() {
      const role = this.getUserRole
      
      if (role === 'admin' || role === 'администратор') return 'admin'
      if (role === 'user' || role === 'пользователь') return 'user'
      return 'guest'
    },
    
    // Иконка для роли
    getRoleIcon() {
      const role = this.getUserRole
      
      if (role === 'admin' || role === 'администратор') return 'fas fa-crown'
      if (role === 'user' || role === 'пользователь') return 'fas fa-user'
      return 'fas fa-user-friends'
    },
    
    // Минимальная дата для доставки (сегодня)
    minDeliveryDate() {
      return this.minDate
    }
  },
  
  async created() {
    this.getUserData()
    this.setDateLimits()
    await this.loadPickupPoints()
    await this.loadOrders()
  },
  
  methods: {
    // Получение данных пользователя
    getUserData() {
      console.log('Получение данных пользователя...')
      
      if (history.state && history.state.userData) {
        this.userData = history.state.userData
        console.log('Данные получены из history.state:', this.userData)
        return
      }
      
      const storedUser = localStorage.getItem('user')
      if (storedUser) {
        try {
          this.userData = JSON.parse(storedUser)
          console.log('Данные получены из localStorage:', this.userData)
          return
        } catch (e) {
          console.error('Ошибка парсинга localStorage:', e)
        }
      }
      
      const sessionUser = sessionStorage.getItem('currentUser')
      if (sessionUser) {
        try {
          this.userData = JSON.parse(sessionUser)
          console.log('Данные получены из sessionStorage:', this.userData)
          return
        } catch (e) {
          console.error('Ошибка парсинга sessionStorage:', e)
        }
      }
      
      this.$router.push('/')
    },
    
    // Установка ограничений по датам
    setDateLimits() {
      const today = new Date()
      const nextYear = new Date()
      nextYear.setFullYear(today.getFullYear() + 1)
      
      this.minDate = this.formatDateForInput(today)
      this.maxDate = this.formatDateForInput(nextYear)
    },
    
    // Форматирование даты для input type="date"
    formatDateForInput(date) {
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      return `${year}-${month}-${day}`
    },
    
    // Загрузка пунктов выдачи
    async loadPickupPoints() {
      try {
        // Здесь нужно будет создать API для получения пунктов выдачи
        // Пока заглушка
        this.pickupPoints = [
          { idPickupPoint: 1, fullAddress: 'г. Москва, ул. Ленина, д. 1, индекс: 123456' },
          { idPickupPoint: 2, fullAddress: 'г. Санкт-Петербург, Невский пр., д. 10, индекс: 654321' }
        ]
      } catch (error) {
        console.error('Ошибка загрузки пунктов выдачи:', error)
      }
    },
    
    // Загрузка заказов
    async loadOrders() {
      this.loading = true
      this.error = null
      
      try {
        const response = await axios.get('http://localhost:5224/api/Order')
        this.orders = response.data
        this.calculateStats()
        this.applyFilters()
        console.log('Загружено заказов:', this.orders.length)
        
      } catch (error) {
        console.error('Ошибка загрузки заказов:', error)
        this.error = 'Не удалось загрузить заказы. Проверьте подключение к серверу.'
        this.orders = []
        this.filteredOrders = []
      } finally {
        this.loading = false
      }
    },
    
    // Подсчет статистики
    calculateStats() {
      this.stats.totalOrders = this.orders.length
      this.stats.newOrders = this.orders.filter(o => o.statusOrderImport === 'Новый').length
      this.stats.completedOrders = this.orders.filter(o => o.statusOrderImport === 'Завершен').length
    },
    
    // Применение фильтров
    applyFilters() {
      let result = [...this.orders]
      
      // Фильтр по статусу
      if (this.statusFilter) {
        result = result.filter(order => order.statusOrderImport === this.statusFilter)
      }
      
      // Фильтр по дате
      if (this.dateFilter) {
        const today = new Date()
        today.setHours(0, 0, 0, 0)
        
        const weekAgo = new Date(today)
        weekAgo.setDate(weekAgo.getDate() - 7)
        
        const monthAgo = new Date(today)
        monthAgo.setMonth(monthAgo.getMonth() - 1)
        
        result = result.filter(order => {
          const orderDate = new Date(order.dateOrderImport)
          
          switch(this.dateFilter) {
            case 'today':
              return orderDate >= today
            case 'week':
              return orderDate >= weekAgo
            case 'month':
              return orderDate >= monthAgo
            default:
              return true
          }
        })
      }
      
      // Поиск по коду заказа
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase()
        result = result.filter(order => 
          order.codeOrderImport.toLowerCase().includes(query)
        )
      }
      
      this.filteredOrders = result
    },
    
    // Сброс фильтров
    resetFilters() {
      this.statusFilter = ''
      this.dateFilter = ''
      this.searchQuery = ''
      this.filteredOrders = [...this.orders]
    },
    
    // Открыть модалку редактирования
    openEditOrderModal(order) {
      this.currentOrder = {
        idOrderImport: order.idOrderImport,
        codeOrderImport: order.codeOrderImport,
        statusOrderImport: order.statusOrderImport,
        dateOrderImport: this.formatDateForInput(new Date(order.dateOrderImport)),
        dateOfDeliveryOrderImport: this.formatDateForInput(new Date(order.dateOfDeliveryOrderImport)),
        idPickupPointOrderImport: order.idPickupPointOrderImport,
        idClientOrderImport: order.idClientOrderImport
      }
      this.showOrderModal = true
    },
    
    // Закрыть модалку
    closeModal() {
      this.showOrderModal = false
      this.currentOrder = {
        idOrderImport: null,
        codeOrderImport: '',
        statusOrderImport: '',
        dateOrderImport: '',
        dateOfDeliveryOrderImport: '',
        idPickupPointOrderImport: '',
        idClientOrderImport: null
      }
    },
    
    // Сохранить изменения
    async saveOrder() {
      this.saving = true
      
      try {
        const updateData = {
          statusOrderImport: this.currentOrder.statusOrderImport,
          dateOrderImport: new Date(this.currentOrder.dateOrderImport).toISOString(),
          dateOfDeliveryOrderImport: new Date(this.currentOrder.dateOfDeliveryOrderImport).toISOString(),
          idPickupPointOrderImport: parseInt(this.currentOrder.idPickupPointOrderImport)
        }
        
        await axios.put(
          `http://localhost:5224/api/Order/${this.currentOrder.idOrderImport}`, 
          updateData
        )
        
        this.showNotification('Заказ успешно обновлен', 'success')
        await this.loadOrders()
        this.closeModal()
        
      } catch (error) {
        console.error('Ошибка сохранения заказа:', error)
        this.showNotification(
          error.response?.data || 'Ошибка при сохранении заказа',
          'error'
        )
      } finally {
        this.saving = false
      }
    },
    
    // Форматирование даты для отображения
    formatDate(dateString) {
      if (!dateString) return ''
      const date = new Date(dateString)
      return date.toLocaleDateString('ru-RU')
    },
    
    // Показать уведомление
    showNotification(message, type = 'info') {
      alert(message)
    },
    
    // Навигация
    goBack() {
      this.$router.push('/katalog')
    },
    
    handleLogout() {
      localStorage.removeItem('user')
      localStorage.removeItem('authToken')
      localStorage.removeItem('userRole')
      localStorage.removeItem('lastLogin')
      sessionStorage.removeItem('currentUser')
      sessionStorage.removeItem('sessionActive')
      
      this.$router.push('/')
    }
  }
}
</script>

<style scoped>
.orders-container {
  padding: 10px;
  max-width: 100%;
  margin: 0px;
  font-family: Arial, sans-serif;
  background-color: #f5f5f5;
  min-height: 100vh;
}

/* Шапка */
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  padding: 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.user-info h1 {
  color: #2c3e50;
  margin: 0;
  font-size: 32px;
}

.user-details {
  display: flex;
  align-items: center;
  gap: 20px;
  flex-wrap: wrap;
}

.user-fullname {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 15px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 20px;
  font-size: 16px;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
}

.user-fullname i {
  font-size: 18px;
}

.user-fullname .fio {
  font-weight: 600;
}

.user-role {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 15px;
  border-radius: 20px;
  font-size: 16px;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.user-role .role-text {
  font-weight: normal;
}

.user-role i {
  font-size: 16px;
}

.user-role.admin {
  background: linear-gradient(135deg, #ff4757 0%, #ff6b81 100%);
  color: white;
}

.user-role.user {
  background: linear-gradient(135deg, #2c9a00 0%, #4ee253 100%);
  color: white;
}

.user-role.guest {
  background: linear-gradient(135deg, #95a5a6 0%, #7f8c8d 100%);
  color: white;
}

.header-actions {
  display: flex;
  gap: 15px;
  align-items: center;
}

.back-btn {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  padding: 12px 25px;
  border-radius: 20px;
  font-size: 18px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 10px;
  box-shadow: 0 2px 5px rgba(102, 126, 234, 0.3);
}

.back-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(102, 126, 234, 0.4);
  background: linear-gradient(135deg, #764ba2, #667eea);
}

.logout-btn {
  background: linear-gradient(135deg, #ff4444, #cc0000);
  color: white;
  border: none;
  padding: 12px 25px;
  border-radius: 20px;
  font-size: 18px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 10px;
  box-shadow: 0 2px 5px rgba(204, 0, 0, 0.3);
}

.logout-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(204, 0, 0, 0.4);
  background: linear-gradient(135deg, #ff5555, #dd0000);
}

/* Статистика */
.stats-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 15px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  transition: transform 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: linear-gradient(135deg, #667eea, #764ba2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 24px;
}

.stat-icon.new {
  background: linear-gradient(135deg, #28a745, #218838);
}

.stat-icon.completed {
  background: linear-gradient(135deg, #ff4757, #ff6b81);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: 32px;
  font-weight: bold;
  color: #2c3e50;
}

.stat-label {
  font-size: 14px;
  color: #7f8c8d;
}

/* Фильтры */
.filters-section {
  background: white;
  padding: 20px;
  border-radius: 12px;
  margin-bottom: 30px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}

.filters-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 15px;
  margin-bottom: 15px;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-group label {
  font-weight: 600;
  color: #2c3e50;
  font-size: 14px;
  display: flex;
  align-items: center;
  gap: 5px;
}

.filter-group label i {
  color: #667eea;
}

.filter-select {
  padding: 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  color: #2c3e50;
  background-color: white;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-select:hover {
  border-color: #667eea;
}

.filter-select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.search-input {
  padding: 12px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  color: #2c3e50;
  background-color: white;
  transition: all 0.3s ease;
  width: 100%;
}

.search-input:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.reset-filters-btn {
  padding: 12px 20px;
  background: linear-gradient(135deg, #95a5a6, #7f8c8d);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: fit-content;
  align-self: flex-end;
  margin-top: 24px;
}

.reset-filters-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.2);
  background: linear-gradient(135deg, #7f8c8d, #95a5a6);
}

.results-info {
  color: #7f8c8d;
  font-size: 14px;
  font-weight: 600;
  padding: 15px 0 5px;
  border-top: 1px solid #e0e0e0;
}

/* Таблица заказов */
.orders-table-container {
  background: white;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  overflow-x: auto;
}

.orders-table {
  width: 100%;
  border-collapse: collapse;
}

.orders-table th {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  padding: 12px;
  text-align: left;
  font-weight: 600;
}

.orders-table th:first-child {
  border-radius: 8px 0 0 0;
}

.orders-table th:last-child {
  border-radius: 0 8px 0 0;
}

.orders-table td {
  padding: 12px;
  border-bottom: 1px solid #e0e0e0;
  color: #2c3e50;
}

.orders-table tbody tr:hover {
  background-color: #f8f9fa;
}

.orders-table tbody tr.completed-order {
  background-color: #f5f5f5;
  opacity: 0.8;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
}

.status-new {
  background: #28a745;
  color: white;
}

.status-completed {
  background: #6c757d;
  color: white;
}

.edit-btn {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 5px;
}

.edit-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 2px 5px rgba(102, 126, 234, 0.3);
}

.disabled-text {
  color: #95a5a6;
  font-size: 12px;
  font-style: italic;
}

/* Состояния загрузки */
.loading-state, .error-state, .no-results {
  text-align: center;
  padding: 50px;
  border-radius: 10px;
  margin: 20px 0;
  background: white;
}

.loading-state {
  background-color: #f8f9fa;
  color: #6c757d;
}

.error-state {
  background-color: #ffeaea;
  color: #e74c3c;
  border: 1px solid #fad1d1;
}

.no-results {
  background-color: #f8f9fa;
  color: #7f8c8d;
}

.no-results i {
  font-size: 48px;
  color: #95a5a6;
  margin-bottom: 15px;
}

.spinner {
  font-size: 40px;
  margin-bottom: 20px;
}

.retry-btn {
  margin-top: 20px;
  padding: 10px 20px;
  background-color: #28a745;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 16px;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.retry-btn:hover {
  background-color: #218838;
}

/* Модальное окно */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  animation: fadeIn 0.3s ease;
}

.modal-content {
  background: white;
  border-radius: 15px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  animation: slideIn 0.3s ease;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 30px;
  border-bottom: 1px solid #e0e0e0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-radius: 15px 15px 0 0;
}

.modal-header h2 {
  margin: 0;
  font-size: 24px;
}

.close-btn {
  background: none;
  border: none;
  color: white;
  font-size: 24px;
  cursor: pointer;
  padding: 5px;
  transition: transform 0.3s ease;
}

.close-btn:hover {
  transform: scale(1.1);
}

.order-form {
  padding: 30px;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 5px;
  font-size: 14px;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 10px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.readonly-field {
  background-color: #f8f9fa;
  cursor: not-allowed;
  color: #6c757d;
}

.field-hint {
  display: block;
  color: #6c757d;
  font-size: 12px;
  margin-top: 5px;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #e0e0e0;
}

.cancel-btn {
  padding: 12px 30px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.cancel-btn:hover {
  background: #5a6268;
  transform: translateY(-2px);
}

.save-btn {
  padding: 12px 30px;
  background: linear-gradient(135deg, #28a745, #218838);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 10px;
}

.save-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(40, 167, 69, 0.3);
}

.save-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Анимации */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideIn {
  from {
    transform: translateY(-50px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

/* Адаптивность */
@media (max-width: 768px) {
  .header {
    flex-direction: column;
    gap: 15px;
    text-align: center;
  }
  
  .user-details {
    justify-content: center;
  }
  
  .filters-container {
    grid-template-columns: 1fr;
  }
  
  .reset-filters-btn {
    margin-top: 0;
  }
  
  .header h1 {
    font-size: 24px;
  }
  
  .stats-section {
    grid-template-columns: 1fr;
  }
  
  .modal-content {
    width: 95%;
    margin: 10px;
  }
  
  .order-form {
    padding: 20px;
  }
}
</style>