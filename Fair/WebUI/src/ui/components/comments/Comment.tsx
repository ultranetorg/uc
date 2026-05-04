import { ComponentType, memo } from "react"

import { TEST_REVIEW_SRC } from "testConfig"
import { AccountBaseAvatar } from "types"
import { RatingBar } from "ui/components"
import { buildSrc, formatDate } from "utils"

export type CommentContextMenuProps = {
  id: string
  text: string
}

export type CommentProps = {
  account: AccountBaseAvatar
  id: string
  created: number
  rating?: number
  text: string
  contextMenu?: ComponentType<CommentContextMenuProps>
}

export const Comment = memo(({ account, id, created, rating, text, contextMenu: ContextMenu }: CommentProps) => {
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
                {ContextMenu && <ContextMenu id={id} text={text} />}
              </div>
              <span className="text-2xs font-medium leading-4 text-gray-500">{formatDate(created)}</span>
            </div>
          </div>
          {rating && <RatingBar value={rating} />}
        </div>
        <div className="text-2sm leading-5 text-gray-800">{text}</div>
      </div>
    </>
  )
})
