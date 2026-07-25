import { useCallback } from "react"
import { useTranslation } from "react-i18next"

import { useStoreTitle } from "hooks"
import { ButtonPrimary, ErrorInfo } from "ui/components"

export const UnknownErrorPage = () => {
  const { t } = useTranslation("error")

  useStoreTitle("Error - Something Went Wrong")

  const handleClick = useCallback(() => window.location.reload(), [])

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-8">
      <ErrorInfo
        title="Something went wrong"
        description="An unexpected error occurred. Try refreshing the page or contact us if the problem persists."
      />
      <ButtonPrimary className="w-33" label={t("refreshPage")} onClick={handleClick} />
    </div>
  )
}
