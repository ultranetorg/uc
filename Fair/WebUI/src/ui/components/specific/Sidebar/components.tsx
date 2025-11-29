import { memo } from "react"

import { StoresSvg } from "assets"

export type AllSitesButtonProps = {
  title: string
}

export const AllSitesButton = memo(({ title }: AllSitesButtonProps) => (
  <div className="group flex items-center gap-3">
    <div className="flex size-10 items-center justify-center rounded-lg bg-gray-950">
      <StoresSvg className="fill-white" />
    </div>
    <span className="w-36 grow truncate text-2xs font-medium leading-4 text-gray-800 group-hover:font-semibold">
      {title}
    </span>
  </div>
))
