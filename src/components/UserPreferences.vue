<template>
  <md-layout md-tag="md-card" md-flex="100" md-column class="md-primary">
    <md-card-header>
      <div class="md-title">User preferences</div>
    </md-card-header>

    <md-card-content>
      <md-input-container>
        <md-icon>person</md-icon>
        <label>Name</label>
        <md-input required v-model="name" />
      </md-input-container>
      <md-input-container>
        <md-icon>timer</md-icon>
        <label>Preferred working hours per day</label>
        <md-input type="number" required v-model="preferredWorkingHourPerDay" />
      </md-input-container>
    </md-card-content>

    <md-card-actions>
      <md-button v-show="!loading" @click.native="save()">Save</md-button>
      <md-button v-show="loading" disabled>Saving ...</md-button>
    </md-card-actions>
  </md-layout>
</template>

<script>
import Vue from 'vue'

export default {
  name: 'user-preferences',
  data () {
    var currentUser = this.$auth.user();
    return {
      loading: false,
      preferredWorkingHourPerDay: currentUser.preferredWorkingHourPerDay,
      name: currentUser.name
    }
  },
  methods: {
    save() {
      var that = this;
      that.loading = true;

      Vue.axios.post('/auth/me/preferrences', {
        name: this.name,
        preferredWorkingHourPerDay: Number(this.preferredWorkingHourPerDay)
      })
      .then(function (response) {
        that.loading = false;
        that.$auth.fetch();
      })
      .catch(function (error) {
        that.loading = false;
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message);
        } else {
          alert('Unexpected error happens.');
        }
      });
    }
  }
}
</script>

<style scoped>
</style>
