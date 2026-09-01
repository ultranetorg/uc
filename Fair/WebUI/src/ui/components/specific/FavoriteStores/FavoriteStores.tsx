import { useCallback, useState } from "react"
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

import { useUserContext } from "app"
import { SvgChevronDown } from "assets"

import { FavoriteStoreItem } from "./FavoriteStoreItem"
import { FavoriteStoresMenu } from "./FavoriteStoresMenu/FavoriteStoresMenu"

const MAX_VISIBLE_ITEMS = 4

export const FavoriteStores = () => {
  const { user } = useUserContext()

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

  if (!user) return null

  const visibleItems = user.favoriteStores.slice(0, MAX_VISIBLE_ITEMS)

  return (
    <>
      <div className="flex items-center gap-2" ref={refs.setPositionReference}>
        {visibleItems.map(x => (
          <FavoriteStoreItem key={x.id} storeId={x.id} name={x.title} logoId={x.imageFileId} />
        ))}
        <div
          className="flex size-8 cursor-pointer items-center justify-center rounded bg-gray-600 hover:bg-gray-550"
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
}
