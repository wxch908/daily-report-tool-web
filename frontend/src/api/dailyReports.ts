import type {
  DailyReport,
  DailyReportItem,
  DailyReportDetail,
} from '../types/dailyReport'

interface DailyReportItemInput {
  description: string
  hours: number
}

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

async function sendRequest(
  url: string,
  options: RequestInit,
  errorText: string,
): Promise<Response> {
  const response = await fetch(url, options)

  if (!response.ok) {
    const message = await getApiErrorMessage(
      response,
      `${errorText}：HTTP ${response.status}`,
    )

    throw new Error(message)
  }

  return response
}

export async function getDailyReports(
  startDate?: string,
  endDate?: string,
): Promise<DailyReport[]> {
  const params = new URLSearchParams()

  if (startDate) {
    params.set('startDate', startDate)
  }

  if (endDate) {
    params.set('endDate', endDate)
  }

  const queryString = params.toString()

  const url = queryString
    ? `/api/daily-reports?${queryString}`
    : '/api/daily-reports'

  const response = await sendRequest(
    url,
    { method: 'GET' },
    '查询失败',
  )

  return (await response.json()) as DailyReport[]
}

export async function getDailyReport(
  id: number,
): Promise<DailyReportDetail> {
  const response = await sendRequest(
    `/api/daily-reports/${id}`,
    { method: 'GET' },
    '详情查询失败',
  )

  return (await response.json()) as DailyReportDetail
}

export async function createDailyReport(
  reportDate: string,
): Promise<DailyReport> {
  const response = await sendRequest(
    '/api/daily-reports',
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ reportDate }),
    },
    '创建失败',
  )

  return (await response.json()) as DailyReport
}

export async function createDailyReportItem(
  reportId: number,
  input: DailyReportItemInput,
): Promise<DailyReportItem> {
  const response = await sendRequest(
    `/api/daily-reports/${reportId}/items`,
    {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    },
    '添加失败',
  )

  return (await response.json()) as DailyReportItem
}

export async function updateDailyReportItem(
  reportId: number,
  itemId: number,
  input: DailyReportItemInput,
): Promise<DailyReportItem> {
  const response = await sendRequest(
    `/api/daily-reports/${reportId}/items/${itemId}`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(input),
    },
    '修改失败',
  )

  return (await response.json()) as DailyReportItem
}

export async function deleteDailyReportItem(
  reportId: number,
  itemId: number,
): Promise<void> {
  await sendRequest(
    `/api/daily-reports/${reportId}/items/${itemId}`,
    { method: 'DELETE' },
    '删除工作项失败',
  )
}

export async function deleteDailyReport(
  id: number,
): Promise<void> {
  await sendRequest(
    `/api/daily-reports/${id}`,
    { method: 'DELETE' },
    '删除日报失败',
  )
}