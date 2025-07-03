import { SvgFilesSm } from "assets"

export type CopyButtonProps = {
  onClick?: () => void
}

export const CopyButton = ({ onClick }: CopyButtonProps) => (
  <SvgFilesSm className="cursor-pointer fill-gray-500" onClick={onClick} />
)
