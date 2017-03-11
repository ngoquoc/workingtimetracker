<template>
<md-table-card>
  <md-toolbar>
    <h1 class="md-title">Users</h1>
  </md-toolbar>

  <md-table md-sort="Name" v-on:sort="sort">
    <md-table-header>
      <md-table-row>
        <md-table-head md-sort-by="Name">Name</md-table-head>
        <md-table-head md-sort-by="Email">Email</md-table-head>
        <md-table-head>User roles</md-table-head>
      </md-table-row>
    </md-table-header>

    <md-table-body>

      <md-table-row>

        <md-table-cell>
          <md-input-container>
            <md-input required v-model="newUser.name"></md-input>
          </md-input-container>
        </md-table-cell>

        <md-table-cell>
          <md-input-container>
            <md-input required type="email" v-model="newUser.email"></md-input>
          </md-input-container>
        </md-table-cell>

        <md-table-cell>
          <md-input-container>
            <md-select name="new-user-roles" id="new-user-roles" multiple v-model="newUser.roles">
              <md-option value="USER">User</md-option>
              <md-option value="USER MANAGER">User manager</md-option>
              <md-option value="ADMIN">Admin</md-option>
            </md-select>
          </md-input-container>
        </md-table-cell>

        <md-button class="md-icon-button" @click.native="createUser()">
            <md-icon>add</md-icon>
        </md-button>

      </md-table-row>

      <md-table-row v-for="(row, rowIndex) in users" :key="rowIndex" :md-item="row">

        <md-table-cell>
          <md-input-container v-show="isEditting(row)">
            <md-input required v-model="row.name"></md-input>
          </md-input-container>
          <span v-show="!isEditting(row)">{{ row.name }}</span>
        </md-table-cell>

        <md-table-cell>
          <md-input-container v-show="isEditting(row)">
            <md-input required type="email" v-model="editingUser.email"></md-input>
          </md-input-container>
          <span v-show="!isEditting(row)">{{ row.email }}</span>
        </md-table-cell>

        <md-table-cell>
          <md-select v-show="isEditting(row)" name="roles" id="roles" multiple v-model="editingUser.roles">
            <md-option value="USER">User</md-option>
            <md-option value="USER MANAGER">User manager</md-option>
            <md-option value="ADMIN">Admin</md-option>
          </md-select>
          <span v-show="!isEditting(row)">{{ row.roles.join(', ') }}</span>
        </md-table-cell>

        <md-button v-show="isEditting(row)" class="md-icon-button" @click.native="updateUser(row)">
            <md-icon>check</md-icon>
          </md-button>
        <md-button v-show="isEditting(row)" class="md-icon-button" @click.native="ignoreEditingUser(row)">
          <md-icon>close</md-icon>
        </md-button>

        <md-button v-show="!isEditting(row)" class="md-icon-button" @click.native="markEditing(row)">
          <md-icon>edit</md-icon>
        </md-button>

        <md-menu v-show="!isEditting(row)">
          <md-button md-menu-trigger class="md-icon-button">
            <md-icon>delete</md-icon>
          </md-button>
          <md-menu-content>
            <md-menu-item @click.native="deleteUser(row, true)">Force delete</md-menu-item>
            <md-menu-item @click.native="deleteUser(row, false)" >Delete</md-menu-item>
          </md-menu-content>
        </md-menu>

      </md-table-row>
    </md-table-body>
  </md-table>

  <md-card-actions>
    <el-pagination
      @size-change="onPageSizeChanged"
      @current-change="onPageChanged"
      :current-page="pagingInfo.page"
      :page-sizes="[5,10,20,50,100]"
      :page-size="pagingInfo.size"
      layout="total, sizes, prev, pager, next"
      :total="pagingInfo.total">
    </el-pagination>
  </md-card-actions>
</md-table-card>
</template>

<script>
import Vue from 'vue'
import GUID from '@/utils/uuid'

export default {
  name: 'users-grid',
  data() {
    return {
      users: [
      ],
      sortInfo: {
        name: 'Name',
        type: 'asc'
      },
      editingUser: {
        id: '',
        name: '',
        email: '',
        roles: []
      },
      newUser: {
        id: '',
        name: '',
        email: '',
        roles: []
      },
      pagingInfo: {
        page: 1,
        size: 5,
        total: 0
      }
    }
  },
  created: function() {
    this.fetchUsers()
  },
  methods: {
    sort(arg) {
      this.sortInfo = arg
      this.fetchUsers()
    },
    onPageSizeChanged(pageSize) {
      this.pagingInfo.size = pageSize
      this.fetchUsers()
    },
    onPageChanged(page) {
      this.pagingInfo.page = page
      this.fetchUsers()
    },
    isEditting(user) {
      return this.editingUser.id == user.id;
    },
    markEditing(user) {
      this.editingUser.id = user.id;
      this.editingUser.name = user.name;
      this.editingUser.email = user.email;
      this.editingUser.roles = user.roles;
    },
    ignoreEditingUser() {
      this.editingUser.id = '';
    },
    fetchUsers() {
      var that = this;

      var params = {};
      params['$orderby'] = that.sortInfo.name + ' ' + that.sortInfo.type;
      params['excludeMe'] = false;
      params['$skip'] = (this.pagingInfo.page - 1) * this.pagingInfo.size;
      params['$top'] = this.pagingInfo.size;
      params['$inlinecount'] = 'allpages';

      Vue.axios.get('/user', {
        params: params
      })
      .then(resp => {
        if (resp && resp.data && resp.data.results && resp.data.results.length) {
          that.pagingInfo.total = resp.data.totalCount;
          that.users = resp.data.results.map(u => u);
        }
      })
      .catch(error => {
        if (error && error.response && error.response.status == 400 && error.response.data && error.response.data.message) {
          alert(error.response.data.message);
        } else if (error && error.response && (error.response.status == 403 || error.response.status == 401)) {
          alert('You are not allowed to get this resource.');
        } else {
          alert('Unexpected error happens.');
        }
      })
    },
    updateUser() {
      var self = this;
      var user = this.editingUser;
      
      Vue.axios.put('user/'+user.id, user)
      .then(resp => {
        var currentUser = self.$auth.user()
        if (user.id == currentUser.id) {
          self.$auth.fetch()
        }
        self.ignoreEditingUser()
        self.fetchUsers()
      })
      .catch(error => {
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message)
        } else if (error && error.response && error.response.status == 403) {
          alert('You are not allowed to update this user.');
        } else {
          console.warn(error)
          alert('Unexpected error happens.')
        }
      })
    },
    createUser() {
      var self = this;
      var user = this.newUser;
      user.id = GUID.guid();
      
      Vue.axios.put('user/'+user.id, user)
      .then(resp => {
        self.fetchUsers()
      })
      .catch(error => {
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message)
        } else if (error && error.response && error.response.status == 403) {
          alert('You are not allowed to create this user.');
        } else {
          console.warn(error)
          alert('Unexpected error happens.')
        }
      })
    },
    deleteUser(user, forceDelete) {
      var self = this;
      
      Vue.axios.delete('user/'+user.id, {
        data: {
          forceDelete: !!forceDelete
        }
      })
      .then(resp => {
        self.fetchUsers()
      })
      .catch(error => {
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message)
        } else if (error && error.response && error.response.status == 403) {
          alert('You are not allowed to delete this user.');
        } else {
          console.warn(error)
          alert('Unexpected error happens.')
        }
      })
    }
  }
}
</script>

<style scoped>
</style>
