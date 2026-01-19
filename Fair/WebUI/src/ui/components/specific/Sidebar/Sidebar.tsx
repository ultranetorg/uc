import { memo, useCallback, useState } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useSiteContext, useUserContext } from "app"
import { useTransactMutationWithStatus } from "entities/node"
import { FavoriteSiteChange, PropsWithClassName, SiteBase } from "types"
import { SitesList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"
import { showToast } from "utils"

import { AllSitesButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sites")

  const { site } = useSiteContext()
  const { user, refetch } = useUserContext()
  const [showPending, setShowPending] = useState(false)
  const [disabledIds, setDisabledIds] = useState<string[]>([])
  const { mutate } = useTransactMutationWithStatus()

  const transactOperation = useCallback(
    ({ id, title }: SiteBase, action: boolean) => {
      if (action) {
        setShowPending(true)
      }

      setDisabledIds(prev => [...prev, id])

      const operation = new FavoriteSiteChange(id, action)
      mutate(operation, {
        onSuccess: () => {
          const message = action
            ? t("toast:favoriteAdded", { site: title })
            : t("toast:favoriteRemoved", { site: title })
          showToast(message, "success")
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
        onSettled: () => {
          setDisabledIds(() => [])
          setShowPending(false)
          refetch()
        },
      })
    },
    [mutate, refetch, t],
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
            disabledFavorite={(!user || user?.favoriteSites?.some(s => s.id === site.id)) ?? false}
            title={t("currentSite")}
            items={[site]}
            emptyStateMessage={t("emptySitesList")}
            onFavoriteClick={handleFavoriteAdd}
            disabledIds={disabledIds}
          />
        )}
        <SitesList
          title={t("starredSites")}
          items={user?.favoriteSites}
          emptyStateMessage={t("emptySitesList")}
          onFavoriteClick={handleFavoriteRemove}
          isStarred={true}
          disabledIds={disabledIds}
          showPending={showPending}
        />
      </div>
      <CurrentAccount />
    </div>
  )
})
