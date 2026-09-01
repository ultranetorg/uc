import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"

import { SvgFair } from "assets"
import { AccountPanel } from "ui/components/specific"
import { routes } from "utils"

export const Header = () => {
  const { t } = useTranslation()

  return (
    <div className="h-16 bg-gray-800">
      <div className="mx-auto flex h-full max-w-[1440px] items-center justify-between px-9 py-2">
        <Link to={routes.home()}>
          <SvgFair className="fill-white" title={t("common:fair")} />
        </Link>
        <AccountPanel />
      </div>
    </div>
  )
}
