import { memo } from "react"

export type OvalButtonProps = {
  label?: string
  onClick?: () => void
}

export const OvalButton = memo(({ label, onClick }: OvalButtonProps) => (
  <div
    className="box-border h-fit min-h-[32px] w-fit cursor-pointer select-none rounded-[32px] bg-cyan-500/10 px-3 py-2 text-sm leading-4 text-cyan-500 hover:bg-cyan-500/20"
    onClick={onClick}
  >
    {label}
  </div>
))
