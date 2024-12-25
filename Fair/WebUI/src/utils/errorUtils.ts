import { AxiosError, AxiosResponse } from "axios"
import { ErrorResponse, useRouteError } from "react-router-dom"

import { AppError } from "types"

export const fromAxiosResponse = (
  response: AxiosResponse<{ message: string; errorCode: number; stackTrace: string }>,
  pathname: string,
): AppError => {
  return {
    name: "AppError",
    message: response.data.message,
    status: response.status,
    errorCode: response.data.errorCode,
    stackTrace: response.data.stackTrace,
    pathname,
  }
}

export const fromAxiosError = (
  error: AxiosError<{ message: string; errorCode: number; stackTrace: string }>,
  pathname: string,
): AppError => {
  return {
    name: "AppError",
    status: error.status,
    message: error.message,
    errorCode: error.code,
    stackTrace: error.stack,
    pathname,
  }
}

export const fromErrorResponse = (error: ErrorResponse, pathname: string): AppError => {
  return {
    name: Error.name,
    status: error.status,
    message: error.data,
    pathname,
  }
}

export const fromError = (error: Error, pathname: string): AppError => {
  return {
    name: error.name,
    message: error.message,
    stackTrace: error.stack,
    pathname,
  }
}

function isErrorResponse(object: any): object is ErrorResponse {
  return "status" in object && "data" in object
}

export const useAppError = (): AppError => {
  const error: any = useRouteError()

  if (error?.name === "AppError") {
    return error
  }

  const pathname = window.location.pathname
  return isErrorResponse(error) ? fromErrorResponse(error, pathname) : fromError(error, pathname)
}
