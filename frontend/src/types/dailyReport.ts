export interface DailyReport {
  id: number
  reportDate: string
  createdAt: string
  updatedAt: string
}

export interface DailyReportItem {
  id: number
  dailyReportId: number
  description: string
  hours: number
  sortOrder: number
  createdAt: string
  updatedAt: string
}

export interface DailyReportDetail extends DailyReport {
  totalHours: number
  items: DailyReportItem[]
}