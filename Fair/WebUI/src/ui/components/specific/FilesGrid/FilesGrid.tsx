import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { FileCard } from "./FileCard"

type FilesGridProps = PropsWithClassName & {
  filesIds: string[]
  hasNextPage?: boolean
  onSelect?: (id: string) => void
  loadingLabel: string
  noFilesLabel: string
}

export const FilesGrid = memo(
  forwardRef<HTMLDivElement, FilesGridProps>(
    ({ className, filesIds, hasNextPage, onSelect, loadingLabel, noFilesLabel }, ref) => (
      <div className={twMerge("grid w-full grid-cols-5 gap-4 pb-4", className)}>
        {filesIds.length === 0 ? (
          <div className="col-span-5 text-center">{noFilesLabel}</div>
        ) : (
          <>
            {filesIds.map(id => (
              <FileCard key={id} fileId={id} onClick={() => onSelect?.(id)} />
            ))}
            {hasNextPage === true && (
              <div ref={ref} className="col-span-5 flex h-10 items-center justify-center">
                {loadingLabel}
              </div>
            )}
          </>
        )}
      </div>
    ),
  ),
)
