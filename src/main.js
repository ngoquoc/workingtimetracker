// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import App from './App'
import router from './router'

import VueMaterial from 'vue-material'
import 'vue-material/dist/vue-material.css';

Vue.use(VueMaterial)
Vue.config.productionTip = false

import TimeEntriesGrid from '@/components/TimeEntries.grid'
Vue.component('time-entries-grid', TimeEntriesGrid);

import MainNav from '@/components/MainNav'
Vue.component('main-nav', MainNav);

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  template: '<App/>',
  components: { App }
})
