import { forwardRef, ReactNode } from "react"
import { twMerge } from "tailwind-merge"
import "@szhsin/react-menu/dist/index.css"
import "@szhsin/react-menu/dist/transitions/zoom.css"

import { PropsWithClassName } from "types"

type ButtonBaseProps = {
  label?: ReactNode
  image?: JSX.Element
  onClick?: () => void
}

export type ButtonProps = PropsWithClassName & ButtonBaseProps

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(({ className, label, image, onClick }, ref) => (
  <button
    className={twMerge(
      "flex select-none items-center justify-start gap-1 rounded-md p-2 hover:bg-slate-300",
      className,
    )}
    onClick={onClick}
    ref={ref}
  >
    {image && <>{image}</>}
    {label && <span className="font-medium">{label}</span>}
  </button>
))
