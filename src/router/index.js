import Vue from 'vue'
import Router from 'vue-router'
import Hello from '@/components/Hello'
import Login from '@/components/Login'
import TimeEntries from '@/components/TimeEntries'
import UserSettings from '@/components/UserSettings'

Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'Hello',
      component: Hello
    },
    {
      path: '/login',
      name: 'Login',
      component: Login
    },
    {
      path: '/time-entries',
      name: 'TimeEntries',
      component: TimeEntries
    },
    {
      path: '/settings',
      name: 'UserSettings',
      component: UserSettings
    }
  ]
})
