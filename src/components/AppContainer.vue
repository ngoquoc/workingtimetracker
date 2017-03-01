<template>
  <div>
    <main-nav></main-nav>
    <router-view class="app-container-inner"></router-view>
  </div>
</template>

<script>
export default {
  name: 'app-container'
}
</script>

<style scoped>
.app-container-inner {
  margin-top: 5vh;
}
</style>