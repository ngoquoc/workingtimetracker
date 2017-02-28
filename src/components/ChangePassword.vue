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
      <md-button v-show="!loading" type="submit">Submit</md-button>
    </md-card-actions>
  </md-layout>
</template>

<script>
export default {
  name: 'change-password',
  data () {
    return {
      oldPassword: '',
      newPassword: '',
      newPasswordConfirm: '',
      loading: false
    }
  }
}
</script>

<style scoped>
</style>
