<template>
  <div class="catalog-container">
    <!-- Шапка с информацией о пользователе и кнопкой выхода -->
    <div class="header">
      <div class="user-info">
        <h1>Каталог товаров</h1>
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
        <!-- Кнопка перехода к заказам для админа -->
        <button 
          v-if="isAdmin" 
          class="orders-btn" 
          @click="goToOrders"
        >
          <i class="fas fa-shopping-cart"></i>
          Заказы
        </button>
        
        <!-- Кнопка добавления товара для админа -->
        <button 
          v-if="isAdmin" 
          class="add-item-btn" 
          @click="openAddItemModal"
        >
          <i class="fas fa-plus"></i>
          Добавить товар
        </button>
        
        <button class="logout-btn" @click="handleLogout">
          <i class="fas fa-sign-out-alt"></i>
          Выйти
        </button>
      </div>
    </div>

    <!-- Фильтры и сортировка -->
    <div class="filters-section">
      <div class="filters-container">
        <!-- Поиск по названию/описанию -->
        <div class="filter-group search-group">
          <label for="searchInput">
            <i class="fas fa-search"></i> Поиск товара:
          </label>
          <input 
            type="text" 
            id="searchInput"
            v-model="searchQuery" 
            @input="applyFilters"
            placeholder="Введите название или описание товара..."
            class="search-input"
          />
        </div>

        <!-- Фильтр по категориям -->
        <div class="filter-group">
          <label for="categoryFilter">
            <i class="fas fa-tags"></i> Категория:
          </label>
          <select 
            id="categoryFilter" 
            v-model="selectedCategory" 
            @change="applyFilters"
            class="filter-select"
          >
            <option value="">Все категории</option>
            <option value="Женская обувь">Женская обувь</option>
            <option value="Мужская обувь">Мужская обувь</option>
          </select>
        </div>

        <!-- Фильтр по поставщикам -->
        <div class="filter-group">
          <label for="supplierFilter">
            <i class="fas fa-truck"></i> Поставщик:
          </label>
          <select 
            id="supplierFilter" 
            v-model="selectedSupplier" 
            @change="applyFilters"
            class="filter-select"
          >
            <option value="">Все поставщики</option>
            <option value="Kari">Kari</option>
            <option value="Обувь для вас">Обувь для вас</option>
          </select>
        </div>

        <!-- Сортировка по количеству -->
        <div class="filter-group">
          <label for="quantitySort">
            <i class="fas fa-boxes"></i> Сортировка по количеству:
          </label>
          <select 
            id="quantitySort" 
            v-model="quantitySort" 
            @change="applyFilters"
            class="filter-select"
          >
            <option value="">Без сортировки</option>
            <option value="asc">По возрастанию (сначала мало)</option>
            <option value="desc">По убыванию (сначала много)</option>
          </select>
        </div>

        <!-- Сортировка по цене -->
        <div class="filter-group">
          <label for="priceSort">
            <i class="fas fa-ruble-sign"></i> Сортировка по цене:
          </label>
          <select 
            id="priceSort" 
            v-model="priceSort" 
            @change="applyFilters"
            class="filter-select"
          >
            <option value="">Без сортировки</option>
            <option value="asc">По возрастанию (сначала дешевле)</option>
            <option value="desc">По убыванию (сначала дороже)</option>
          </select>
        </div>

        <!-- Сортировка по дате добавления -->
        <div class="filter-group">
          <label for="dateSort">
            <i class="fas fa-calendar-alt"></i> Сортировка по дате:
          </label>
          <select 
            id="dateSort" 
            v-model="dateSort" 
            @change="applyFilters"
            class="filter-select"
          >
            <option value="">Без сортировки</option>
            <option value="desc">Сначала новые</option>
            <option value="asc">Сначала старые</option>
          </select>
        </div>

        <!-- Кнопка сброса фильтров -->
        <button class="reset-filters-btn" @click="resetFilters">
          <i class="fas fa-undo"></i> Сбросить все фильтры
        </button>
      </div>

      <!-- Информация о количестве товаров -->
      <div class="results-info" v-if="!loading">
        <i class="fas fa-box"></i>
        Найдено товаров: {{ filteredItems.length }} из {{ items.length }}
        <span v-if="searchQuery" class="search-info">
          (поиск: "{{ searchQuery }}")
        </span>
      </div>
    </div>

    <!-- Состояние загрузки -->
    <div v-if="loading" class="loading-state">
      <div class="spinner">
        <i class="fas fa-spinner fa-spin"></i>
      </div>
      <p>Загрузка товаров...</p>
    </div>

    <!-- Ошибка -->
    <div v-else-if="error" class="error-state">
      <i class="fas fa-exclamation-triangle"></i>
      <p>{{ error }}</p>
      <button @click="loadItems" class="retry-btn">
        <i class="fas fa-redo"></i>
        Попробовать снова
      </button>
    </div>

    <!-- Карточки товаров -->
    <div v-else-if="filteredItems.length > 0" class="catalog-page">
      <div 
        v-for="item in filteredItems" 
        :key="item.articuleItem" 
        class="card"
        :class="{ 'admin-card': isAdmin, 'editable': isAdmin }"
        @click="isAdmin && openEditItemModal(item)"
        >
        
        <!-- Изображение товара -->
        <div class="card_image">
          <img 
            v-if="hasPhoto(item.photoItem)" 
            :src="getImageUrl(item.photoItem)" 
            :alt="item.descriptionItem"
            class="product-image"
            @error="handleImageError"
          />
          <img 
            v-else 
            src="../assets/image/default-product.jpg" 
            alt="Заглушка"
            class="product-image placeholder"
          />
        </div>

        <!-- Информация о товаре -->
        <div class="text_block">
          <p class="category-name">{{ item.categoryName }}</p>
          <p class="description">{{ item.descriptionItem }}</p>
          <p class="manufacture">{{ item.manufacture }}</p>
          <p class="supplier">{{ item.supplier }}</p>
          
          <!-- Цена -->
          <div class="price-block">
            <span 
              v-if="item.hasDiscount" 
              class="original-price"
              :style="{ color: item.priceColor, textDecoration: item.priceStyle }"
            >
              {{ formatPrice(item.originalPrice) }} ₽
            </span>
            <span class="current-price">
              {{ formatPrice(item.priceWithDiscount) }} ₽
            </span>
          </div>
          
          <p class="measurement">{{ item.measurement }}</p>
          
          <p
            class="stock" 
            :class="{ zero_count: item.countInStorageItem == 0 }"
          >
            {{ item.countInStorage }} 
          </p>

          <!-- Дата добавления -->
          <div class="date-info" v-if="item.addedDate">
            <i class="fas fa-clock"></i>
            <span>{{ formatDate(item.addedDate) }}</span>
          </div>
        </div>

        <!-- Блок скидки -->
        <div 
          v-if="item.hasDiscount" 
          class="card_sale"
          :style="{ backgroundColor: item.discountColor !== 'none' ? item.discountColor : 'coral' }"
        >
          <h1>{{ item.discountItem }}%</h1>
        </div>

        <!-- Индикатор редактирования для админа -->
        <div v-if="isAdmin" class="edit-overlay">
          <i class="fas fa-pen"></i>
          <span>Редактировать</span>
        </div>

        <!-- Кнопка удаления для админа -->
        <button 
          v-if="isAdmin" 
          class="delete-btn" 
          @click.stop="confirmDelete(item)"
        >
          <i class="fas fa-trash"></i>
        </button>

        <!-- Бейдж "Новинка" для товаров добавленных за последние 7 дней -->
        <div v-if="isNewItem(item)" class="new-badge">
          <i class="fas fa-star"></i>
          Новинка
        </div>
      </div>
    </div>

    <!-- Если товары не найдены -->
    <div v-else class="no-results">
      <i class="fas fa-search"></i>
      <p>Товары не найдены</p>
      <p v-if="searchQuery" class="no-results-hint">
        По запросу "{{ searchQuery }}" ничего не найдено
      </p>
      <button @click="resetFilters" class="retry-btn">
        <i class="fas fa-undo"></i> Сбросить фильтры
      </button>
    </div>

    <!-- Модальное окно для добавления/редактирования товара -->
    <div v-if="showItemModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-header">
          <h2>{{ modalMode === 'add' ? 'Добавление нового товара' : 'Редактирование товара' }}</h2>
          <button class="close-btn" @click="closeModal">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <form @submit.prevent="saveItem" class="item-form">
          <!-- Артикул -->
          <div class="form-group">
            <label>Артикул</label>
            <input 
              type="text" 
              v-model="currentItem.articuleItem"
              :readonly="modalMode === 'edit'"
              :class="{ 'readonly-field': modalMode === 'edit' }"
              :maxlength="MAX_ARTICULE_LENGTH"
              required
            />
            <small class="field-hint">Максимум {{ MAX_ARTICULE_LENGTH }} символов</small>
          </div>

          <!-- Описание -->
          <div class="form-group">
            <label>Описание товара</label>
            <textarea 
              v-model="currentItem.descriptionItem"
              :maxlength="MAX_DESCRIPTION_LENGTH"
              rows="3"
              required
            ></textarea>
            <small class="field-hint">Максимум {{ MAX_DESCRIPTION_LENGTH }} символов</small>
          </div>

          <!-- Единица измерения (заблокирована, всегда "шт.") -->
          <div class="form-group">
            <label>Единица измерения</label>
            <input 
              type="text" 
              v-model="currentItem.measurementItem"
              readonly
              class="readonly-field"
              value="шт."
            />
            <small class="field-hint">Значение по умолчанию: шт.</small>
          </div>

          <!-- Цена -->
          <div class="form-group">
            <label>Цена</label>
            <input 
              type="number" 
              v-model.number="currentItem.priceItem"
              min="0"
              step="0.01"
              required
            />
          </div>

          <!-- Скидка -->
          <div class="form-group">
            <label>Скидка (%)</label>
            <input 
              type="number" 
              v-model.number="currentItem.discountItem"
              min="0"
              max="99"
            />
            <small v-if="currentItem.discountItem > 15" class="warning-hint">
              Скидка больше 15%
            </small>
          </div>

          <!-- Количество на складе -->
          <div class="form-group">
            <label>Количество на складе</label>
            <input 
              type="number" 
              v-model.number="currentItem.countInStorageItem"
              min="0"
              required
            />
            <small v-if="currentItem.countInStorageItem === 0" class="warning-hint">
              Товара нет на складе
            </small>
          </div>

          <!-- Поставщик -->
          <div class="form-group">
            <label>Поставщик</label>
            <select v-model="currentItem.supplierId" required>
              <option value="">Выберите поставщика</option>
              <option value="1">Kari</option>
              <option value="2">Обувь для вас</option>
            </select>
          </div>

          <!-- Производитель -->
          <div class="form-group">
            <label>Производитель</label>
            <select v-model="currentItem.manufacturerId" required>
              <option value="">Выберите производителя</option>
              <option value="1">Kari</option>
              <option value="2">Marco Tozzi</option>
              <option value="3">Рос</option>
              <option value="4">Rieker</option>
              <option value="5">Alessio Nesca</option>
              <option value="6">CROSBY</option>
            </select>
          </div>

          <!-- Категория -->
          <div class="form-group">
            <label>Категория</label>
            <select v-model="currentItem.categoryId" required>
              <option value="">Выберите категорию</option>
              <option value="1">Женская обувь</option>
              <option value="2">Мужская обувь</option>
            </select>
          </div>

          <!-- Фото (обязательно для нового товара) -->
          <div class="form-group">
            <label>Изображение товара (300x200 px)</label>
            <div class="photo-upload-container">
              <button type="button" class="select-photo-btn" @click="triggerFileInput">
                <i class="fas fa-image"></i>
                Выбрать изображение
              </button>
              <input 
                type="file" 
                ref="fileInput"
                accept="image/png, image/jpeg, image/jpg"
                @change="handleFileUpload"
                style="display: none;"
              />
            </div>
            
            <!-- Превью с проверкой размера -->
            <div v-if="photoPreview" class="photo-preview-container">
              <div class="photo-preview">
                <img :src="photoPreview" alt="Превью" />
                <button type="button" class="remove-photo" @click="removePhoto">
                  <i class="fas fa-times"></i>
                </button>
              </div>
              <div class="photo-info" :class="{ 'error': !isImageValidSize }">
                <i :class="isImageValidSize ? 'fas fa-check-circle success' : 'fas fa-exclamation-circle error'"></i>
                <span v-if="isImageValidSize">Размер изображения: {{ imageWidth }}x{{ imageHeight }} (корректный)</span>
                <span v-else>Ожидаемый размер: 300x200 px, текущий: {{ imageWidth }}x{{ imageHeight }}</span>
              </div>
            </div>

            <!-- Предупреждение для нового товара -->
            <small v-if="modalMode === 'add' && !photoPreview" class="required-hint">
              * Обязательно загрузите изображение
            </small>
          </div>

          <!-- Кнопки -->
          <div class="form-actions">
            <button type="button" class="cancel-btn" @click="closeModal">
              Отмена
            </button>
            <button type="submit" class="save-btn" :disabled="!canSave">
              <i class="fas fa-spinner fa-spin" v-if="saving"></i>
              <span v-else>Сохранить</span>
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Модальное окно подтверждения удаления -->
    <div v-if="showDeleteConfirm" class="modal-overlay" @click.self="showDeleteConfirm = false">
      <div class="modal-content confirm-modal">
        <div class="modal-header">
          <h2>Подтверждение удаления</h2>
          <button class="close-btn" @click="showDeleteConfirm = false">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <div class="confirm-content">
          <i class="fas fa-exclamation-triangle warning-icon"></i>
          <p>Вы уверены, что хотите удалить товар?</p>
          <p class="item-info">{{ itemToDelete?.articuleItem }} - {{ itemToDelete?.descriptionItem }}</p>
        </div>
        
        <div class="confirm-actions">
          <button class="cancel-btn" @click="showDeleteConfirm = false">
            Отмена
          </button>
          <button class="delete-confirm-btn" @click="deleteItem" :disabled="deleting">
            <i class="fas fa-spinner fa-spin" v-if="deleting"></i>
            <span v-else>Удалить</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import defaultImage from '../assets/image/default-product.jpg'

export default {
  name: 'KatalogViews',
  
  data() {
    return {
      // Константы как в WPF
      MAX_ARTICULE_LENGTH: 6,
      MAX_DESCRIPTION_LENGTH: 500,
      MAX_MEASUREMENT_LENGTH: 20,
      DEFAULT_TYPE_ITEM: 1,
      REQUIRED_IMAGE_WIDTH: 300,
      REQUIRED_IMAGE_HEIGHT: 200,
      
      items: [],
      filteredItems: [],
      loading: true,
      error: null,
      defaultImagePath: defaultImage,
      userData: null,
      
      // Фильтры и сортировка
      searchQuery: '',
      selectedCategory: '',
      selectedSupplier: '',
      quantitySort: '',
      priceSort: '',
      dateSort: '', // НОВАЯ: сортировка по дате
      
      // Модальное окно товара
      showItemModal: false,
      modalMode: 'add', // 'add' или 'edit'
      currentItem: {
        articuleItem: '',
        descriptionItem: '',
        measurementItem: 'шт.',
        priceItem: 0,
        discountItem: 0,
        countInStorageItem: 0,
        supplierId: '',
        manufacturerId: '',
        categoryId: '',
        typeId: 1,
        photoItem: null
      },
      
      // Фото
      photoPreview: null,
      selectedFile: null,
      imageWidth: 0,
      imageHeight: 0,
      isImageValidSize: false,
      
      // Состояния
      saving: false,
      showDeleteConfirm: false,
      itemToDelete: null,
      deleting: false
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

    // Проверка на администратора
    isAdmin() {
      const role = this.getUserRole
      return role === 'admin' || role === 'администратор'
    },

    // Проверка возможности сохранения
    canSave() {
      if (this.modalMode === 'add') {
        // Для нового товара проверяем все поля
        return (
          this.currentItem.articuleItem &&
          this.currentItem.articuleItem.length <= this.MAX_ARTICULE_LENGTH &&
          this.currentItem.descriptionItem &&
          this.currentItem.descriptionItem.length <= this.MAX_DESCRIPTION_LENGTH &&
          this.currentItem.priceItem > 0 &&
          this.currentItem.countInStorageItem > 0 &&
          this.currentItem.supplierId &&
          this.currentItem.manufacturerId &&
          this.currentItem.categoryId &&
          this.currentItem.discountItem >= 0 &&
          this.currentItem.discountItem <= 99 &&
          this.isImageValidSize
        )
      } else {
        // Для редактирования фото необязательно
        return (
          this.currentItem.descriptionItem &&
          this.currentItem.descriptionItem.length <= this.MAX_DESCRIPTION_LENGTH &&
          this.currentItem.priceItem > 0 &&
          this.currentItem.countInStorageItem > 0 &&
          this.currentItem.supplierId &&
          this.currentItem.manufacturerId &&
          this.currentItem.categoryId &&
          this.currentItem.discountItem >= 0 &&
          this.currentItem.discountItem <= 99
        )
      }
    },

    // Уникальные категории для фильтра (из фиксированных значений)
    uniqueCategories() {
      return ['Женская обувь', 'Мужская обувь']
    },

    // Уникальные поставщики для фильтра (из фиксированных значений)
    uniqueSuppliers() {
      return ['Kari', 'Обувь для вас']
    }
  },
  
  async created() {
    this.getUserData()
    await this.loadItems()
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
      
      this.setGuestData()
    },
    goToOrders() {
  this.$router.push('/orders')
},
    
    // Установка данных гостя
    setGuestData() {
      this.userData = {
        idUserImport: 'guest_' + Date.now(),
        surnameUserImport: 'Гость',
        nameUserImport: '',
        lastnameUserImport: '',
        loginUserImport: 'Гость',
        roleUserImport: 'guest'
      }
      console.log('Установлены данные гостя:', this.userData)
    },
    
    async loadItems() {
      this.loading = true
      this.error = null
      
      try {
        const response = await axios.get('http://localhost:5224/api/Items')
        
        // Добавляем дату добавления для каждого товара (если её нет в API)
        // В реальном проекте дата должна приходить с сервера
        this.items = response.data.map((item, index) => {
          // Имитация даты добавления (для демонстрации)
          // В реальности используйте поле из API, например item.createdDate
          const date = new Date()
          date.setDate(date.getDate() - Math.floor(Math.random() * 30)) // Случайная дата за последние 30 дней
          
          return {
            ...item,
            addedDate: item.addedDate || date.toISOString() // Используем поле из API или генерируем
          }
        })
        
        this.applyFilters()
        console.log('Загружено товаров:', this.items.length)
        
      } catch (error) {
        console.error('Ошибка загрузки товаров:', error)
        this.error = 'Не удалось загрузить товары. Проверьте подключение к серверу.'
        this.items = []
        this.filteredItems = []
      } finally {
        this.loading = false
      }
    },

    // Применение фильтров и сортировки
    applyFilters() {
      let result = [...this.items]

      // Поиск по тексту
      if (this.searchQuery.trim()) {
        const query = this.searchQuery.toLowerCase().trim()
        result = result.filter(item => {
          return (
            (item.descriptionItem && item.descriptionItem.toLowerCase().includes(query)) ||
            (item.categoryName && item.categoryName.toLowerCase().includes(query)) ||
            (item.manufacture && item.manufacture.toLowerCase().includes(query)) ||
            (item.supplier && item.supplier.toLowerCase().includes(query))
          )
        })
      }

      // Фильтрация по категории
      if (this.selectedCategory) {
        result = result.filter(item => 
          item.categoryName.includes(this.selectedCategory)
        )
      }

      // Фильтрация по поставщику
      if (this.selectedSupplier) {
        result = result.filter(item => 
          item.supplier.includes(this.selectedSupplier)
        )
      }

      // Применяем сортировки в правильном порядке
      
      // Сначала сортировка по дате (если выбрана)
      if (this.dateSort) {
        result.sort((a, b) => {
          const dateA = new Date(a.addedDate || 0)
          const dateB = new Date(b.addedDate || 0)
          
          if (this.dateSort === 'desc') {
            return dateB - dateA // Сначала новые
          } else {
            return dateA - dateB // Сначала старые
          }
        })
      }

      // Затем сортировка по количеству (если выбрана)
      if (this.quantitySort) {
        result.sort((a, b) => {
          if (this.quantitySort === 'asc') {
            return a.countInStorageItem - b.countInStorageItem
          } else {
            return b.countInStorageItem - a.countInStorageItem
          }
        })
      }

      // Затем сортировка по цене (если выбрана)
      if (this.priceSort) {
        result.sort((a, b) => {
          const priceA = a.priceWithDiscount
          const priceB = b.priceWithDiscount
          
          if (this.priceSort === 'asc') {
            return priceA - priceB
          } else {
            return priceB - priceA
          }
        })
      }

      this.filteredItems = result
    },

    // Сброс всех фильтров
    resetFilters() {
      this.searchQuery = ''
      this.selectedCategory = ''
      this.selectedSupplier = ''
      this.quantitySort = ''
      this.priceSort = ''
      this.dateSort = '' // НОВАЯ: сбрасываем сортировку по дате
      this.filteredItems = [...this.items]
    },
    
    // Открыть модалку добавления товара
    openAddItemModal() {
      this.modalMode = 'add'
      this.currentItem = {
        articuleItem: '',
        descriptionItem: '',
        measurementItem: 'шт.',
        priceItem: 0,
        discountItem: 0,
        countInStorageItem: 0,
        supplierId: '',
        manufacturerId: '',
        categoryId: '',
        typeId: this.DEFAULT_TYPE_ITEM,
        photoItem: null
      }
      this.photoPreview = null
      this.selectedFile = null
      this.imageWidth = 0
      this.imageHeight = 0
      this.isImageValidSize = false
      this.showItemModal = true
    },
    
    // Открыть модалку редактирования товара
    openEditItemModal(item) {
      this.modalMode = 'edit'
      
      // Заполняем текущий товар
      this.currentItem = {
        articuleItem: item.articuleItem,
        descriptionItem: item.descriptionItem,
        measurementItem: 'шт.',
        priceItem: item.priceItem,
        discountItem: item.discountItem,
        countInStorageItem: item.countInStorageItem,
        supplierId: item.idSupplierItem,
        manufacturerId: item.idManufactureItem,
        categoryId: item.categoryItem,
        typeId: this.DEFAULT_TYPE_ITEM,
        photoItem: item.photoItem
      }
      
      // Если есть фото, создаем превью
      if (item.photoItem) {
        this.photoPreview = this.getImageUrl(item.photoItem)
        // Для существующего фото не проверяем размер
        this.isImageValidSize = true
      } else {
        this.photoPreview = null
        this.isImageValidSize = false
      }
      
      this.showItemModal = true
    },
    
    // Закрыть модалку
    closeModal() {
      this.showItemModal = false
      this.currentItem = {}
      this.photoPreview = null
      this.selectedFile = null
      this.imageWidth = 0
      this.imageHeight = 0
      this.isImageValidSize = false
    },
    
    // Триггер для выбора файла
    triggerFileInput() {
      this.$refs.fileInput.click()
    },
    
    // Обработка загрузки файла
    handleFileUpload(event) {
      const file = event.target.files[0]
      if (!file) return
      
      this.selectedFile = file
      
      // Создаем объект для чтения файла
      const reader = new FileReader()
      reader.onload = (e) => {
        // Создаем изображение для проверки размеров
        const img = new Image()
        img.onload = () => {
          this.imageWidth = img.width
          this.imageHeight = img.height
          
          // Проверяем размер
          if (img.width === this.REQUIRED_IMAGE_WIDTH && img.height === this.REQUIRED_IMAGE_HEIGHT) {
            this.isImageValidSize = true
            this.photoPreview = e.target.result
            // Конвертируем в base64 для отправки
            this.currentItem.photoItem = e.target.result.split(',')[1]
          } else {
            this.isImageValidSize = false
            this.showNotification(
              `Изображение должно быть размером ${this.REQUIRED_IMAGE_WIDTH}x${this.REQUIRED_IMAGE_HEIGHT} пикселей.\n` +
              `Текущий размер: ${img.width}x${img.height}`,
              'error'
            )
            this.$refs.fileInput.value = ''
          }
        }
        img.src = e.target.result
      }
      reader.readAsDataURL(file)
    },
    
    // Удалить фото
    removePhoto() {
      this.photoPreview = null
      this.currentItem.photoItem = null
      this.selectedFile = null
      this.imageWidth = 0
      this.imageHeight = 0
      this.isImageValidSize = false
      this.$refs.fileInput.value = ''
    },
    
    // Сохранить товар
    async saveItem() {
      // Валидация
      if (this.modalMode === 'add' && !this.currentItem.articuleItem) {
        this.showNotification('Введите артикул товара', 'error')
        return
      }

      if (!this.currentItem.descriptionItem) {
        this.showNotification('Введите описание товара', 'error')
        return
      }

      if (this.currentItem.priceItem <= 0) {
        this.showNotification('Введите корректную цену (положительное число)', 'error')
        return
      }

      if (this.currentItem.discountItem > 99) {
        this.showNotification('Не возможно добавить товар, скидка слишком большая', 'error')
        return
      }

      if (this.currentItem.countInStorageItem <= 0) {
        this.showNotification('Добавьте количество на складе', 'error')
        return
      }

      if (!this.currentItem.supplierId) {
        this.showNotification('Выберите поставщика', 'error')
        return
      }

      if (!this.currentItem.manufacturerId) {
        this.showNotification('Выберите производителя', 'error')
        return
      }

      if (!this.currentItem.categoryId) {
        this.showNotification('Выберите категорию', 'error')
        return
      }

      // Проверка фото для нового товара
      if (this.modalMode === 'add' && !this.isImageValidSize) {
        this.showNotification('Загрузите изображение товара размером 300x200 пикселей', 'error')
        return
      }

      this.saving = true
      
      try {
        if (this.modalMode === 'add') {
          // Добавление нового товара
          await axios.post('http://localhost:5224/api/Items/AddItem', this.currentItem)
          this.showNotification('Товар успешно добавлен', 'success')
        } else {
          // Обновление существующего товара
          await axios.put(`http://localhost:5224/api/Items/UpdateItem/${this.currentItem.articuleItem}`, this.currentItem)
          this.showNotification('Товар успешно обновлен', 'success')
        }
        
        // Перезагружаем товары
        await this.loadItems()
        this.closeModal()
      } catch (error) {
        console.error('Ошибка сохранения товара:', error)
        
        // Обработка ошибок
        let errorMessage = 'Ошибка при сохранении товара:\n'
        
        if (error.response) {
          if (error.response.status === 400) {
            errorMessage += error.response.data
          } else if (error.response.data && error.response.data.includes('already exists')) {
            errorMessage = 'Товар с таким артикулом уже существует'
          } else {
            errorMessage += error.response.data || error.message
          }
        } else {
          errorMessage += error.message
        }
        
        this.showNotification(errorMessage, 'error')
      } finally {
        this.saving = false
      }
    },
    
    // Подтверждение удаления
    confirmDelete(item) {
      this.itemToDelete = item
      this.showDeleteConfirm = true
    },
    
    // Удаление товара
    async deleteItem() {
      if (!this.itemToDelete) return
      
      this.deleting = true
      
      try {
        await axios.delete(`http://localhost:5224/api/Items/DeleteItem/${this.itemToDelete.articuleItem}`)
        
        // Удаляем из локального массива
        this.items = this.items.filter(i => i.articuleItem !== this.itemToDelete.articuleItem)
        this.applyFilters()
        
        this.showNotification('Товар успешно удален', 'success')
        this.showDeleteConfirm = false
        this.itemToDelete = null
      } catch (error) {
        console.error('Ошибка удаления товара:', error)
        this.showNotification('Ошибка при удалении товара', 'error')
      } finally {
        this.deleting = false
      }
    },
    
    // Показать уведомление
    showNotification(message, type = 'info') {
      alert(message)
    },
    
    handleLogout() {
      localStorage.removeItem('user')
      localStorage.removeItem('authToken')
      localStorage.removeItem('userRole')
      localStorage.removeItem('lastLogin')
      sessionStorage.removeItem('currentUser')
      sessionStorage.removeItem('sessionActive')
      
      this.$router.push('/')
    },
    
    formatPrice(price) {
      return new Intl.NumberFormat('ru-RU').format(price)
    },
    
    // НОВЫЙ МЕТОД: Форматирование даты
    formatDate(dateString) {
      if (!dateString) return ''
      
      const date = new Date(dateString)
      return date.toLocaleDateString('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      })
    },
    
    // НОВЫЙ МЕТОД: Проверка, является ли товар новым (добавлен за последние 7 дней)
    isNewItem(item) {
      if (!item.addedDate) return false
      
      const addedDate = new Date(item.addedDate)
      const now = new Date()
      const diffTime = Math.abs(now - addedDate)
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      
      return diffDays <= 7
    },
    
    hasPhoto(photoBytes) {
      if (!photoBytes) return false
      if (Array.isArray(photoBytes)) {
        return photoBytes.length > 0
      }
      if (photoBytes instanceof ArrayBuffer) {
        return photoBytes.byteLength > 0
      }
      if (typeof photoBytes === 'string') {
        return photoBytes.length > 0
      }
      return false
    },
    
    getImageUrl(photoBytes) {
      try {
        if (!this.hasPhoto(photoBytes)) {
          return this.defaultImagePath
        }
        
        if (typeof photoBytes === 'string') {
          if (photoBytes.startsWith('data:image')) {
            return photoBytes
          }
          return `data:image/jpeg;base64,${photoBytes}`
        }
        
        if (Array.isArray(photoBytes)) {
          const uint8Array = new Uint8Array(photoBytes)
          return this.convertToBase64(uint8Array)
        }
        
        if (photoBytes instanceof ArrayBuffer) {
          const uint8Array = new Uint8Array(photoBytes)
          return this.convertToBase64(uint8Array)
        }
        
        return this.defaultImagePath
        
      } catch (error) {
        console.error('Ошибка обработки фото:', error)
        return this.defaultImagePath
      }
    },
    
    convertToBase64(buffer) {
      try {
        let binary = ''
        const bytes = new Uint8Array(buffer)
        
        for (let i = 0; i < bytes.length; i++) {
          binary += String.fromCharCode(bytes[i])
        }
        
        return 'data:image/jpeg;base64,' + window.btoa(binary)
      } catch (error) {
        console.error('Ошибка конвертации в base64:', error)
        return this.defaultImagePath
      }
    },
    
    handleImageError(event) {
      console.log('Ошибка загрузки изображения, заменяем на заглушку')
      event.target.src = this.defaultImagePath
      event.target.classList.add('placeholder')
    }
  }
}
</script>

<style scoped>
.catalog-container {
  padding: 10px;
  max-width: 100%;
  margin: 0px;
  font-family: Arial, sans-serif;
  background-color: #b3ff00;
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

.add-item-btn {
  background: linear-gradient(135deg, #28a745, #218838);
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
  box-shadow: 0 2px 5px rgba(40, 167, 69, 0.3);
}

.add-item-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(40, 167, 69, 0.4);
  background: linear-gradient(135deg, #34ce57, #28a745);
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

/* Секция фильтров */
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

.search-group {
  grid-column: span 2;
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

.search-input::placeholder {
  color: #999;
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
  display: flex;
  align-items: center;
  gap: 8px;
  color: #2c3e50;
  font-size: 14px;
  font-weight: 600;
  padding: 15px 0 5px;
  border-top: 1px solid #e0e0e0;
}

.results-info i {
  color: #667eea;
}

.search-info {
  color: #667eea;
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

.no-results-hint {
  color: #95a5a6;
  font-size: 14px;
  margin-top: 5px;
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

/* Карточки товаров */
.catalog-page {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-bottom: 30px;
}

.card {
  background-color: rgb(0, 255, 30);
  width: 100%;
  min-height: 250px;
  display: flex;
  border-radius: 15px;
  box-shadow: 0 6px 15px rgba(0, 0, 0, 0.2);
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.25);
}

.card.editable {
  cursor: pointer;
}

.card.editable:hover .edit-overlay {
  opacity: 1;
}

.admin-card {
  border: 2px solid #ffc107;
}

.orders-btn {
  background: linear-gradient(135deg, #ffc107, #ff9800);
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
  box-shadow: 0 2px 5px rgba(255, 152, 0, 0.3);
}

.orders-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(255, 152, 0, 0.4);
  background: linear-gradient(135deg, #ff9800, #ffc107);
}

.orders-btn i {
  font-size: 18px;
}

.card_image {
  background-color: #ff4444;
  width: 350px;
  height: 220px;
  margin: 15px;
  border-radius: 10px;
  overflow: hidden;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
  background-color: white;
}

.product-image.placeholder {
  object-fit: contain;
  padding: 10px;
  background-color: #f0f0f0;
}

.text_block {
  background-color: blanchedalmond;
  min-height: 220px;
  width: 60%;
  margin: 15px 10px;
  padding: 20px;
  border-radius: 10px;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.text_block p {
  margin: 5px 0;
  font-size: 15px;
  line-height: 1.4;
}

.category-name {
  font-weight: bold;
  color: #2c3e50;
  font-size: 18px !important;
  margin-bottom: 8px !important;
  border-bottom: 1px solid #ddd;
  padding-bottom: 5px !important;
}

.description {
  color: #34495e;
  font-style: italic;
  font-size: 16px !important;
  margin-bottom: 8px !important;
}

.manufacture, .supplier {
  color: #7f8c8d;
  font-size: 14px !important;
}

.price-block {
  margin: 8px 0;
  padding: 8px;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px solid #dee2e6;
}

.original-price {
  color: #dc3545;
  text-decoration: line-through;
  font-size: 16px !important;
  margin-right: 10px !important;
  display: inline-block;
}

.current-price {
  color: #28a745;
  font-weight: bold;
  font-size: 20px !important;
}

.zero_count{
  color: blue !important;
}

.measurement {
  color: #495057;
  font-weight: 500;
  margin-top: 5px !important;
}

.stock  {
  padding: 5px 12px;
  border-radius: 4px;
  display: inline-block;
  margin-top: 5px !important;
  font-weight: bold;
  font-size: 14px !important;
  background-color: #ffc107 !important;
  color: #212529;
}

/* НОВЫЕ СТИЛИ для информации о дате */
.date-info {
  margin-top: 8px;
  padding: 4px 8px;
  background: #e9ecef;
  border-radius: 4px;
  font-size: 12px;
  color: #495057;
  display: flex;
  align-items: center;
  gap: 5px;
}

.date-info i {
  color: #667eea;
  font-size: 12px;
}

/* НОВЫЙ СТИЛЬ для бейджа "Новинка" */
.new-badge {
  position: absolute;
  top: 10px;
  left: 10px;
  background: linear-gradient(135deg, #ff4757, #ff6b81);
  color: white;
  padding: 5px 10px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: bold;
  display: flex;
  align-items: center;
  gap: 5px;
  box-shadow: 0 2px 5px rgba(255, 71, 87, 0.3);
  z-index: 10;
  animation: pulse 2s infinite;
}

.new-badge i {
  font-size: 12px;
}

@keyframes pulse {
  0% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
  100% {
    transform: scale(1);
  }
}

.card_sale {
  background-color: coral;
  margin: 15px 15px 15px 5px;
  width: 100px;
  height: 100px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  align-self: center;
}

.card_sale h1 {
  font-size: 32px;
  color: white;
  text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
  margin: 0;
}

.edit-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 20px;
  gap: 10px;
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
}

.edit-overlay i {
  font-size: 40px;
}

.delete-btn {
  position: absolute;
  top: 10px;
  right: 10px;
  background: rgba(220, 53, 69, 0.9);
  color: white;
  border: none;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  z-index: 10;
  opacity: 0;
}

.card:hover .delete-btn {
  opacity: 1;
}

.delete-btn:hover {
  background: #dc3545;
  transform: scale(1.1);
}

/* Модальные окна */
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
  max-width: 600px;
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

/* Форма товара */
.item-form {
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
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 10px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  font-size: 14px;
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.form-group input:focus,
.form-group select:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.form-group textarea {
  resize: vertical;
  min-height: 80px;
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

.warning-hint {
  display: block;
  color: #ffc107;
  font-size: 12px;
  margin-top: 5px;
}

.required-hint {
  display: block;
  color: #dc3545;
  font-size: 12px;
  margin-top: 5px;
  font-weight: bold;
}

/* Загрузка фото */
.select-photo-btn {
  background: linear-gradient(135deg, #667eea, #764ba2);
  color: white;
  border: none;
  padding: 12px 20px;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.select-photo-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
}

.photo-preview-container {
  margin-top: 15px;
}

.photo-preview {
  position: relative;
  width: 300px;
  height: 200px;
  border: 2px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
  margin-bottom: 10px;
}

.photo-preview img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.remove-photo {
  position: absolute;
  top: 5px;
  right: 5px;
  background: rgba(220, 53, 69, 0.9);
  color: white;
  border: none;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
}

.remove-photo:hover {
  background: #dc3545;
  transform: scale(1.1);
}

.photo-info {
  font-size: 13px;
  padding: 8px;
  border-radius: 4px;
  background: #f8f9fa;
}

.photo-info .success {
  color: #28a745;
  margin-right: 5px;
}

.photo-info .error {
  color: #dc3545;
  margin-right: 5px;
}

.photo-info.error {
  color: #dc3545;
  background: #ffeaea;
}

/* Кнопки формы */
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

/* Модальное окно подтверждения */
.confirm-modal {
  max-width: 400px;
}

.confirm-content {
  padding: 30px;
  text-align: center;
}

.warning-icon {
  font-size: 60px;
  color: #ffc107;
  margin-bottom: 20px;
}

.confirm-content p {
  margin: 10px 0;
  color: #2c3e50;
  font-size: 16px;
}

.confirm-content .item-info {
  font-weight: 600;
  color: #dc3545;
  background: #f8f9fa;
  padding: 10px;
  border-radius: 8px;
  margin-top: 15px;
}

.confirm-actions {
  display: flex;
  justify-content: center;
  gap: 15px;
  padding: 20px 30px;
  border-top: 1px solid #e0e0e0;
}

.delete-confirm-btn {
  padding: 10px 30px;
  background: #dc3545;
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

.delete-confirm-btn:hover:not(:disabled) {
  background: #c82333;
  transform: translateY(-2px);
}

.delete-confirm-btn:disabled {
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
@media (max-width: 1100px) {
  .filters-container {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .search-group {
    grid-column: span 2;
  }
  
  .reset-filters-btn {
    grid-column: span 2;
    margin-top: 0;
  }
}

@media (max-width: 900px) {
  .card {
    flex-direction: column;
    min-height: auto;
    padding: 15px;
  }
  
  .card_image {
    width: 100%;
    height: 250px;
    margin: 0 auto 15px;
  }
  
  .text_block {
    width: 100%;
    margin: 0 auto 15px;
    min-height: auto;
  }
  
  .card_sale {
    width: 80px;
    height: 80px;
    margin: 0 auto;
  }
  
  .card_sale h1 {
    font-size: 28px;
  }

  .header-actions {
    flex-direction: column;
    width: 100%;
  }

  .add-item-btn,
  .logout-btn {
    width: 100%;
    justify-content: center;
  }

  .photo-preview {
    width: 100%;
    height: auto;
    aspect-ratio: 3/2;
  }
}

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
  
  .search-group {
    grid-column: span 1;
  }
  
  .reset-filters-btn {
    grid-column: span 1;
    margin-top: 0;
  }
  
  .header h1 {
    font-size: 24px;
  }
  
  .text_block p {
    font-size: 14px !important;
  }
  
  .category-name {
    font-size: 16px !important;
  }
  
  .current-price {
    font-size: 18px !important;
  }

  .modal-content {
    width: 95%;
    margin: 10px;
  }

  .item-form {
    padding: 20px;
  }
}
</style>