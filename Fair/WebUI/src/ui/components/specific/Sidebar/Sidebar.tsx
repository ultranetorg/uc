import { memo, useCallback, useState } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useUserContext } from "app"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { FavoriteSiteChange, PropsWithClassName, StoreBase } from "types"
import { StoresList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"
import { routes, showToast } from "utils"

import { AllSitesButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("sites")

  const { store } = useStoreContext()
  const { user, refetch } = useUserContext()
  const [showPending, setShowPending] = useState(false)
  const [disabledIds, setDisabledIds] = useState<string[]>([])
  const { mutate } = useTransactMutationWithStatus()

  const transactOperation = useCallback(
    ({ id, title }: StoreBase, action: boolean) => {
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

  const handleFavoriteAdd = useCallback((item: StoreBase) => transactOperation(item, true), [transactOperation])

  const handleFavoriteRemove = useCallback((item: StoreBase) => transactOperation(item, false), [transactOperation])

  return (
    <div className={twMerge("flex w-65 min-w-65 flex-col gap-6 p-2", className)}>
      <div className="flex grow flex-col gap-8 p-2">
        <Link to={routes.home()}>
          <AllSitesButton title={t("allSites")} />
        </Link>
        {store && (
          <StoresList
            disabledFavorite={(!user || user?.favoriteStores?.some(s => s.id === store.id)) ?? false}
            title={t("currentSite")}
            items={[store]}
            emptyStateMessage={t("emptySitesList")}
            onFavoriteClick={handleFavoriteAdd}
            disabledIds={disabledIds}
          />
        )}
        <StoresList
          title={t("starredSites")}
          items={user?.favoriteStores}
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
