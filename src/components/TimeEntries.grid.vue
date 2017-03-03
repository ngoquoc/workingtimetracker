<template>
<md-theme md-name="primary">
<md-table-card>
  <md-toolbar>
    <h1 class="md-title">Time entries</h1>

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
    </md-table-body>
  </md-table>
</md-table-card>
</md-theme>
</template>

<script>
export default {
  name: 'time-entries-grid',
  data: () => ({
    timeEntries: [
      {
        ownerId: '123456789',
        ownerName: 'Quoc Ngo',
        description: 'Working on Toptal test project',
        date: new Date(),
        duration: 0.6
      },
      {
        ownerId: '123456789',
        ownerName: 'Quoc Ngo',
        description: 'Working on Toptal test project',
        date: new Date(),
        duration: 0.6
      },
      {
        ownerId: '123456789',
        ownerName: 'Quoc Ngo',
        description: `Toolbars appear a step above the content they affect. They may constrain their width to accommodate material passing over them.

You need to wrap all the elements of your toolbar in a element with the class md-toolbar-container for medium and large toolbars. On large toolbars you can have two containers to push content to bottom.`,
        date: new Date(),
        duration: 0.6
      },
      {
        ownerId: '123456789',
        ownerName: 'Quoc Ngo',
        description: 'Working on Toptal test project',
        date: new Date(),
        duration: 0.6
      }
    ]
  })
}
</script>

<style scoped>
</style>
