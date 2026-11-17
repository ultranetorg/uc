import { PropsWithChildren, memo } from "react"

type BackdropBaseProps = {
  onClick?: () => void
}

export type BackdropProps = PropsWithChildren & BackdropBaseProps

export const Backdrop = memo(({ children, onClick }: BackdropProps) => (
  <div
    className="fixed top-0 left-0 z-60 flex size-full min-h-screen items-center justify-center overflow-auto bg-black/50 p-4"
    onClick={onClick}
  >
    {children}
  </div>
))
