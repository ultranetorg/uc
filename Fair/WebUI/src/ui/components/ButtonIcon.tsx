import { ReactNode } from "react"
import { PropsWithClassName } from "types"

type ButtonIconBaseProps = {
  icon: ReactNode
  onClick?: () => void
}

export type ButtonIconProps = PropsWithClassName & ButtonIconBaseProps

export const ButtonIcon = ({ className, icon, onClick }: ButtonIconProps) => (
  <div className={className} onClick={onClick}>
    {icon}
  </div>
)
