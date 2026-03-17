import { createRouter, createWebHistory } from 'vue-router'

import AuthorizationViews from '../Views/AuthorizationViews.vue'
import KatalogViews from '../Views/KatalogViews.vue'
import Orders from '../Views/Orders.vue'

const routes = [
  {
    path: '/',
    name: 'authorizationViews',
    component: AuthorizationViews  
  },
  {
    path: '/KatalogViews',
    name: "katalogViews",
    component: KatalogViews
  },
  {
     path: '/orders',
     name: 'orders',
     component: Orders,
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router