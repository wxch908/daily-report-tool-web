<script setup lang="ts">
import { onMounted, ref } from 'vue'

interface DailyReport {
  id: number
  reportDate: string
  createdAt: string
  updatedAt: string
}

function getTodayText() {
  const today = new Date()
  const year = today.getFullYear()
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day = String(today.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

const reports = ref<DailyReport[]>([])
const selectedDate = ref(getTodayText())
const errorMessage = ref('')
const operationMessage = ref('')
const isLoading = ref(false)
const isCreating = ref(false)

async function loadReports() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await fetch('/api/daily-reports')

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`)
    }

    reports.value = (await response.json()) as DailyReport[]
  } catch (error: unknown) {
    errorMessage.value =
      error instanceof Error ? error.message : '发生了未知错误'
  } finally {
    isLoading.value = false
  }
}

async function createReport() {
  if (!selectedDate.value) {
    errorMessage.value = '请先选择日报日期'
    return
  }

  isCreating.value = true
  errorMessage.value = ''
  operationMessage.value = ''

  try {
    const response = await fetch('/api/daily-reports', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        reportDate: selectedDate.value,
      }),
    })

    if (response.status === 409) {
      throw new Error('该日期的日报已经存在')
    }

    if (!response.ok) {
      throw new Error(`创建失败：HTTP ${response.status}`)
    }

    await loadReports()
    operationMessage.value = '日报创建成功'
  } catch (error: unknown) {
    errorMessage.value =
      error instanceof Error ? error.message : '发生了未知错误'
  } finally {
    isCreating.value = false
  }
}

function formatDate(value: string) {
  return value.slice(0, 10)
}

onMounted(loadReports)
</script>

<template>
  <main class="page">
    <h1>工作日报</h1>

    <section class="toolbar">
      <label>
        日报日期：
        <input v-model="selectedDate" type="date" />
      </label>

      <button
        type="button"
        :disabled="isCreating"
        @click="createReport"
      >
        {{ isCreating ? '正在创建……' : '新建日报' }}
      </button>
    </section>

    <p v-if="operationMessage" class="message">
      {{ operationMessage }}
    </p>

    <p v-if="isLoading">正在查询 SQLite 数据库……</p>

    <div v-else-if="errorMessage" class="status-card error">
      <p>{{ errorMessage }}</p>
      <button type="button" @click="loadReports">
        重新查询
      </button>
    </div>

    <section v-else class="status-card success">
      <p>当前共有 {{ reports.length }} 条日报记录。</p>

      <table v-if="reports.length > 0">
        <thead>
          <tr>
            <th>ID</th>
            <th>日报日期</th>
            <th>创建时间</th>
          </tr>
        </thead>

        <tbody>
          <tr
            v-for="report in reports"
            :key="report.id"
          >
            <td>{{ report.id }}</td>
            <td>{{ formatDate(report.reportDate) }}</td>
            <td>
              {{ new Date(report.createdAt).toLocaleString() }}
            </td>
          </tr>
        </tbody>
      </table>

      <p v-else>还没有日报，请创建第一条。</p>
    </section>
  </main>
</template>

<style scoped>
.page {
  padding: 48px 24px;
  text-align: left;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 20px;
}

input,
button {
  padding: 8px 12px;
  font: inherit;
}

button {
  cursor: pointer;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.6;
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

.message {
  color: #276749;
}

table {
  width: 100%;
  margin-top: 16px;
  border-collapse: collapse;
}

th,
td {
  padding: 10px;
  border-bottom: 1px solid #c6f6d5;
  text-align: left;
}
</style>