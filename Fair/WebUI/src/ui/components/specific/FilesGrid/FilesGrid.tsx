import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { SvgSpinnerXl } from "assets"
import { PropsWithClassName } from "types"

import { FileCard } from "./FileCard"

type FilesGridProps = PropsWithClassName & {
  filesIds: string[]
  hasNextPage?: boolean
  onSelect?: (id: string) => void
  noFilesLabel: string
}

export const FilesGrid = memo(
  forwardRef<HTMLDivElement, FilesGridProps>(({ className, filesIds, hasNextPage, onSelect, noFilesLabel }, ref) => (
    <div className={twMerge("grid w-full grid-cols-5 gap-4 pb-4", className)}>
      {filesIds.length === 0 ? (
        <div className="col-span-5 text-center">{noFilesLabel}</div>
      ) : (
        <>
          {filesIds.map(id => (
            <FileCard key={id} fileId={id} onClick={() => onSelect?.(id)} />
          ))}

          {hasNextPage === true && (
            <div ref={ref} className="col-span-5 flex items-center justify-center">
              <SvgSpinnerXl className="animate-spin fill-gray-300" />
            </div>
          )}
        </>
      )}
    </div>
  )),
)
