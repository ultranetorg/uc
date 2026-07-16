import { useCallback } from "react"
import { useTranslation } from "react-i18next"

import { useSiteTitle } from "hooks"
import { ButtonPrimary, ErrorInfo } from "ui/components"

export const ServerErrorPage = () => {
  const { t } = useTranslation("error")

  useSiteTitle("Error - Server Error")

  const handleClick = useCallback(() => window.location.reload(), [])

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-8">
      <ErrorInfo
        title="Server error"
        description="Something went wrong on our end. Please try again in a few minutes."
      />
      <ButtonPrimary className="w-33" label={t("refreshPage")} onClick={handleClick} />
    </div>
  )
}
