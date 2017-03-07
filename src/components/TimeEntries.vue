<template>
  <md-layout md-align="center">
    <md-layout md-align="center" md-flex="90">
      <time-entries-grid></time-entries-grid>
    </md-layout>
  </md-layout>
</template>

<script>
import TimeEntriesGrid from '@/components/TimeEntries.grid'

export default {
  name: 'time-entries',
  data () {
    return {
      email: '',
      password: '',
      loading: false
    }
  },
  components: {
    TimeEntriesGrid
  }
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
</style>
