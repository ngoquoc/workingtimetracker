<template>
  <md-layout class='time-entry-form'>
    <md-layout class='time-entry-form' md-tag='form' novalidate @submit.stop.prevent='submit' md-flex='100' md-align='center'>
      <md-layout md-tag='md-card' md-column md-flex='30' md-flex-medium='40' md-flex-small='60' md-flex-xsmall='90' class='md-primary'>
        <md-card-content>
          <md-input-container>
            <md-icon>person</md-icon>
            <label>Date</label>
            <md-input required v-model='date' />
          </md-input-container>

          <md-input-container>
            <md-icon>lock</md-icon>
            <label>Description</label>
            <md-input required v-model='description' />
          </md-input-container>

          <md-input-container>
            <md-icon>lock</md-icon>
            <label>Duration (hour)</label>
            <md-input type='number' required v-model='duration' />
          </md-input-container>
        </md-card-content>

        <md-card-actions>
          <md-button v-show='!loading' @click.native='login()'>Save</md-button>
          <md-button v-show='loading' disabled>Saving ...</md-button>
        </md-card-actions>
      </md-layout>
    </md-layout>
  </md-layout>
</template>

<script>
import Vue from 'vue'

export default {
  name: 'login',
  props: [
      'date',
      'description',
      'duration'
  ]
  data () {
    return {
        loading: false,
        date: new Date(),
        description: '',
        duration: 0
    }
  },
  methods: {
      submit() {

      }
  }
}
</script>

<style scoped>
</style>
