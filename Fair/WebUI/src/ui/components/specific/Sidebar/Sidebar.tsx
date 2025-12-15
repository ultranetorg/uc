import { memo, useCallback } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useAccountsContext, useSiteContext } from "app"
import { useTransactMutation } from "entities/mcv"
import { FavoriteSiteChange, PropsWithClassName, SiteBase } from "types"
import { SitesList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"
import { showToast } from "utils"

import { AllSitesButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sites")

  const { site } = useSiteContext()
  const { currentAccount, refetchAccount } = useAccountsContext()
  const { mutate: transact } = useTransactMutation()

  const transactOperation = useCallback(
    ({ id, title }: SiteBase, action: boolean) => {
      const operation = new FavoriteSiteChange(id, action)
      transact(
        { operation },
        {
          onSuccess: () => {
            setTimeout(() => {
              showToast(
                t(action ? t("toast:favoriteAdded", { site: title }) : t("toast:favoriteRemoved", { site: title })),
              )
              refetchAccount()
            }, 1500)
          },
        },
      )
    },
    [refetchAccount, t, transact],
  )

  const handleFavoriteAdd = useCallback((item: SiteBase) => transactOperation(item, true), [transactOperation])

  const handleFavoriteRemove = useCallback((item: SiteBase) => transactOperation(item, false), [transactOperation])

  return (
    <div className={twMerge("flex w-65 min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex grow flex-col gap-8 p-2">
        <Link to="/">
          <AllSitesButton title={t("allSites")} />
        </Link>
        {site && (
          <SitesList
            title={t("currentSite")}
            items={[site]}
            emptyStateMessage={t("emptySitesList")}
            onFavoriteClick={handleFavoriteAdd}
          />
        )}
        <SitesList
          title={t("starredSites")}
          items={currentAccount?.favoriteSites}
          emptyStateMessage={t("emptySitesList")}
          onFavoriteClick={handleFavoriteRemove}
          isStarred={true}
        />
      </div>
      <CurrentAccount />
    </div>
  )
})
