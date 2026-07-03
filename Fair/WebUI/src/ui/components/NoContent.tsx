import { memo, PropsWithChildren } from "react"

export const NoContent = memo(({ children }: PropsWithChildren) => (
  <div className="flex h-80 items-center justify-center">{children}</div>
))
