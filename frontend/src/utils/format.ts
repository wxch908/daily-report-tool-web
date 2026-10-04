export function getTodayText(): string {
  const today = new Date()
  const year = today.getFullYear()
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day = String(today.getDate()).padStart(2, '0')

  return `${year}-${month}-${day}`
}

export function formatDate(value: string): string {
  return value.slice(0, 10)
}

export function formatDateTime(value: string): string {
  return new Date(value).toLocaleString()
}

export function formatHours(value: number): string {
  return Number.isInteger(value)
    ? `${value}H`
    : `${value.toFixed(1)}H`
}