<template>
  <div>
    <img src="./assets/logo.png" alt="Vue logo">
    <Uploader msg="CSV Uploader"></Uploader>
  </div>
</template>

<script>
import Uploader from './components/Uploader.vue'
export default {
  name: 'App',
  components: {
    Uploader
  }
}
</script>
