import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"

import { useStoreTitle } from "hooks"
import { ButtonPrimary, ErrorInfo } from "ui/components"
import { routes } from "utils"

export const NotFoundPage = () => {
  const { t } = useTranslation("error")

  useStoreTitle("Error - Page Not Found")

  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-8">
      <ErrorInfo
        title="Page not found"
        description="The page you’re looking for doesn’t exist or is no longer available."
      />
      <Link to={routes.home()}>
        <ButtonPrimary className="w-33" label={t("backToHome")} />
      </Link>
    </div>
  )
}
