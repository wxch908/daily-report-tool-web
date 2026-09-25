<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'

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

function formatDate(value: string) {
  return value.slice(0, 10)
}
function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}
function getErrorMessage(error: unknown) {
  return error instanceof Error ? error.message : '发生了未知错误'
}

const reports = ref<DailyReport[]>([])
const selectedDate = ref(getTodayText())
const errorMessage = ref('')
const isLoading = ref(false)
const isCreating = ref(false)

async function loadReports() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await fetch('/api/daily-reports')

    if (!response.ok) {
      throw new Error(`查询失败：HTTP ${response.status}`)
    }

    reports.value = (await response.json()) as DailyReport[]
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
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
    ElMessage.success('日报创建成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isCreating.value = false
  }
}

onMounted(loadReports)
</script>

<template>
  <main class="page-shell">
    <header class="page-header">
      <div>
        <p class="eyebrow">DAILY REPORT</p>
        <h1>工作日报</h1>
        <p class="subtitle">
          创建并管理每日工作记录
        </p>
      </div>
    </header>

    <el-card class="create-card" shadow="never">
      <el-form class="create-form" inline>
        <el-form-item label="日报日期">
          <el-date-picker
            v-model="selectedDate"
            type="date"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            placeholder="选择日期"
            :clearable="false"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            :loading="isCreating"
            @click="createReport"
          >
            新建日报
          </el-button>

          <el-button
            :loading="isLoading"
            @click="loadReports"
          >
            刷新
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card class="list-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>日报列表</span>

          <el-tag type="success" effect="light">
            {{ reports.length }} 条
          </el-tag>
        </div>
      </template>

      <el-alert
        v-if="errorMessage"
        class="feedback"
        :title="errorMessage"
        type="error"
        show-icon
        :closable="false"
      />

      <el-skeleton
        v-if="isLoading"
        :rows="4"
        animated
      />

      <el-empty
        v-else-if="reports.length === 0"
        description="还没有日报，请创建第一条"
      />

      <el-table
        v-else
        :data="reports"
        stripe
        border
      >
        <el-table-column
          prop="id"
          label="ID"
          width="80"
        />

        <el-table-column
          label="日报日期"
          min-width="160"
        >
          <template #default="scope">
            {{ formatDate(scope.row.reportDate) }}
          </template>
        </el-table-column>

        <el-table-column
          label="创建时间"
          min-width="220"
        >
          <template #default="scope">
            {{ formatDateTime(scope.row.createdAt) }}
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </main>
</template>

<style scoped>
.page-shell {
  width: min(1100px, calc(100% - 32px));
  margin: 0 auto;
  padding: 48px 0;
}

.page-header {
  margin-bottom: 24px;
}

.eyebrow {
  margin: 0 0 8px;
  color: #409eff;
  font-size: 13px;
  font-weight: 700;
  letter-spacing: 0.12em;
}

h1 {
  margin: 0;
  color: #303133;
  font-size: 32px;
}

.subtitle {
  margin: 8px 0 0;
  color: #909399;
}

.create-card {
  margin-bottom: 20px;
}

.create-form :deep(.el-form-item) {
  margin-bottom: 0;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
}

.feedback {
  margin-bottom: 16px;
}

@media (max-width: 640px) {
  .page-shell {
    width: min(100% - 20px, 1100px);
    padding: 24px 0;
  }

  .create-form :deep(.el-form-item) {
    display: flex;
    margin-right: 0;
    margin-bottom: 12px;
  }
}
</style>