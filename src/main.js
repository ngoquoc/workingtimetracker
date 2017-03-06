// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import App from './App'
import router from './router'

import VueMaterial from 'vue-material'
import 'vue-material/dist/vue-material.css';

import axios from 'axios'
import VueAxios from 'vue-axios'
import VueAuth from '@websanova/vue-auth'

import MainNav from '@/components/MainNav'
import UserPreferences from '@/components/UserPreferences'
import ChangePassword from '@/components/ChangePassword'

Vue.router = router

Vue.use(VueAxios, axios)
Vue.use(VueAuth, {
    auth: {
      request: function (req, token) {
        this.options.http._setHeaders.call(this, req, { Authorization: 'Bearer ' + token });
      },
      
      response: function (res) {
          return (res.data || {}).access_token;
      }
    },
    http: require('@websanova/vue-auth/drivers/http/axios.1.x.js'),
    router: require('@websanova/vue-auth/drivers/router/vue-router.2.x.js'),
    rolesVar: 'roles',
    loginData: {
      url: 'http://localhost:31798/token',
      method: 'POST'
    },
    fetchData: {
      url: 'auth/me'
    },
    parseUserData: function (data) {
        return data.user;
    },
    refreshData: {
      enabled: false
    }
});

Vue.axios.defaults.baseURL = 'http://localhost:31798/api/';

Vue.use(VueMaterial)
Vue.config.productionTip = false;

Vue.material.registerTheme('default', {
  primary: 'teal',
  accent: 'teal',
  background: 'white'
})

Vue.component('main-nav', MainNav);
Vue.component('user-preferences', UserPreferences);
Vue.component('change-password', ChangePassword);

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  template: '<App/>',
  components: { App }
})