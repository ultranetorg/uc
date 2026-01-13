export type StatusResult<T extends object> = {
  data?: T
} & Pick<Response, "ok" | "status">
