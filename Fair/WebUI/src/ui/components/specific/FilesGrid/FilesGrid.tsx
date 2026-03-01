import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgSpinnerXl } from "assets"
import { File, PropsWithClassName } from "types"

import { FileCard } from "./FileCard"

type FilesGridProps = PropsWithClassName & {
  files: File[]
  hasNextPage?: boolean
  selectedFileId?: string
  onSelect?: (file: File) => void
  notUsedLabel: string
  usedLabel: string
}

export const FilesGrid = memo(
  forwardRef<HTMLDivElement, FilesGridProps>(
    ({ className, files, hasNextPage, selectedFileId, onSelect, notUsedLabel, usedLabel }, ref) => (
      <div className={twMerge("grid w-full auto-rows-min grid-cols-5 gap-4 pb-4", className)}>
        {files.map(x => (
          <FileCard
            key={x.id}
            fileId={x.id}
            refs={x.refs}
            selected={x.id === selectedFileId}
            onClick={() => onSelect?.(x)}
            notUsedLabel={notUsedLabel}
            usedLabel={usedLabel}
          />
        ))}

        {hasNextPage === true && (
          <div ref={ref} className="col-span-5 flex items-center justify-center">
            <SvgSpinnerXl className="animate-spin fill-gray-300" />
          </div>
        )}
      </div>
    ),
  ),
)
