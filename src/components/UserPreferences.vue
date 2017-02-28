<template>
  <md-layout md-tag="md-card" md-flex="100" md-column class="md-primary">
    <md-card-header>
      <div class="md-title">User preferences</div>
    </md-card-header>

    <md-card-content>
      <md-input-container>
        <md-icon>timer</md-icon>
        <label>Preferred working hours per day</label>
        <md-input type="number" required v-model="preferredWorkingHoursPerDay" />
      </md-input-container>
    </md-card-content>

    <md-card-actions>
      <md-button v-show="!loading" type="submit">Save</md-button>
    </md-card-actions>
  </md-layout>
</template>

<script>
export default {
  name: 'user-preferences',
  data () {
    return {
      loading: false,
      preferredWorkingHoursPerDay: 0
    }
  }
}
</script>

<style scoped>
</style>
