import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonBaseProps = {
  label?: string
  image?: JSX.Element
  onClick?: () => void
}

export type ButtonProps = PropsWithClassName & ButtonBaseProps

export const Button = ({ className, label, image, onClick }: ButtonProps) => (
  <button
    className={twMerge("flex select-none items-center justify-start gap-1 p-2 hover:bg-slate-300", className)}
    onClick={onClick}
  >
    {image && <>{image}</>}
    {label && <span className="font-medium">{label}</span>}
  </button>
)
