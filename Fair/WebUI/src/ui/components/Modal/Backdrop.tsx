import { PropsWithChildren, memo } from "react"

type BackdropBaseProps = {
  onClick?: () => void
}

export type BackdropProps = PropsWithChildren & BackdropBaseProps

export const Backdrop = memo(({ children, onClick }: BackdropProps) => (
  <div
    className="fixed left-0 top-0 z-60 flex size-full min-h-screen items-center justify-center overflow-auto bg-black/50"
    onClick={onClick}
  >
    {children}
  </div>
))
