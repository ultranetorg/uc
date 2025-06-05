import { forwardRef, memo } from "react"
import { twMerge } from "tailwind-merge"

import { ThreeDotsSvg } from "assets"
import { PropsWithClassName } from "types"

type MoreButtonBaseProps = {
  onClick?: () => void
}

export type MoreButtonProps = PropsWithClassName & MoreButtonBaseProps

export const MoreButton = memo(
  forwardRef<HTMLDivElement, MoreButtonProps>(({ className, onClick, ...rest }, ref) => {
    return (
      <div
        ref={ref}
        className={twMerge(
          "flex h-8.5 w-8.5 items-center justify-center rounded bg-gray-100 hover:bg-gray-200",
          onClick && "cursor-pointer",
          className,
        )}
        onClick={onClick}
        {...rest}
      >
        <ThreeDotsSvg className="fill-gray-950" />
      </div>
    )
  }),
)
