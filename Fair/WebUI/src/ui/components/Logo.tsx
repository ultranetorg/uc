export type LogoProps = {
  title: string
}

export const Logo = ({ title }: LogoProps) => (
  <div className="flex max-w-36 items-center gap-3" title={title}>
    <div className="h-10 w-10 rounded-lg bg-zinc-700" />
    <span className="select-none overflow-hidden text-ellipsis text-nowrap text-sm font-medium">{title}</span>
  </div>
)
