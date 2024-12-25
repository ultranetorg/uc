import { useDocumentTitle } from "usehooks-ts"

import { UnknownError, NotFoundError } from "ui/components"
import { useAppError } from "utils"

export const ErrorPage = () => {
  const error = useAppError()
  const title = error?.status !== 404 ? "Error" : "Page Not Found"
  useDocumentTitle(title)

  if (error?.status === 404) {
    return (
      <div className="flex h-full w-full items-center justify-center text-sm">
        <NotFoundError />
      </div>
    )
  }

  return (
    <div className="flex h-full w-full items-center justify-center text-sm">
      <UnknownError />
    </div>
  )
}
