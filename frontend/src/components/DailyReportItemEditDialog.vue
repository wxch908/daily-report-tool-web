<script setup lang="ts">
const visible = defineModel<boolean>({
  required: true,
})

const description = defineModel<string>('description', {
  required: true,
})

const hours = defineModel<number>('hours', {
  required: true,
})

defineProps<{
  saving: boolean
  hourOptions: number[]
}>()

const emit = defineEmits<{
  save: []
  closed: []
}>()

function formatHours(value: number) {
  return Number.isInteger(value)
    ? `${value}H`
    : `${value.toFixed(1)}H`
}
</script>

<template>
  <el-dialog
    v-model="visible"
    title="编辑工作项"
    width="min(520px, 90%)"
    :close-on-click-modal="false"
    :close-on-press-escape="!saving"
    :show-close="!saving"
    @closed="emit('closed')"
  >
    <el-form
      label-width="80px"
      @submit.prevent
    >
      <el-form-item label="工作内容">
        <el-input
          v-model="description"
          type="textarea"
          :rows="4"
          maxlength="500"
          show-word-limit
          placeholder="请输入工作内容"
          :disabled="saving"
        />
      </el-form-item>

      <el-form-item label="工时">
        <el-select
          v-model="hours"
          class="hours-select"
          :disabled="saving"
        >
          <el-option
            v-for="option in hourOptions"
            :key="option"
            :label="formatHours(option)"
            :value="option"
          />
        </el-select>
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button
        :disabled="saving"
        @click="visible = false"
      >
        取消
      </el-button>

      <el-button
        type="primary"
        :loading="saving"
        @click="emit('save')"
      >
        保存
      </el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.hours-select {
  width: 120px;
}
</style>