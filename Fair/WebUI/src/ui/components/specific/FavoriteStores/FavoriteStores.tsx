import { memo, useCallback, useState } from "react"
import {
  FloatingPortal,
  offset,
  shift,
  useClick,
  useDismiss,
  useFloating,
  useInteractions,
  useRole,
} from "@floating-ui/react"
import { twMerge } from "tailwind-merge"

import { usePendingOperationsContext, useUserContext } from "app"
import { SvgChevronDown } from "assets"
import { useScrollOrResize } from "hooks"
import { PropsWithClassName } from "types"

import { FavoriteStoreItem } from "./FavoriteStoreItem"
import { FavoriteStoresMenu } from "./FavoriteStoresMenu"
import { FavoriteStoresSkeleton } from "./FavoriteStores.Skeleton"
import { getFavoriteStoreVisibilityClassName } from "./utils"

const MAX_VISIBLE_ITEMS = 12

export const FavoriteStores = memo(({ className }: PropsWithClassName) => {
  const { isLoading, user } = useUserContext()
  const { favoritesChanges } = usePendingOperationsContext()

  const [isOpen, setOpen] = useState(false)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(24), shift({ padding: 8 })],
    open: isOpen,
    placement: "bottom",
    onOpenChange: setOpen,
  })

  const dismiss = useDismiss(context)
  const click = useClick(context, { toggle: true })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, click, role])

  const handleClose = useCallback(() => setOpen(false), [])

  useScrollOrResize(() => setOpen(false))

  if (isLoading) return <FavoriteStoresSkeleton className={className} />

  if (!user) return null

  const pendingChanges = Object.values(favoritesChanges)
  const pendingAdditions = pendingChanges.filter(
    x => x.action && !user.favoriteStores.some(store => store.id === x.store.id),
  )

  if (!user.favoriteStores.length && pendingAdditions.length == 0) return null

  const allItems = [...user.favoriteStores, ...pendingAdditions.map(x => x.store)]
  const visibleItems = allItems.slice(0, MAX_VISIBLE_ITEMS)

  return (
    <>
      <div className={twMerge("flex items-center gap-2", className)} ref={refs.setPositionReference}>
        {visibleItems.map((x, i) => (
          <FavoriteStoreItem
            key={x.id}
            className={getFavoriteStoreVisibilityClassName(i)}
            storeId={x.id}
            name={x.title}
            logoId={x.imageFileId}
            isPending={!!favoritesChanges[x.id]}
          />
        ))}
        <div
          className="flex size-8 cursor-pointer items-center justify-center rounded-sm bg-gray-600 hover:bg-gray-550"
          ref={refs.setReference}
          {...getReferenceProps()}
        >
          <SvgChevronDown className="stroke-gray-300" />
        </div>
      </div>

      {isOpen && (
        <FloatingPortal>
          <FavoriteStoresMenu
            ref={refs.setFloating}
            style={floatingStyles}
            items={user.favoriteStores}
            onClose={handleClose}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
