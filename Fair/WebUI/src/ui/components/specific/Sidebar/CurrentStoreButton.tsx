import { memo, useCallback } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useUserContext } from "app"
import { SvgStar } from "assets"
import { SvgStoreLogo } from "assets/fallback"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { FavoriteStoreChange } from "types"
import { ImageFallback } from "ui/components"
import { buildFileUrl, routes, showToast } from "utils"

export interface CurrentStoreProps {
  storeId: string
  title: string
  logoFileId?: string
  publishersCount: number
}

export const CurrentStore = memo(({ storeId, title, logoFileId, publishersCount }: CurrentStoreProps) => {
  const { t } = useTranslation()

  const { mutate } = useTransactMutationWithStatus()
  const { user, refetch } = useUserContext()

  const transactOperation = useCallback(
    (action: boolean) => {
      if (action) {
        //setShowPending(true)
      }

      //setDisabledIds(prev => [...prev, id])

      const operation = new FavoriteStoreChange(storeId, action)
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
          //setDisabledIds(() => [])
          //setShowPending(false)
          refetch()
        },
      })
    },
    [mutate, refetch, storeId, t, title],
  )

  const handleFavoriteAdd = useCallback(() => transactOperation(true), [transactOperation])

  const handleFavoriteRemove = useCallback(() => transactOperation(false), [transactOperation])

  const isInFavorites = user?.favoriteStores.some(x => x.id === storeId)

  return (
    <div className="flex h-11 items-center gap-3 rounded-lg">
      <Link to={routes.store(storeId)}>
        <div className="shrink-0 overflow-hidden rounded-lg">
          <ImageFallback
            className="size-8"
            src={buildFileUrl(logoFileId)}
            fallback={<SvgStoreLogo className="size-8" />}
          />
        </div>
      </Link>
      <Link to={routes.about(storeId)} className="min-w-0 flex-1">
        <div className="flex min-w-0 flex-col">
          <span className="min-w-0 truncate text-2xs font-medium">{title}</span>
          <span className="truncate text-2xs text-gray-500">
            {publishersCount} {t("common:publishers")}
          </span>
        </div>
      </Link>
      <SvgStar
        className={twMerge(
          "size-5 shrink-0 cursor-pointer stroke-gray-400 hover:fill-favorite hover:stroke-favorite",
          isInFavorites && "fill-favorite stroke-favorite hover:fill-transparent hover:stroke-gray-400",
        )}
        onClick={isInFavorites ? handleFavoriteRemove : handleFavoriteAdd}
      />
    </div>
  )
})
