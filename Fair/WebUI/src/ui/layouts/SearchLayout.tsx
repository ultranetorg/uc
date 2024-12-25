import { Outlet } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { SearchDropdown } from "ui/components"

export const SearchLayout = () => {
  const { t } = useTranslation("layout")

  return (
    <div className="mt-10 flex flex-col gap-5">
      <SearchDropdown placeholder={t("searchPlaceholder")} />
      <Outlet />
    </div>
  )
}
