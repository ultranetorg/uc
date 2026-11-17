import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"

import { SvgFairLogo4Xl, SvgList } from "assets"
import { AccountPanel, FavoriteStores, useSidebarDrawer } from "ui/components/specific"
import { routes } from "utils"

export const Header = () => {
  const { t } = useTranslation()
  const { hasSidebar, isOpen, toggle } = useSidebarDrawer()

  return (
    <div className="h-16 bg-gray-800">
      <div className="mx-auto flex h-full max-w-360 items-center gap-3 px-4 py-2 sm:px-6 lg:px-9">
        <div className="flex h-full flex-1 shrink-0 items-center justify-start gap-3">
          {hasSidebar && (
            <button
              type="button"
              className="flex size-10 shrink-0 cursor-pointer items-center justify-center rounded-sm hover:bg-gray-600 lg:hidden"
              aria-expanded={isOpen}
              aria-label={t("common:menu")}
              title={t("common:menu")}
              onClick={toggle}
            >
              <SvgList className="stroke-white" />
            </button>
          )}
          <Link to={routes.home()}>
            <SvgFairLogo4Xl className="fill-white" title={t("common:fair")} />
          </Link>
        </div>
        <FavoriteStores />
        <div className="flex h-full min-w-0 justify-end md:flex-1 md:basis-0">
          <AccountPanel />
        </div>
      </div>
    </div>
  )
}
