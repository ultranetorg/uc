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
      <div
        className={twMerge(
          "grid w-full auto-rows-min grid-cols-2 gap-3 pb-4 sm:grid-cols-3 sm:gap-4 md:grid-cols-5",
          className,
        )}
      >
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
          <div ref={ref} className="col-span-full flex items-center justify-center">
            <SvgSpinnerXl className="animate-spin fill-gray-300" />
          </div>
        )}
      </div>
    ),
  ),
)
