<template>
  <md-layout class='login-page'>
    <md-layout md-flex='100' md-align='center'>
      <span class='md-display-3'>Working time tracker</span>
    </md-layout>
    <md-layout class='login-form' md-tag='form' novalidate @submit.stop.prevent='submit' md-flex='100' md-align='center'>
      <md-layout md-tag='md-card' md-column md-flex='30' md-flex-medium='40' md-flex-small='60' md-flex-xsmall='90' class='md-primary'>
        <md-card-header>
          <div class='md-title'>Login</div>
        </md-card-header>

        <md-card-content>
          <md-input-container>
            <md-icon>person</md-icon>
            <label>Email</label>
            <md-input email required v-model='email' />
          </md-input-container>

          <md-input-container md-has-password>
            <md-icon>lock</md-icon>
            <label>Password</label>
            <md-input type='password' required v-model='password' />
          </md-input-container>
        </md-card-content>

        <md-card-actions>
          <md-layout md-align="left">
            <router-link tag="a" style="color:white;" to="/register">
              Register
            </router-link>
          </md-layout>
          <md-button v-show='!loading' @click.native='login()'>Login</md-button>
          <md-button v-show='loading' disabled>Logging in ...</md-button>
        </md-card-actions>
      </md-layout>
    </md-layout>
  </md-layout>
</template>

<script>
import Vue from 'vue'
export default {
  name: 'login',
  data () {
    return {
      email: '',
      password: '',
      loading: false
    }
  },
  methods: {
    login: function() {
      Vue.axios.defaults.headers['Content-Type'] = 'application/x-www-form-urlencoded';
      this.loading = true;
      this.$auth.login({
        data: 'grant_type=password&username='+this.email+'&password='+this.password,
        success: function () {
          this.loading = false;
        },
        error: function () {
          this.loading = false;
        },
        rememberMe: true,
        redirect: '/app',
        fetchUser: true
      });
    }
  }
}
</script>

<style scoped>
.login-page {
  padding: 25vh 2vh;
}
.login-form {
  margin-top: 2em;
}
</style>
