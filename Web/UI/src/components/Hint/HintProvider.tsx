import { memo } from "react"
import { createPortal } from "react-dom"
import { ITooltip, Tooltip as ReactTooltip } from "react-tooltip"
import { isMobile } from "react-device-detect"

export const HintProviderId = "global-hint"

// TODO: use tailwind colors.
export const HintProvider = memo((props: ITooltip) =>
  createPortal(
    <ReactTooltip
      id={HintProviderId}
      style={{
        fontSize: "13px",
        backgroundColor: "#0E7490", // cyan-700
        boxShadow: "0px 4px 20px 0px rgba(0, 0, 0, 0.08)",
        borderRadius: "8px",
        color: "#E4E4E7",
        maxWidth: "324px",
        padding: "12px",
        zIndex: "29",
      }}
      border={"1px solid #06B6D4"} // cyan-500
      opacity={1}
      offset={6}
      place="right-end"
      delayHide={2000}
      noArrow={true}
      openEvents={{ click: isMobile, mouseenter: !isMobile }}
      render={({ content, activeAnchor }) => {
        const title = activeAnchor?.getAttribute("data-tooltip-title")
        return (
          <div className="flex flex-col gap-1">
            {title && <div className="font-bold">{title}</div>}
            {content}
          </div>
        )
      }}
      {...props}
    />,
    document.body,
  ),
)
