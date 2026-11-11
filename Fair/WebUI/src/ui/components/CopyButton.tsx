import { memo } from "react"

import { SvgFilesSm } from "assets"

export type CopyButtonProps = {
  onClick?: () => void
}

export const CopyButton = memo(({ onClick }: CopyButtonProps) => (
  <SvgFilesSm className="cursor-pointer fill-gray-300 hover:fill-gray-100" onClick={onClick} />
))
