<template>
<md-table-card>
  <md-toolbar>
    <h1 class="md-title">Users</h1>
  </md-toolbar>

  <md-table>
    <md-table-header>
      <md-table-row>
        <md-table-head md-sort-by="name">Name</md-table-head>
        <md-table-head md-sort-by="email">Email</md-table-head>
        <md-table-head md-sort-by="roles">User roles</md-table-head>
      </md-table-row>
    </md-table-header>

    <md-table-body>
      <md-table-row v-for="(row, rowIndex) in users" :key="rowIndex" :md-item="row">
        <md-table-cell>
          <span>{{ row.name }}</span>
        </md-table-cell>
        <md-table-cell>
          <span>{{ row.email }}</span>
        </md-table-cell>
        <md-table-cell>
          <span>{{ row.roles.join(', ') }}</span>
        </md-table-cell>
        <md-button class="md-icon-button">
            <md-icon>edit</md-icon>
          </md-button>
      </md-table-row>
    </md-table-body>
  </md-table>
</md-table-card>
</template>

<script>
export default {
  name: 'users-grid',
  data: () => ({
    users: [
      {
        name: 'Test user',
        email: 'quoc99@gmail.com',
        roles: ['Admin', 'User']
      },
      {
        name: 'Test user 2',
        email: 'quoc1010@gmail.com',
        roles: ['Admin', 'User']
      },
      {
        name: `You need to wrap all the elements of your toolbar in a element with the class `,
        email: 'quoc1010@gmail.com',
        roles: ['Admin', 'User', 'User manager']
      }
    ]
  })
}
</script>

<style scoped>
</style>
