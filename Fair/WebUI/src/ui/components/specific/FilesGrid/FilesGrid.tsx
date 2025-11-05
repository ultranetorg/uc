import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { FileCard } from "./FileCard"

type FilesGridBaseProps = {
  isLoading: boolean
  filesIds?: string[]
  onSelect?: (id: string) => void
}

export type FilesGridProps = PropsWithClassName & FilesGridBaseProps

export const FilesGrid = memo(({ className, isLoading, filesIds, onSelect }: FilesGridProps) =>
  isLoading || !filesIds ? (
    <>LOADING 🕐</>
  ) : filesIds.length === 0 ? (
    <>YOU DON'T HAVE ANY IMAGE</>
  ) : (
    <div className={twMerge("grid w-full grid-cols-5 gap-6", className)}>
      {filesIds?.map(id => (
        <FileCard key={id} fileId={id} onClick={() => onSelect?.(id)} />
      ))}
    </div>
  ),
)
