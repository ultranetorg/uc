import { memo } from "react"

import { SvgStoreLogo } from "assets/fallback"
import { StoreBase } from "types"
import { ImageFallback } from "ui/components"
import { buildFileUrl } from "utils"

import { COLUMN_NAME_CLASSNAME, ROW_CLASSNAME } from "./styles"

export type StoreRowProps = Pick<StoreBase, "title" | "imageFileId">

export const StoreRow = memo(({ title, imageFileId }: StoreRowProps) => (
  <div className={ROW_CLASSNAME} title={title}>
    <div className={COLUMN_NAME_CLASSNAME}>
      <div className="size-8 flex-none overflow-hidden rounded-lg">
        <ImageFallback src={buildFileUrl(imageFileId)} fallback={<SvgStoreLogo className="size-full object-cover" />} />
      </div>
      <span className="truncate text-2xs leading-4 text-gray-900">{title}</span>
    </div>
  </div>
))
