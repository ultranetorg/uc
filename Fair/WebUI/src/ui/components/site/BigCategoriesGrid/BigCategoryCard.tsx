import { SvgFolderSoftwareXl } from "assets"
import { ModeratorCategoryContextMenu } from "ui/components/specific"
import { buildFileUrl } from "utils"

export type BigCategoryCardProps = {
  id: string
  title: string
  avatarId?: string
}

export const BigCategoryCard = ({ id, title, avatarId }: BigCategoryCardProps) => (
  <div
    className="relative flex w-full flex-col items-center gap-2 rounded-lg bg-gray-100 py-5.25 hover:bg-gray-200"
    title={title}
  >
    <div className="size-8 overflow-hidden rounded-md">
      {avatarId ? (
        <img src={buildFileUrl(avatarId)} className="size-full object-cover" />
      ) : (
        <SvgFolderSoftwareXl className="stroke-gray-500" />
      )}
    </div>
    <span className="w-40 truncate text-center text-2sm leading-4.5 text-gray-800">{title}</span>

    <ModeratorCategoryContextMenu categoryId={id} categoryTitle={title} className="absolute right-1 top-1" />
  </div>
)
