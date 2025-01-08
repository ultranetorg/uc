import { memo, PropsWithChildren } from "react"
import { PlacesType } from "react-tooltip"

import { ClickableTooltipProviderId, TooltipProviderId } from "./TooltipProvider"

type TooltipBaseProps = {
  text: string
  place?: PlacesType
  openOnClick?: boolean
}

export type TooltipProps = PropsWithChildren & TooltipBaseProps

export const Tooltip = memo(({ children, text, place = "right", openOnClick = false }: TooltipProps) => (
  <a
    data-tooltip-id={openOnClick !== true ? TooltipProviderId : ClickableTooltipProviderId}
    data-tooltip-content={text}
    data-tooltip-place={place}
  >
    {children}
  </a>
))
