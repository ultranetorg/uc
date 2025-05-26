import { Grid3x3GapFillSvg } from "assets"
import { memo } from "react"

export type SidebarButtonProps = {
  title: string
}

export const SidebarButton = memo(({ title }: SidebarButtonProps) => (
  <div className="group flex items-center gap-3">
    <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gray-950">
      <Grid3x3GapFillSvg className="fill-white stroke-white" />
    </div>
    <span className="w-36 flex-grow overflow-hidden text-ellipsis whitespace-nowrap text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
      {title}
    </span>
  </div>
))
