<template>
<md-layout class="main-nav">
  <md-layout>
    <router-link tag="button" class="md-button nav-link" to="/time-entries">
      <md-icon>list</md-icon>
      Time entries
    </router-link>
    <router-link tag="button" class="md-button nav-link" to="/users">
      <md-icon>people</md-icon>
      Users
    </router-link>
  </md-layout>
  <md-layout md-align="end">
    <router-link tag="button" class="md-button nav-link" to="/settings">
      <md-icon>settings</md-icon>
      Settings
    </router-link>
  </md-layout>
</md-layout>
</template>

<script>
export default {
  name: 'main-nav'
}
</script>

<style>
.main-nav {
  background: #3f51b5;
  color: white;
}
.nav-link {
  margin: 0;
}
.router-link-active {
  background: #6a7ad4;
}
</style>