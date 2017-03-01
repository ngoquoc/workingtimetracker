<template>
  <md-layout md-align="center">
    <time-entries-grid></time-entries-grid>
  </md-layout>
</template>

<script>
export default {
  name: 'time-entries',
  data () {
    return {
      email: '',
      password: '',
      loading: false
    }
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
