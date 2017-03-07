<template>
<md-theme md-name="primary">
<md-table-card>
  <md-toolbar>
    <h1 class="md-title">Time entries</h1>
    <md-button @click.native="showNewTimeEntryForm()">
      <md-icon>add</md-icon>
    </md-button>
    <md-button @click.native="fetchTimeEntries()">
      <md-icon>refresh</md-icon>
    </md-button>
    <md-switch class="md-primary">Show all entries</md-switch>
  </md-toolbar>

  <md-table md-sort="date">
    <md-table-header>
      <md-table-row>
        <md-table-head md-sort-by="ownerName">Owner</md-table-head>
        <md-table-head md-sort-by="date">Date</md-table-head>
        <md-table-head md-sort-by="note">Note         </md-table-head>
        <md-table-head md-sort-by="duration">Hours</md-table-head>
        <md-table-head></md-table-head>
      </md-table-row>
    </md-table-header>

    <md-table-body>
      <md-table-row v-for="(row, rowIndex) in timeEntries" :key="rowIndex" :md-item="row">
        <md-table-cell>
          <span>{{ row.ownerName }}</span>
        </md-table-cell>
        <md-table-cell>
          <span>{{ row.date.toLocaleDateString() }}</span>
        </md-table-cell>
        <md-table-cell>
          <span>{{ row.note }}</span>
        </md-table-cell>
        <md-table-cell>
          <span>{{ row.duration }}</span>
        </md-table-cell>
        <md-button class="md-icon-button">
          <md-icon>edit</md-icon>
        </md-button>
      </md-table-row>

      <md-table-row>
        <md-table-cell>
          <md-input-container>
            <md-select name="ownerName" id="ownerId" v-model="newTimeEntry.ownerId">
              <md-option v-for="(owner, idx) in owners" :key="idx" :value="owner.id">
                {{ owner.name }}
              </md-option>
            </md-select>
          </md-input-container>
        </md-table-cell>
        <md-table-cell>
          <el-date-picker v-model="newTimeEntry.date" type="date" placeholder="Pick a day">
          </el-date-picker>
        </md-table-cell>
        <md-table-cell>
          <md-input-container>
            <md-textarea required v-model="newTimeEntry.note"></md-textarea>
          </md-input-container>
        </md-table-cell>
        <md-table-cell>
          <md-input-container>
            <md-input type="number" required v-model="newTimeEntry.duration"></md-input>
          </md-input-container>
        </md-table-cell>

        <md-button class="md-icon-button" @click.native="saveNewTimeEntry()">
          <md-icon>check</md-icon>
        </md-button>
      </md-table-row>

    </md-table-body>
  </md-table>
</md-table-card>
</md-theme>
</template>

<script>
import Vue from 'vue'
import GUID from '@/utils/uuid'

export default {
  name: 'time-entries-grid',
  data() {
    return {
      timeEntries: [],
      newTimeEntry: {
        loading: false,
        ownerId: '',
        ownerName: '',
        note: '',
        date: new Date(),
        duration: 0
      },
      owners: [],
      currentUser: {}
    }
  },
  created: function () {
    this.currentUser = this.$auth.user()
    this.newTimeEntry.ownerId = this.currentUser.id
    this.fetchTimeEntries()
    this.fetchOwners()
  },
  methods: {
    fetchTimeEntries() {
      var that = this;
      that.timeEntries = [];

      Vue.axios.get('/timeEntry')
      .then(resp => {
        if (resp && resp.data && resp.data.length) {
          for (var i = 0; i < resp.data.length; ++i) {
            var te = resp.data[i];
            te.date = new Date(te.date);
            that.timeEntries.push(te);
          }
        }
      })
      .catch(err => {
        console.log(err);
      })
    },
    fetchOwners() {
      var that = this;
      
      if (that.owners.length < 1) {
        that.owners = [
          {
            id: that.currentUser.id,
            name: that.currentUser.name
          }
        ]
      } else {
        that.owner = that.owners.slice(0, 1);
      }

      Vue.axios.get('/user')
      .then(resp => {
        if (resp.data && resp.data.users && resp.data.users.length) {
          resp.data.users.foreach(u => {
            that.owners.push({
              id: u.id,
              name: u.name
            })
          });
        }
      })
      .catch(err => {
        console.log(err);
      })
    },
    showNewTimeEntryForm() {
      alert('new time entry form');
    },
    saveNewTimeEntry() {
      var self = this;
      self.newTimeEntry.loading = true;
      var newTimeEntryId = GUID.guid();

      Vue.axios.put('timeEntry/'+newTimeEntryId, {
        id: newTimeEntryId,
        ownerId: self.newTimeEntry.ownerId,
        note: self.newTimeEntry.note,
        date: self.newTimeEntry.date,
        duration: self.newTimeEntry.duration
      })
      .then(resp => {
        self.newTimeEntry.loading = false;
        self.fetchTimeEntries();
      })
      .catch(error => {
        self.newTimeEntry.loading = false;
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message);
        } else {
          alert('Unexpected error happens.');
        }
      })
    }
  }
}
</script>

<style scoped>
</style>
