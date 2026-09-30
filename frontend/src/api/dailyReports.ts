import type {
  DailyReport,
  DailyReportDetail,
} from '../types/dailyReport'

async function getApiErrorMessage(
  response: Response,
  fallback: string,
): Promise<string> {
  try {
    const result = (await response.json()) as {
      message?: string
    }

    return result.message ?? fallback
  } catch {
    return fallback
  }
}

export async function getDailyReports(): Promise<DailyReport[]> {
  const response = await fetch('/api/daily-reports')

  if (!response.ok) {
    const message = await getApiErrorMessage(
      response,
      `查询失败：HTTP ${response.status}`,
    )

    throw new Error(message)
  }

  return (await response.json()) as DailyReport[]
}

export async function getDailyReport(
  id: number,
): Promise<DailyReportDetail> {
  const response = await fetch(`/api/daily-reports/${id}`)

  if (!response.ok) {
    const message = await getApiErrorMessage(
      response,
      `详情查询失败：HTTP ${response.status}`,
    )

    throw new Error(message)
  }

  return (await response.json()) as DailyReportDetail
}