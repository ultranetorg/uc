import { ErrorResult } from "types"

export class ApiError extends Error {
  readonly status: number
  readonly statusText: string
  readonly body?: ErrorResult

  constructor(status: number, statusText: string, body?: ErrorResult) {
    super(body?.message ?? `API request failed with status ${status} ${statusText}`.trim())
    this.name = "ApiError"
    this.status = status
    this.statusText = statusText
    this.body = body
  }
}
