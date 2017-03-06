<template>
<md-toolbar class="main-nav">
  <md-layout>
    <router-link v-if="$auth.check(['ADMIN', 'USER'])"  tag="span" class="md-button nav-link" to="/app/time-entries">
      <md-icon>list</md-icon>
      Time entries
    </router-link>
    <router-link v-if="$auth.check(['ADMIN', 'USER MANAGER'])" tag="span" class="md-button nav-link" to="/app/users">
      <md-icon>people</md-icon>
      Users
    </router-link>
  </md-layout>
  <md-layout md-align="end">
    <router-link tag="span" class="md-button nav-link" to="/app/settings">
      <md-icon>settings</md-icon>
      Settings
    </router-link>
    <md-button v-if="$auth.check()" @click.native="logout()">
      Logout
    </md-button>
  </md-layout>
</md-toolbar>
</template>

<script>
export default {
  name: 'main-nav',
  methods: {
    logout() {
      this.$auth.logout({
        makeRequest: false,
        success: function () {},
        error: function () {},
        redirect: '/login',
      });
    }
  }
}
</script>

<style>
.router-link-active {
  background: rgba(0, 0,0, 0.2);
}
</style>