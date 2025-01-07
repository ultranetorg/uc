import { memo } from "react"

import { OvalButton } from "./OvalButton"

export type CounterButtonProps = {
  count: number
  counterLabel: string
  buttonLabel: string
  onClick: () => void
}

export const CounterButton = memo(({ count, counterLabel, buttonLabel, onClick }: CounterButtonProps) => (
  <div className="flex items-center gap-2">
    {count} {counterLabel} <OvalButton label={buttonLabel} onClick={onClick} />
  </div>
))
