import { isRouteErrorResponse, useLocation, useRouteError } from "react-router-dom"

import { ApiError } from "api"
import { NotFoundPage, ServerErrorPage, UnknownErrorPage } from "ui/pages"
import { ENTITY_PREFIXES } from "utils"

import { MaybeFullscreen } from "./route"

const isFullscreenPage = (pathname: string): boolean => {
  const [, appEntity = "", subEntity = ""] = pathname.split("/")

  if (appEntity.startsWith(ENTITY_PREFIXES.authorId)) return true

  return subEntity.startsWith(ENTITY_PREFIXES.userId) || subEntity.startsWith(ENTITY_PREFIXES.publisherId)
}

export const RouteErrorBoundary = () => {
  const error = useRouteError()
  const { pathname } = useLocation()

  const isNotFound =
    (isRouteErrorResponse(error) && error.status === 404) || (error instanceof ApiError && error.status === 404)
  const isServerError = error instanceof ApiError && error.status >= 500

  const ErrorPage = isNotFound ? NotFoundPage : isServerError ? ServerErrorPage : UnknownErrorPage

  return (
    <MaybeFullscreen showFullscreen={isFullscreenPage(pathname)}>
      <ErrorPage />
    </MaybeFullscreen>
  )
}
