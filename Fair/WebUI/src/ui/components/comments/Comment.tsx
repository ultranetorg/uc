import { memo, useCallback, useMemo, useState } from "react"
import {
  FloatingPortal,
  offset,
  safePolygon,
  useDismiss,
  useFloating,
  useHover,
  useInteractions,
  useRole,
} from "@floating-ui/react"

import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize } from "hooks"
import { TEST_REVIEW_SRC } from "testConfig"
import { AccountBaseAvatar } from "types"
import { RatingBar, SimpleMenu } from "ui/components"
import { buildSrc, formatDate } from "utils"

export type CommentProps = {
  account: AccountBaseAvatar
  created: number
  rating?: number
  text: string
}

export const Comment = memo(({ account, created, rating, text }: CommentProps) => {
  const [isExpanded, setExpanded] = useState(false)

  useScrollOrResize(() => setExpanded(false), isExpanded)

  const { context, floatingStyles, refs } = useFloating({
    middleware: [offset(8)],
    open: isExpanded,
    placement: "bottom-end",
    onOpenChange: setExpanded,
  })
  const dismiss = useDismiss(context)
  const hover = useHover(context, { handleClose: safePolygon() })
  const role = useRole(context)
  const { getReferenceProps, getFloatingProps } = useInteractions([dismiss, hover, role])

  const menuItems = useMemo(
    () => [
      {
        onClick: () => alert("Banish user"),
        label: "Banish user",
      },
      { onClick: () => alert("Reject the review"), label: "Reject the review" },
    ],
    [],
  )

  const handleMenuClick = useCallback(() => setExpanded(false), [])

  const displayName = account.nickname ?? account.id

  return (
    <>
      <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
        <div className="flex flex-col gap-4">
          <div className="flex gap-4.5">
            <div className="size-13 overflow-hidden rounded-full">
              <img src={buildSrc(account.avatar, TEST_REVIEW_SRC)} className="size-full object-cover" />
            </div>
            <div className="flex flex-1 flex-col justify-center gap-2">
              <div className="flex items-center justify-between">
                <span className="text-2sm font-semibold leading-4.5 text-gray-800" title={displayName}>
                  {displayName}
                </span>
                <div ref={refs.setReference} {...getReferenceProps()}>
                  <SvgThreeDotsVertical className="cursor-pointer fill-gray-500 hover:fill-gray-800" />
                </div>
              </div>
              <span className="text-2xs font-medium leading-4 text-gray-500">{formatDate(created)}</span>
            </div>
          </div>
          {rating && <RatingBar value={rating} />}
        </div>
        <div className="text-2sm leading-5 text-gray-800">{text}</div>
      </div>
      {isExpanded && (
        <FloatingPortal>
          <SimpleMenu
            ref={refs.setFloating}
            items={menuItems}
            style={floatingStyles}
            onClick={handleMenuClick}
            {...getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
