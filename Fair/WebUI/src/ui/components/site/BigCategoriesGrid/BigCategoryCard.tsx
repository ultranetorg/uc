import { SvgFolderSoftwareXl } from "assets"
import { ModeratorCategoryContextMenu } from "ui/components/specific"
import { buildSrc } from "utils"

export type BigCategoryCardProps = {
  id: string
  title: string
  avatar?: string
}

export const BigCategoryCard = ({ id, title, avatar }: BigCategoryCardProps) => (
  <div
    className="relative flex w-full flex-col items-center gap-2 rounded-lg bg-gray-100 py-5.25 hover:bg-gray-200"
    title={title}
  >
    <div className="size-8 overflow-hidden rounded-md">
      {avatar ? (
        <img src={buildSrc(avatar)} className="size-full object-cover" />
      ) : (
        <SvgFolderSoftwareXl className="stroke-gray-500" />
      )}
    </div>
    <span className="w-40 truncate text-center text-2sm leading-4.5 text-gray-800">{title}</span>

    <ModeratorCategoryContextMenu categoryId={id} className="absolute right-1 top-1" />
  </div>
)
