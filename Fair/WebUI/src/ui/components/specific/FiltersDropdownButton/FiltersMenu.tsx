import { forwardRef, memo } from "react"
import { PropsWithStyle } from "types"
import { ResetAllButton } from "./ResetAllButton"

type FiltersMenuBaseProps = {
  onResetClick(): void
  resetAllLabel: string
}

export type FiltersMenuProps = PropsWithStyle & FiltersMenuBaseProps

export const FiltersMenu = memo(
  forwardRef<HTMLDivElement, FiltersMenuProps>(({ style, onResetClick, resetAllLabel }, ref) => (
    <div className="flex flex-col rounded-lg border border-[#d9d9d9] bg-gray-50 p-4 shadow-md" style={style} ref={ref}>
      <ResetAllButton onClick={onResetClick} label={resetAllLabel} />
    </div>
  )),
)
