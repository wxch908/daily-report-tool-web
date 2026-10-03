<script setup lang="ts">
import type { DailyReportItem } from '../types/dailyReport'

defineProps<{
  items: DailyReportItem[]
  deletingItemId: number | null
}>()

const emit = defineEmits<{
  edit: [item: DailyReportItem]
  delete: [item: DailyReportItem]
}>()

function formatHours(value: number) {
  return Number.isInteger(value)
    ? `${value}H`
    : `${value.toFixed(1)}H`
}
</script>

<template>
  <el-empty
    v-if="items.length === 0"
    description="当前日报还没有工作项"
  />

  <el-table
    v-else
    :data="items"
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

    <el-table-column
      label="操作"
      width="150"
      fixed="right"
    >
      <template #default="scope">
        <el-button
          type="primary"
          link
          @click="emit('edit', scope.row as DailyReportItem)"
        >
          编辑
        </el-button>

        <el-button
          type="danger"
          link
          :loading="deletingItemId === scope.row.id"
          @click="emit('delete', scope.row as DailyReportItem)"
        >
          删除
        </el-button>
      </template>
    </el-table-column>
  </el-table>
</template>