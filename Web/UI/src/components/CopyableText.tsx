import { memo, useCallback } from "react"
import { useCopyToClipboard } from "usehooks-ts"

import { SvgCopy } from "assets"
import { Tooltip } from "components"
import { PropsWithClassName } from "types"

type CopyableTextBaseProps = {
  copiedMessage?: string
  text: string
  title?: string
}

type CopyableTextProps = PropsWithClassName & CopyableTextBaseProps

export const CopyableText = memo((props: CopyableTextProps) => {
  const { className, copiedMessage, text, title } = props

  const [value, copy] = useCopyToClipboard()

  const handleClick = useCallback(() => copy(text), [copy, text])

  return (
    <div className="flex items-center gap-1">
      <div title={title} className={className}>
        {text}
      </div>
      <Tooltip text={copiedMessage ? copiedMessage : `${value} copied to clipboard.`} openOnClick={true}>
        <SvgCopy onClick={handleClick} className="cursor-pointer fill-gray-500 hover:fill-cyan-500" />
      </Tooltip>
    </div>
  )
})
