// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import App from './App'
import router from './router'

import VueMaterial from 'vue-material'
import 'vue-material/dist/vue-material.css';

Vue.use(VueMaterial)
Vue.config.productionTip = false;

import MainNav from '@/components/MainNav'
Vue.component('main-nav', MainNav);

import UserPreferences from '@/components/UserPreferences'
Vue.component('user-preferences', UserPreferences);

import ChangePassword from '@/components/ChangePassword'
Vue.component('change-password', ChangePassword);

Vue.material.registerTheme('default', {
  primary: 'teal',
  accent: 'teal',
  background: 'white'
})

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  template: '<App/>',
  components: { App }
})