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
        <md-table-head md-sort-by="ownerName">Owner name</md-table-head>
        <md-table-head md-sort-by="date">Date</md-table-head>
        <md-table-head md-sort-by="description">Description</md-table-head>
        <md-table-head md-sort-by="duration">Duration (hours)</md-table-head>
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
          <span>{{ row.description }}</span>
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
            <md-select name="ownerName" id="ownerName" v-model="newTimeEntry.ownerId">
              <md-option value="fight_club">Fight Club</md-option>
              <md-option value="godfather">Godfather</md-option>
              <md-option value="godfather_ii">Godfather II</md-option>
              <md-option value="godfather_iii">Godfather III</md-option>
              <md-option value="godfellas">Godfellas</md-option>
              <md-option value="pulp_fiction">Pulp Fiction</md-option>
              <md-option value="scarface">Scarface</md-option>
            </md-select>
          </md-input-container>
        </md-table-cell>
        <md-table-cell>
          <el-date-picker v-model="newTimeEntry.date" type="date" placeholder="Pick a day">
          </el-date-picker>
        </md-table-cell>
        <md-table-cell>
          <md-input-container>
            <md-textarea required v-model="newTimeEntry.description"></md-textarea>
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

export default {
  name: 'time-entries-grid',
  data() {
    return {
      timeEntries: [],
      newTimeEntry: {
        ownerId: '',
        ownerName: '',
        description: '',
        date: new Date(),
        duration: 0
      }
    }
  },
  ready() {
    this.fetchTimeEntries()
  },
  created: function () {
    this.fetchTimeEntries()
  },
  methods: {
    fetchTimeEntries() {
      var that = this;

      Vue.axios.get('/timeEntry')
      .then(resp => {
        that.timeEntries = resp.data;
      })
      .catch(err => {
        console.log(err);
      })
    },
    showNewTimeEntryForm() {
      alert('new time entry form');
    },
    saveNewTimeEntry() {
      console.log(this.newTimeEntry);
    }
  }
}
</script>

<style scoped>
</style>
