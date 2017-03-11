<template>
<md-theme md-name="primary">
<md-table-card>
  <md-toolbar>
    <h1 class="md-title">Time entries</h1>
    <md-button @click.native="fetchTimeEntries()">
      <md-icon>refresh</md-icon>
    </md-button>

    <md-switch class="md-primary" v-model="filterInfo.enabled">Filter</md-switch>
    <md-layout v-show="filterInfo.enabled">
      <el-date-picker v-model="filterInfo.from" type="date" placeholder="From">
      </el-date-picker>
      <span> - </span>
      <el-date-picker v-model="filterInfo.to" type="date" placeholder="To">
      </el-date-picker>
    </md-layout>

    <md-switch class="md-primary" v-model="includeAllUsers">Show all entries</md-switch>

    <md-button @click.native="exportSummaryReport()">Report</md-button>

  </md-toolbar>

  <md-table md-sort="date" v-on:sort="sort">
    <md-table-header>
      <md-table-row>
        <md-table-head md-sort-by="OwnerId">Owner</md-table-head>
        <md-table-head md-sort-by="Date">Date</md-table-head>
        <md-table-head md-sort-by="Note">Note         </md-table-head>
        <md-table-head md-sort-by="Duration">Hours</md-table-head>
        <md-table-head></md-table-head>
      </md-table-row>
    </md-table-header>

    <md-table-body>
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
          <el-date-picker v-model="newTimeEntry.date" type="date" placeholder="Pick a day"></el-date-picker>
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
          <md-icon>add</md-icon>
        </md-button>

      </md-table-row>

      <md-table-row :class="{ danger: row.isUnderPreferredWorkingHourPerDay, success: !row.isUnderPreferredWorkingHourPerDay }" 
        v-for="(row, rowIndex) in timeEntries" 
        :key="row.id" 
        :md-item="row">
        
        <md-table-cell>
          <md-input-container v-show="isEditting(row)">
            <md-select name="ownerName" id="ownerId" v-model="editingTimeEntry.ownerId">
              <md-option v-for="(owner, idx) in owners" :key="owner.id" :value="owner.id">
                {{ owner.name }}
              </md-option>
            </md-select>
          </md-input-container>
          <span v-show="!isEditting(row)">{{ row.ownerName }}</span>
        </md-table-cell>

        <md-table-cell>
          <el-date-picker v-show="isEditting(row)" v-model="editingTimeEntry.date" type="date" placeholder="Pick a day"></el-date-picker>
          <span v-show="!isEditting(row)">{{ formatDate(row.date) }}</span>
        </md-table-cell>

        <md-table-cell>
          <md-input-container v-show="isEditting(row)">
            <md-textarea required v-model="editingTimeEntry.note"></md-textarea>
          </md-input-container>
          <span v-show="!isEditting(row)">{{ row.note }}</span>
        </md-table-cell>

        <md-table-cell>
          <md-input-container v-show="isEditting(row)">
            <md-input type="number" required v-model="editingTimeEntry.duration"></md-input>
          </md-input-container>
          <span v-show="!isEditting(row)">{{ row.duration }}</span>
        </md-table-cell>

        <md-button v-show="isEditting(row)" class="md-icon-button" @click.native="updateTimeEntry(row)">
          <md-icon>check</md-icon>
        </md-button>
        <md-button v-show="isEditting(row)" class="md-icon-button" @click.native="ignoreEditTimeEntry(row)">
          <md-icon>close</md-icon>
        </md-button>
        <md-button v-show="!isEditting(row)" class="md-icon-button" @click.native="markTimeEntryAsEditing(row)">
          <md-icon>edit</md-icon>
        </md-button>
        <md-button v-show="!isEditting(row)" @click.native="deleteTimeEntry(row)" class="md-icon-button">
          <md-icon>delete</md-icon>
        </md-button>

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
</md-theme>
</template>

<script>
import Vue from 'vue'
import GUID from '@/utils/uuid'
import Moment from 'moment'

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
      currentUser: {},
      includeAllUsers: false,
      sortInfo: {
        name: 'Date',
        type: 'desc'
      },
      filterInfo: {
        enabled: false,
        from: Moment().startOf('m'),
        to: Moment().endOf('m')
      },
      editingTimeEntry: {
        id: '',
        ownerId: '',
        ownerName: '',
        note: '',
        date: new Date(),
        duration: 0
      },
      pagingInfo: {
        page: 1,
        size: 5,
        total: 0
      }
    }
  },
  created: function () {
    this.currentUser = this.$auth.user()
    this.newTimeEntry.ownerId = this.currentUser.id
    this.fetchTimeEntries()
    this.fetchOwners()
  },
  methods: {
    onPageSizeChanged(pageSize) {
      this.pagingInfo.size = pageSize
      this.fetchTimeEntries()
    },
    onPageChanged(page) {
      this.pagingInfo.page = page
      this.fetchTimeEntries()
    },
    fetchTimeEntries() {
      var that = this;

      var endpoint = that.includeAllUsers ? '/timeEntry/all' : '/timeEntry';
      var params = {};
      params['$orderby'] = that.sortInfo.name + ' ' + that.sortInfo.type;
      if (that.filterInfo.enabled) {
        params['$filter'] = that.formatFilter();
      }
      params['$skip'] = (this.pagingInfo.page - 1) * this.pagingInfo.size;
      params['$top'] = this.pagingInfo.size;
      params['$inlinecount'] = 'allpages';

      Vue.axios.get(endpoint, {
        params: params
      })
      .then(resp => {
        if (resp && resp.data && resp.data.results && resp.data.results.length) {
          that.pagingInfo.total = resp.data.totalCount;
          that.timeEntries = resp.data.results.map(te => {
            return {
              id: te.id,
              ownerId: te.ownerId,
              ownerName: te.ownerName,
              note: te.note,
              date: new Date(te.date),
              duration: te.duration,
              isUnderPreferredWorkingHourPerDay: te.isUnderPreferredWorkingHourPerDay
            }
          })
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
        if (resp.data && resp.data.results && resp.data.results.length) {
          for (var i = 0; i < resp.data.results.length; ++i) {
            var u = resp.data.results[i]
            that.owners.push({
              id: u.id,
              name: u.name
            })
          }
        }
      })
      .catch(err => {
        console.log(err);
      })
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
    },
    sort(arg) {
      this.sortInfo = arg
      this.fetchTimeEntries()
    },
    formatDate(d) {
      return Moment(d)
        .local()
        .format('DD/MM/YYYY');
    },
    formatFilter() {
      var formatString = 'YYYY-MM-DDTHH:mm';
      var fromDate = Moment(this.filterInfo.from).add(-1, 'd').endOf('d').format(formatString);
      var toDate = Moment(this.filterInfo.to).add(1, 'd').startOf('d').format(formatString);

      return 'Date lt datetime\''+toDate+'\' and Date gt datetime\'' + fromDate + '\''
    },
    exportSummaryReport() {
      var that = this;
      var endpoint = that.includeAllUsers ? '/timeEntry/report/all' : '/timeEntry/report';

      var params = {};
      params['$orderby'] = that.sortInfo.name + ' ' + that.sortInfo.type;
      if (that.filterInfo.enabled) {
        params['$filter'] = that.formatFilter();
      }

      Vue.axios.get(endpoint, {
        params: params
      })
      .then(resp => {
        window.open().document.write(resp.data);
      })
      .catch(error => {
        if (error && error.response && error.response.status == 400 && error.response.data && error.response.data.message) {
          alert(error.response.data.message);
        } else if (error && error.response && error.response.status == 403) {
          alert('You are not allowed to get this resource.');
        } else {
          alert('Unexpected error happens.');
        }
      })
    },
    markTimeEntryAsEditing(timeEntry) {
      this.editingTimeEntry.id = timeEntry.id;
      this.editingTimeEntry.ownerId = timeEntry.ownerId;
      this.editingTimeEntry.note = timeEntry.note;
      this.editingTimeEntry.ownerName = timeEntry.ownerName;
      this.editingTimeEntry.date = timeEntry.date;
      this.editingTimeEntry.duration = timeEntry.duration;
    },
    ignoreEditTimeEntry(timeEntry) {
      this.editingTimeEntry.id = '';
    },
    isEditting(timeEntry) {
      return timeEntry.id == this.editingTimeEntry.id;
    },
    updateTimeEntry() {
      var self = this;
      
      Vue.axios.put('timeEntry/'+self.editingTimeEntry.id, {
        id: self.editingTimeEntry.id,
        ownerId: self.editingTimeEntry.ownerId,
        note: self.editingTimeEntry.note,
        date: self.editingTimeEntry.date,
        duration: self.editingTimeEntry.duration
      })
      .then(resp => {
        self.editingTimeEntry.id = '';
        self.fetchTimeEntries();
      })
      .catch(error => {
        if (error && error.response && error.response.data && error.response.data.message){
          alert(error.response.data.message);
        } else {
          alert('Unexpected error happens.');
        }
      })
    },
    deleteTimeEntry(timeEntry) {
      var self = this;
      
      Vue.axios.delete('timeEntry/'+timeEntry.id)
      .then(resp => {
        self.fetchTimeEntries();
      })
      .catch(error => {
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

<style>
.success {
  background: rgba(0,255,0,0.3);
}
.danger {
  background: rgba(255,0,0,0.3);
}
</style>
