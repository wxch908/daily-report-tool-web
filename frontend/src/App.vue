<script setup lang="ts">
import { onMounted, ref } from 'vue'

interface DailyReport {
  id: number
  reportDate: string
  createdAt: string
  updatedAt: string
}

const reports = ref<DailyReport[]>([])
const errorMessage = ref('')
const isLoading = ref(false)
const loadSucceeded = ref(false)

async function loadReports() {
  isLoading.value = true
  errorMessage.value = ''
  loadSucceeded.value = false

  try {
    const response = await fetch('/api/daily-reports')

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`)
    }

    reports.value = (await response.json()) as DailyReport[]
    loadSucceeded.value = true
  } catch (error: unknown) {
    errorMessage.value =
      error instanceof Error ? error.message : '发生了未知错误'
  } finally {
    isLoading.value = false
  }
}

onMounted(loadReports)
</script>

<template>
  <main class="page">
    <h1>工作日报</h1>

    <p v-if="isLoading">正在查询 SQLite 数据库……</p>

    <div
      v-else-if="loadSucceeded"
      class="status-card success"
    >
      <p>SQLite 数据库查询成功。</p>
      <p>当前共有 {{ reports.length }} 条日报记录。</p>
    </div>

    <div v-else class="status-card error">
      <p>数据库查询失败：{{ errorMessage }}</p>
      <button type="button" @click="loadReports">
        重新查询
      </button>
    </div>
  </main>
</template>

<style scoped>
.page {
  padding: 48px 24px;
  text-align: left;
}

.status-card {
  padding: 20px;
  border: 1px solid;
  border-radius: 8px;
}

.success {
  color: #276749;
  background: #f0fff4;
  border-color: #9ae6b4;
}

.error {
  color: #9b2c2c;
  background: #fff5f5;
  border-color: #feb2b2;
}

button {
  padding: 8px 16px;
  cursor: pointer;
}
</style>