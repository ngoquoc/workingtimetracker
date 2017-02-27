<template>
  <div id="app">
    <main-nav></main-nav>
    <router-view class="router-view"></router-view>
  </div>
</template>

<script>
export default {
  name: 'app'
}
</script>

<style>
.router-view {
  margin-top: 5vh;
}
</style>