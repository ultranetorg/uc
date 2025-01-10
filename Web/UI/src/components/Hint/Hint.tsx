import { memo } from "react"

import { SvgInfoCircle } from "assets"

import { HintProviderId } from "./HintProvider"

export type HintProps = {
  title?: string
  text: string
}

export const Hint = memo(({ title, text }: HintProps) => (
  <div className="cursor-pointer">
    <a data-tooltip-id={HintProviderId} data-tooltip-content={text} data-tooltip-title={title}>
      <SvgInfoCircle className="stroke-cyan-500" />
    </a>
  </div>
))
