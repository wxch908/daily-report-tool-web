<script setup lang="ts">
import DailyReportItemEditDialog from './components/DailyReportItemEditDialog.vue'
import DailyReportItemTable from './components/DailyReportItemTable.vue'
import DailyReportList from './components/DailyReportList.vue'
import { onMounted, ref } from 'vue'
import { ElMessage,ElMessageBox, } from 'element-plus'
import type {
  DailyReport,
  DailyReportItem,
  DailyReportDetail,
} from './types/dailyReport'

import {
  getDailyReports,
  getDailyReport,
  createDailyReport,
  createDailyReportItem,
  updateDailyReportItem,
  deleteDailyReportItem,
  deleteDailyReport,
} from './api/dailyReports'

import {
  getTodayText,
  formatDate,
  formatHours,
} from './utils/format'


function getErrorMessage(error: unknown) {
  return error instanceof Error
    ? error.message
    : '发生了未知错误'
}


const reports = ref<DailyReport[]>([])
const selectedReport = ref<DailyReportDetail | null>(null)
const filterDateRange = ref<[string, string] | null>(null)
const appliedDateRange = ref<[string, string] | null>(null)

const selectedDate = ref(getTodayText())
const itemDescription = ref('')
const itemHours = ref(1)

const editingItem = ref<DailyReportItem | null>(null)
const editDescription = ref('')
const editHours = ref(1)
const isEditDialogVisible = ref(false)

const errorMessage = ref('')
const isLoading = ref(false)
const isCreating = ref(false)
const isLoadingDetail = ref(false)
const isAddingItem = ref(false)
const isUpdatingItem = ref(false)
const deletingItemId = ref<number | null>(null)
const deletingReportId = ref<number | null>(null)

const hourOptions = Array.from(
  { length: 16 },
  (_, index) => (index + 1) * 0.5,
)

async function loadReports() {
  isLoading.value = true
  errorMessage.value = ''

  try {
    const range = appliedDateRange.value

    reports.value = await getDailyReports(
      range?.[0],
      range?.[1],
    )
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isLoading.value = false
  }
}

async function searchReports() {
  const range = filterDateRange.value

  if (range && range[0] > range[1]) {
    ElMessage.warning('开始日期不能晚于结束日期')
    return
  }

  appliedDateRange.value = range
    ? [range[0], range[1]]
    : null

  clearReportDetail()
  await loadReports()
}

async function resetReportFilter() {
  filterDateRange.value = null
  appliedDateRange.value = null

  clearReportDetail()
  await loadReports()
}

function clearReportDetail() {
  selectedReport.value = null
  itemDescription.value = ''
  itemHours.value = 1
  isEditDialogVisible.value = false
}

async function loadReportDetail(id: number) {
  isLoadingDetail.value = true
  errorMessage.value = ''

  try {
    selectedReport.value = await getDailyReport(id)
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
    const createdReport = await createDailyReport(
      selectedDate.value,
    )

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
    await createDailyReportItem(reportId, {
      description,
      hours: itemHours.value,
    })

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

function openEditDialog(item: DailyReportItem) {
  editingItem.value = item
  editDescription.value = item.description
  editHours.value = item.hours
  isEditDialogVisible.value = true
}

function resetEditDialog() {
  editingItem.value = null
  editDescription.value = ''
  editHours.value = 1
}

async function updateItem() {
  if (!selectedReport.value || !editingItem.value) {
    return
  }

  const description = editDescription.value.trim()

  if (!description) {
    ElMessage.warning('请输入工作内容')
    return
  }

  const reportId = selectedReport.value.id
  const itemId = editingItem.value.id

  isUpdatingItem.value = true
  errorMessage.value = ''

  try {
    await updateDailyReportItem(reportId, itemId, {
      description,
      hours: editHours.value,
    })

    isEditDialogVisible.value = false

    await loadReportDetail(reportId)
    await loadReports()

    ElMessage.success('工作项修改成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    isUpdatingItem.value = false
  }
}

async function deleteItem(item: DailyReportItem) {
  if (!selectedReport.value) {
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定删除第 ${item.sortOrder} 项工作内容吗？删除后序号会自动重排。`,
      '删除工作项',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
  } catch {
    return
  }

  const reportId = selectedReport.value.id

  deletingItemId.value = item.id
  errorMessage.value = ''

  try {
    await deleteDailyReportItem(reportId, item.id)

    await loadReportDetail(reportId)
    await loadReports()

    ElMessage.success('工作项删除成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    deletingItemId.value = null
  }
}

async function deleteReport(report: DailyReport) {
  if (deletingReportId.value !== null) {
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定删除 ${formatDate(report.reportDate)} 的日报吗？其中的全部工作项也会被删除，且无法撤销。`,
      '删除日报',
      {
        confirmButtonText: '删除',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
  } catch {
    return
  }

  deletingReportId.value = report.id
  errorMessage.value = ''

  try {
    await deleteDailyReport(report.id)

    reports.value = reports.value.filter(
      item => item.id !== report.id,
    )

    if (selectedReport.value?.id === report.id) {
      selectedReport.value = null
      itemDescription.value = ''
      itemHours.value = 1
      isEditDialogVisible.value = false
    }

    ElMessage.success('日报删除成功')
  } catch (error: unknown) {
    errorMessage.value = getErrorMessage(error)
    ElMessage.error(errorMessage.value)
  } finally {
    deletingReportId.value = null
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

    <el-card class="filter-card" shadow="never">
      <el-form
        class="filter-form"
        inline
        @submit.prevent
      >
        <el-form-item label="日期范围">
          <el-date-picker
            v-model="filterDateRange"
            type="daterange"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
            range-separator="至"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            :disabled="isLoading"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            :loading="isLoading"
            @click="searchReports"
          >
            查询
          </el-button>

          <el-button
            :disabled="isLoading"
            @click="resetReportFilter"
          >
            重置
          </el-button>
        </el-form-item>
      </el-form>

      <p class="filter-summary">
        {{
          appliedDateRange
            ? `当前查询范围：${appliedDateRange[0]} 至 ${appliedDateRange[1]}`
            : '当前查询范围：全部日期'
        }}
      </p>
    </el-card>

    <DailyReportList
        :reports="reports"
        :loading="isLoading"
        :deleting-report-id="deletingReportId"
        :empty-description="
          appliedDateRange
            ? '所选日期范围内没有日报'
            : '还没有日报，请创建第一条'
        "
        @view="loadReportDetail"
        @delete="deleteReport"
    />

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

      <DailyReportItemTable
        :items="selectedReport.items"
        :deleting-item-id="deletingItemId"
        @edit="openEditDialog"
        @delete="deleteItem"
      />
    </el-card>

    <DailyReportItemEditDialog
      v-model="isEditDialogVisible"
      v-model:description="editDescription"
      v-model:hours="editHours"
      :saving="isUpdatingItem"
      :hour-options="hourOptions"
      @save="updateItem"
      @closed="resetEditDialog"
    />

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