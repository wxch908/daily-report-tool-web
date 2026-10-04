<script setup lang="ts">
import type { DailyReport } from '../types/dailyReport'

import {
  formatDate,
  formatDateTime,
} from '../utils/format'

defineProps<{
  reports: DailyReport[]
  loading: boolean
  deletingReportId: number | null
}>()

const emit = defineEmits<{
  view: [id: number]
  delete: [report: DailyReport]
}>()

</script>

<template>
  <el-card class="report-list" shadow="never">
    <template #header>
      <div class="card-header">
        <span>日报列表</span>

        <el-tag type="success" effect="light">
          {{ reports.length }} 条
        </el-tag>
      </div>
    </template>

    <el-skeleton
      v-if="loading"
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
        width="180"
        fixed="right"
      >
        <template #default="scope">
          <el-button
            type="primary"
            link
            :disabled="deletingReportId !== null"
            @click="emit('view', scope.row.id)"
          >
            查看详情
          </el-button>

          <el-button
            type="danger"
            link
            :loading="deletingReportId === scope.row.id"
            :disabled="
              deletingReportId !== null &&
              deletingReportId !== scope.row.id
            "
            @click="emit('delete', scope.row as DailyReport)"
          >
            删除日报
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </el-card>
</template>

<style scoped>
.report-list {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  font-weight: 600;
}
</style>