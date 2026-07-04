import { memo } from "react"

export type LogoProps = {
  title: string
}

export const Logo = memo(({ title }: LogoProps) => (
  <div className="flex max-w-36 items-center gap-3" title={title}>
    <div className="size-10 rounded-lg bg-zinc-700" />
    <span className="flex-1 select-none overflow-hidden text-ellipsis text-nowrap text-sm font-medium">{title}</span>
  </div>
))
