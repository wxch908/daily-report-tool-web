<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'

interface DailyReport {
  id: number
  reportDate: string
  createdAt: string
  updatedAt: string
}

interface DailyReportItem {
  id: number
  dailyReportId: number
  description: string
  hours: number
  sortOrder: number
  createdAt: string
  updatedAt: string
}

interface DailyReportDetail extends DailyReport {
  totalHours: number
  items: DailyReportItem[]
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

function formatHours(value: number) {
  return Number.isInteger(value)
    ? `${value}H`
    : `${value.toFixed(1)}H`
}

function getErrorMessage(error: unknown) {
  return error instanceof Error
    ? error.message
    : '发生了未知错误'
}

async function getApiErrorMessage(
  response: Response,
  fallback: string,
) {
  try {
    const result = (await response.json()) as {
      message?: string
    }

    return result.message ?? fallback
  } catch {
    return fallback
  }
}

const reports = ref<DailyReport[]>([])
const selectedReport = ref<DailyReportDetail | null>(null)

const selectedDate = ref(getTodayText())
const itemDescription = ref('')
const itemHours = ref(1)

const errorMessage = ref('')
const isLoading = ref(false)
const isCreating = ref(false)
const isLoadingDetail = ref(false)
const isAddingItem = ref(false)

const hourOptions = Array.from(
  { length: 16 },
  (_, index) => (index + 1) * 0.5,
)

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

async function loadReportDetail(id: number) {
  isLoadingDetail.value = true
  errorMessage.value = ''

  try {
    const response = await fetch(`/api/daily-reports/${id}`)

    if (!response.ok) {
      const message = await getApiErrorMessage(
        response,
        `详情查询失败：HTTP ${response.status}`,
      )

      throw new Error(message)
    }

    selectedReport.value =
      (await response.json()) as DailyReportDetail
  } catch (error: unknown) {
    selectedReport.value = null
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isLoadingDetail.value = false
  }
}

async function createReport() {
  if (!selectedDate.value) {
    ElMessage.warning('请先选择日报日期')
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

    if (!response.ok) {
      const message = await getApiErrorMessage(
        response,
        `创建失败：HTTP ${response.status}`,
      )

      throw new Error(message)
    }

    const createdReport =
      (await response.json()) as DailyReport

    await loadReports()
    await loadReportDetail(createdReport.id)

    ElMessage.success('日报创建成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isCreating.value = false
  }
}

async function createItem() {
  if (!selectedReport.value) {
    ElMessage.warning('请先选择一份日报')
    return
  }

  const description = itemDescription.value.trim()

  if (!description) {
    ElMessage.warning('请输入工作内容')
    return
  }

  const reportId = selectedReport.value.id

  isAddingItem.value = true
  errorMessage.value = ''

  try {
    const response = await fetch(
      `/api/daily-reports/${reportId}/items`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          description,
          hours: itemHours.value,
        }),
      },
    )

    if (!response.ok) {
      const message = await getApiErrorMessage(
        response,
        `添加失败：HTTP ${response.status}`,
      )

      throw new Error(message)
    }

    itemDescription.value = ''
    itemHours.value = 1

    await loadReportDetail(reportId)
    await loadReports()

    ElMessage.success('工作项添加成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isAddingItem.value = false
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
          创建日报并记录每天的工作内容
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

    <el-alert
      v-if="errorMessage"
      class="feedback"
      :title="errorMessage"
      type="error"
      show-icon
      :closable="false"
    />

    <el-card class="list-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>日报列表</span>

          <el-tag type="success" effect="light">
            {{ reports.length }} 条
          </el-tag>
        </div>
      </template>

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

        <el-table-column
          label="操作"
          width="120"
          fixed="right"
        >
          <template #default="scope">
            <el-button
              type="primary"
              link
              @click="loadReportDetail(scope.row.id)"
            >
              查看详情
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card
      v-if="isLoadingDetail"
      class="detail-card"
      shadow="never"
    >
      <el-skeleton :rows="5" animated />
    </el-card>

    <el-card
      v-else-if="selectedReport"
      class="detail-card"
      shadow="never"
    >
      <template #header>
        <div class="card-header">
          <span>
            {{ formatDate(selectedReport.reportDate) }}
            日报详情
          </span>

          <el-tag type="primary" effect="dark">
            合计 {{ formatHours(selectedReport.totalHours) }}
          </el-tag>
        </div>
      </template>

      <el-form
        class="item-form"
        inline
        @submit.prevent
      >
        <el-form-item
          class="description-item"
          label="工作内容"
        >
          <el-input
            v-model="itemDescription"
            maxlength="500"
            show-word-limit
            placeholder="请输入本项工作内容"
          />
        </el-form-item>

        <el-form-item label="工时">
          <el-select
            v-model="itemHours"
            class="hours-select"
          >
            <el-option
              v-for="hours in hourOptions"
              :key="hours"
              :label="formatHours(hours)"
              :value="hours"
            />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            :loading="isAddingItem"
            @click="createItem"
          >
            添加工作项
          </el-button>
        </el-form-item>
      </el-form>

      <el-empty
        v-if="selectedReport.items.length === 0"
        description="当前日报还没有工作项"
      />

      <el-table
        v-else
        :data="selectedReport.items"
        border
        stripe
      >
        <el-table-column
          prop="sortOrder"
          label="序号"
          width="80"
        />

        <el-table-column
          prop="description"
          label="工作内容"
          min-width="320"
        />

        <el-table-column
          label="工时"
          width="120"
        >
          <template #default="scope">
            {{ formatHours(scope.row.hours) }}
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

.create-card,
.list-card,
.feedback {
  margin-bottom: 20px;
}

.detail-card {
  margin-top: 20px;
}

.create-form :deep(.el-form-item) {
  margin-bottom: 0;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  font-weight: 600;
}

.item-form {
  display: flex;
  align-items: flex-start;
  margin-bottom: 18px;
}

.description-item {
  flex: 1;
}

.description-item :deep(.el-form-item__content) {
  min-width: 280px;
}

.description-item :deep(.el-input) {
  width: 100%;
}

.hours-select {
  width: 120px;
}

@media (max-width: 640px) {
  .page-shell {
    width: min(100% - 20px, 1100px);
    padding: 24px 0;
  }

  .create-form,
  .item-form {
    display: block;
  }

  .create-form :deep(.el-form-item),
  .item-form :deep(.el-form-item) {
    display: flex;
    margin-right: 0;
    margin-bottom: 12px;
  }

  .description-item :deep(.el-form-item__content) {
    min-width: 0;
  }
}
</style>