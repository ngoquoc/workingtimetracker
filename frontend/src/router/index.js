import Vue from 'vue'
import Router from 'vue-router'
import Register from '@/components/Register'
import Login from '@/components/Login'
import TimeEntries from '@/components/TimeEntries'
import UserSettings from '@/components/UserSettings'
import Users from '@/components/Users'
import AppContainer from '@/components/AppContainer'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/register',
      name: 'Register',
      component: Register
    },
    {
      path: '/login',
      name: 'Login',
      component: Login
    },
    {
      path: '/app',
      component: AppContainer,
      meta: { auth: true },
      children: [
        {
          meta: { auth: ['ADMIN', 'USER'] },
          path: 'time-entries',
          name: 'TimeEntries',
          component: TimeEntries
        },
        {
          meta: { auth: ['ADMIN', 'USER MANAGER'] },
          path: 'users',
          name: 'Users',
          component: Users
        },
        {
          path: 'settings',
          name: 'UserSettings',
          component: UserSettings
        }
      ]
    }
  ]
})
