import { memo } from "react"

import { SvgImageSlash } from "assets"
import { buildFileUrl, formatTitle } from "utils"
import { CategoryParentBaseWithChildren } from "types"
import { CategoryTreeContextThreeDotsMenu } from "ui/components/specific"

export type ChildCategoryProps = Pick<CategoryParentBaseWithChildren, "id" | "avatarId" | "title">

export const ChildCategory = memo(({ id, avatarId, title }: ChildCategoryProps) => (
  <div className="group relative flex w-full min-w-0 flex-col items-center gap-2 rounded-lg bg-gray-100 py-5 hover:bg-gray-200">
    <div className="size-8 overflow-hidden rounded-md">
      {avatarId ? (
        <img src={buildFileUrl(avatarId)} className="size-full object-cover" />
      ) : (
        <SvgImageSlash className="size-full stroke-gray-800" />
      )}
    </div>
    <span className="text-2sm leading-4.5">{formatTitle(title)}</span>
    <CategoryTreeContextThreeDotsMenu
      categoryId={id}
      categoryTitle={title}
      className="invisible absolute right-1 top-1 group-hover:visible"
    />
  </div>
))
