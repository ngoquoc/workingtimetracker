<template>
  <md-layout class="login-page">
    <md-layout md-flex="100" md-align="center">
      <span class="md-display-3">Working time tracker</span>
    </md-layout>
    <md-layout class="login-form" md-tag="form" novalidate @submit.stop.prevent="submit" md-flex="100" md-align="center">
      <md-layout md-tag="md-card" md-column md-flex="30" md-flex-medium="40" md-flex-small="60" md-flex-xsmall="90" class="md-primary">
        <md-card-header>
          <div class="md-title">Login</div>
        </md-card-header>

        <md-card-content>
          <md-input-container>
            <md-icon>person</md-icon>
            <label>Email</label>
            <md-input email required v-model="email" />
          </md-input-container>

          <md-input-container md-has-password>
            <md-icon>lock</md-icon>
            <label>Password</label>
            <md-input type="password" required v-model="password" />
          </md-input-container>
        </md-card-content>

        <md-card-actions>
          <md-button v-show="!loading" type="submit">Login</md-button>
        </md-card-actions>
      </md-layout>
    </md-layout>
  </md-layout>
</template>

<script>
export default {
  name: 'login',
  data () {
    return {
      email: '',
      password: '',
      loading: false
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
