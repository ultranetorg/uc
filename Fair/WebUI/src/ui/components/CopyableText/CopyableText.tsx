/* eslint-disable jsx-a11y/anchor-is-valid */
import { ReactNode, memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { useCopyToClipboard } from "usehooks-ts"
import { Tooltip } from "react-tooltip"

import { SvgCopy } from "assets"
import { PropsWithClassName } from "types"

type CopyableTextBaseProps = {
  textClassName?: string
  children: string
  textCopiedMessage?: ReactNode
  textTitle?: string
}

type CopyableTextProps = PropsWithClassName<CopyableTextBaseProps>

export const CopyableText = memo(
  ({ className, textClassName, children, textCopiedMessage, textTitle }: CopyableTextProps) => {
    const [value, copy] = useCopyToClipboard()

    const handleClick = useCallback(() => {
      copy(children)
    }, [children, copy])

    return (
      <>
        <div className={twMerge("flex items-center gap-3", className)}>
          <div className={textClassName} title={textTitle ?? children}>
            {children}
          </div>
          <a data-tooltip-id="copy-text-click">
            <SvgCopy onClick={handleClick} className="cursor-pointer hover:fill-white" />
          </a>
        </div>
        <Tooltip
          delayHide={2000}
          id="copy-text-click"
          place="right"
          className="opaque"
          style={{ padding: "20px", backgroundColor: "#3dc1f2" }}
          noArrow
          openOnClick
        >
          {textCopiedMessage ? textCopiedMessage : <>{value} copied to clipboard.</>}
        </Tooltip>
      </>
    )
  },
)
