<template>
  <div class="container">
    <h1>{{ msg }}</h1>
    <form class="form-group">
      <div class="mt-3">
        <label for="customFile">
          <button class="btn btn-success">
            <input type="file" id="customFile" name="fileName" @change="handleFileChange" accept=".csv" />
            Choose File
          </button>
        </label>
      </div>
      <h4 v-if="file" class="mt-3">Selected file: {{ file }}</h4>
    </form>
  </div>
</template>

<script>
import { ref } from "vue";
import http from "../http-common";

export default {
  name: "Uploader",
  props: {
    msg: String,
  },
  setup() {
    var file = ref("");
    function handleFileChange(e) {
      this.file = e.target.files[0].name;
      console.log(file);
      addRecords(file);
    }

    function addRecords(localFile) {
      let formData = new FormData();

      formData.append("file", localFile);

      return http.post("/upload", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
    }

    return {
      file,
      handleFileChange,
    };
  },
};
</script>

<style>
h1 {
  color: #35495e;
}

#customFile {
  opacity: 0;
  position: absolute;
  width: 111px;
  height: 38px;
  z-index: 2;
  margin: -7px -13px;
  cursor: pointer;
  overflow: hidden;
}
input {
  cursor: pointer;
}
.btn {
  color: #35495e;
  background-color: #1db36f;
}
label {
  margin-top: 5px;
}
</style>
