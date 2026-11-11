import { memo, useCallback } from "react"
import { useCopyToClipboard } from "usehooks-ts"

import { CopyButton } from "ui/components"
import { shortenAddress } from "utils"

export type CopyAddressButtonProps = {
  address: string
  onCopy?: () => void
}

export const CopyAddressButton = memo(({ address, onCopy }: CopyAddressButtonProps) => {
  const [, copy] = useCopyToClipboard()

  const handleCopyClick = useCallback(async () => {
    const copied = await copy(address)
    if (copied) onCopy?.()
  }, [address, copy, onCopy])

  return (
    <div className="flex items-center gap-1">
      <span className="overflow-hidden text-2xs leading-4 text-nowrap text-ellipsis text-gray-400" title={address}>
        {shortenAddress(address)}
      </span>
      <CopyButton onClick={handleCopyClick} />
    </div>
  )
})
