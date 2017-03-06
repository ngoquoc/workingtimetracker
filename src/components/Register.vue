<template>
  <md-layout class="register-page">
    <md-layout md-flex='100' md-align='center'>
      <span class='md-display-3'>Working time tracker</span>
    </md-layout>
    <md-layout class="register-form" md-tag="form" novalidate @submit.stop.prevent="submit" md-flex="100" md-align="center">
      <md-layout md-tag="md-card" md-column md-flex="30" md-flex-medium="40" md-flex-small="60" md-flex-xsmall="90" class="md-primary">
        <md-card-header>
          <div class="md-title">Register</div>
        </md-card-header>

        <md-card-content>
          <md-input-container>
            <md-icon>person</md-icon>
            <label>Name</label>
            <md-input required v-model="name" />
          </md-input-container>

          <md-input-container>
            <md-icon>email</md-icon>
            <label>Email</label>
            <md-input email required v-model="email" />
          </md-input-container>

          <md-input-container md-has-password>
            <md-icon>lock</md-icon>
            <label>Password</label>
            <md-input type="password" required v-model="password" />
          </md-input-container>

          <md-input-container md-has-password>
            <md-icon>lock</md-icon>
            <label>Password confirmation</label>
            <md-input type="password" required v-model="passwordConfirm" />
          </md-input-container>

        </md-card-content>

        <md-card-actions>
          <md-layout md-align="left">
            <router-link tag="a" style="color:white;" to="/login">
              Login
            </router-link>
          </md-layout>
          <md-button v-show="!loading" type="submit">Submit</md-button>
        </md-card-actions>
        
      </md-layout>
    </md-layout>
  </md-layout>
</template>

<script>
export default {
  name: 'register',
  data: function() {
    return {
      name: '',
      email: '',
      password: '',
      passwordConfirm: '',
      loading: false
    };
  }
}
</script>

<style scoped>
.register-page {
  padding: 23.5vh 2vh;
}
.register-form {
  margin-top: 2em;
}
</style>