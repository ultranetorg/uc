import { memo, useCallback, useState } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useUserContext } from "app"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { FavoriteStoreChange, PropsWithClassName, StoreBase } from "types"
import { StoresList } from "ui/components/sidebar"
import { CurrentAccount } from "ui/components/specific"
import { routes, showToast } from "utils"

import { AllStoresButton } from "./components"

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("storesPage")

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

      const operation = new FavoriteStoreChange(id, action)
      mutate(operation, {
        onSuccess: () => {
          const message = action
            ? t("toast:favoriteAdded", { store: title })
            : t("toast:favoriteRemoved", { store: title })
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
          <AllStoresButton title={t("allStores")} />
        </Link>
        {store && (
          <StoresList
            disabledFavorite={(!user || user?.favoriteStores?.some(s => s.id === store.id)) ?? false}
            title={t("currentStore")}
            items={[store]}
            emptyStateMessage={t("emptyStoresList")}
            onFavoriteClick={handleFavoriteAdd}
            disabledIds={disabledIds}
          />
        )}
        <StoresList
          title={t("starredStores")}
          items={user?.favoriteStores}
          emptyStateMessage={t("emptyStoresList")}
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
