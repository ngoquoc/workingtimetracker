<template>
  <md-layout md-align="center">
    <md-layout md-align="center" md-flex="70">
      <users-grid></users-grid>
    </md-layout>
  </md-layout>
</template>

<script>
import UsersGrid from '@/components/Users.grid'

export default {
  name: 'time-entries',
  data () {
    return {};
  },
  components: {
    UsersGrid
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
