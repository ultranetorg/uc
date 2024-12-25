import { useTranslation } from "react-i18next"
import { Outlet } from "react-router-dom"

import { usePageId } from "hooks"

export const PageLayout = () => {
  const pageId = usePageId()
  const { t } = useTranslation(pageId)

  return (
    <>
      <div className="mb-6 mt-10 select-none overflow-hidden text-ellipsis whitespace-nowrap font-sans-medium text-2xl leading-6 text-white">
        {t("pageTitle")}
      </div>
      <Outlet />
    </>
  )
}
