<template>
  <md-layout md-align="center">
    <md-layout class="user-preferences" md-flex="100" md-align="center">
      <md-layout md-flex="50" md-align="center">
        <user-preferences></user-preferences>
      </md-layout>  
    </md-layout>
    <md-layout class="change-password" md-flex="100" md-align="center">
      <md-layout md-flex="50" md-align="center">
        <change-password></change-password>
      </md-layout>
    </md-layout>
  </md-layout>
</template>

<script>
export default {
  name: 'hello',
  data () {
    return {
      msg: 'Welcome to Your Vue.js App'
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.change-password {
  margin-top: 2em;
}
</style>
