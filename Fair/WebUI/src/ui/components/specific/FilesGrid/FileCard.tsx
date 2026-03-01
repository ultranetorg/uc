import { memo } from "react"
import { twMerge } from "tailwind-merge"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export type FileCardProps = {
  fileId: string
  refs: number
  selected?: boolean
  onClick: () => void
  notUsedLabel: string
  usedLabel: string
}

export const FileCard = memo(({ fileId, refs, selected, onClick, notUsedLabel, usedLabel }: FileCardProps) => (
  <div
    className={twMerge(
      "focused:border-gray-500 flex h-[152px] cursor-pointer flex-col overflow-hidden rounded border border-gray-300 bg-white text-center hover:border-gray-400",
      selected && "border-[3px] border-gray-500",
    )}
    onClick={onClick}
    title={fileId}
  >
    <div className="h-25 w-full overflow-hidden bg-gray-200">
      <img
        className="size-full object-cover object-center"
        src={`${BASE_URL}/files/${fileId}`}
        title={fileId}
        loading="lazy"
      />
    </div>
    <div className="flex flex-1 flex-col items-start justify-center gap-1 px-2 text-2xs leading-4">
      <span className="truncate text-gray-900">{fileId}</span>
      <span className="truncate text-gray-500">{refs === 0 ? notUsedLabel : `${usedLabel} ${refs}`}</span>
    </div>
  </div>
))
