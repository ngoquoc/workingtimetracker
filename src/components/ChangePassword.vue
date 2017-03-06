<template>
  <md-layout md-tag="md-card" md-column md-flex="100" class="md-primary">
    <md-card-header>
      <div class="md-title">Change password</div>
    </md-card-header>

    <md-card-content>
      <md-input-container md-has-password>
        <md-icon>lock</md-icon>
        <label>Current password</label>
        <md-input type="password" required v-model="oldPassword" />
      </md-input-container>

      <md-input-container md-has-password>
        <md-icon>lock</md-icon>
        <label>New password</label>
        <md-input type="password" required v-model="newPassword" />
      </md-input-container>

      <md-input-container md-has-password>
        <md-icon>lock</md-icon>
        <label>New password confirmation</label>
        <md-input type="password" required v-model="newPasswordConfirm" />
      </md-input-container>
    </md-card-content>

    <md-card-actions>
      <md-button v-show="!loading" @click.native="changePassword()">Submit</md-button>
      <md-button v-show="loading" disabled>Submiting ...</md-button>
    </md-card-actions>
  </md-layout>
</template>

<script>
import Vue from 'vue'

export default {
  name: 'change-password',
  data () {
    return {
      oldPassword: '',
      newPassword: '',
      newPasswordConfirm: '',
      loading: false
    }
  },
  methods: {
    changePassword() {
      var that = this;
      that.loading = true;

      Vue.axios.post('/auth/changePassword', {
        oldPassword: that.oldPassword,
        newPassword: that.newPassword,
        newPasswordConfirm: that.newPasswordConfirm
      })
      .then(function (response) {
        that.loading = false;
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
