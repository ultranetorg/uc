export type AppError = {
  name: string
  message: string
  status?: number
  errorCode?: number | string
  stackTrace?: string
  pathname?: string
}
