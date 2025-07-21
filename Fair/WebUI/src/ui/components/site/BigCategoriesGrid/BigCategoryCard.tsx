import { SvgFolderSoftwareXl } from "assets"
import { buildSrc } from "utils"

export type BigCategoryCardProps = {
  title: string
  avatar?: string
}

export const BigCategoryCard = ({ title, avatar }: BigCategoryCardProps) => (
  <div
    className="flex w-full flex-col items-center gap-2 rounded-lg bg-gray-100 py-5.25 hover:bg-gray-200"
    title={title}
  >
    <div className="h-8 w-8 overflow-hidden rounded-md">
      {avatar ? (
        <img src={buildSrc(avatar)} className="h-full w-full object-cover" />
      ) : (
        <SvgFolderSoftwareXl className="stroke-gray-500" />
      )}
    </div>
    <span className="w-40 overflow-hidden text-ellipsis whitespace-nowrap text-center text-2sm leading-4.5 text-gray-800">
      {title}
    </span>
  </div>
)
