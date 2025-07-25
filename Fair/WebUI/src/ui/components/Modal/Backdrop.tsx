import { PropsWithChildren, memo } from "react"

type BackdropBaseProps = {
  onClick?: () => void
}

export type BackdropProps = PropsWithChildren & BackdropBaseProps

export const Backdrop = memo(({ children, onClick }: BackdropProps) => (
  <div
    className="items-centner z-60 fixed left-0 top-0 flex h-full min-h-[100vh] w-full justify-center overflow-auto bg-black/50"
    onClick={onClick}
  >
    {children}
  </div>
))
