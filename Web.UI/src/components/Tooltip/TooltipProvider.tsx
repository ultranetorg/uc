import { memo } from "react"
import { createPortal } from "react-dom"
import { ITooltip, Tooltip as ReactTooltip } from "react-tooltip"
import { isMobile } from "react-device-detect"

export const TooltipProviderId = "global-tooltip"

export const ClickableTooltipProviderId = "global-clickable-tooltip"

// TODO: use tailwind colors.
const Tooltip = memo((props: ITooltip) =>
  createPortal(
    <ReactTooltip
      style={{
        fontSize: "13px",
        backgroundColor: "#0E7490", // cyan-700
        backdropFilter: "blur(10px)",
        boxShadow: "0px 10px 12px 0px rgba(25, 27, 32, 0.45)",
        borderRadius: "6px",
        color: "#E5E7EB", // gray-200
        padding: "8px",
        zIndex: "100",
      }}
      border={"1px solid #06B6D4"} // cyan-500
      opacity={1}
      offset={6}
      delayHide={2000}
      {...props}
    />,
    document.body,
  ),
)

export const TooltipProvider = () => (
  <Tooltip id={TooltipProviderId} openEvents={{ click: isMobile, mouseenter: !isMobile }} />
)

export const ClickableTooltipProvider = () => <Tooltip id={ClickableTooltipProviderId} openEvents={{ click: true }} />
