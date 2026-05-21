import { useCallback } from "react"
import { useCopyToClipboard } from "usehooks-ts"
import { CopyButton } from "ui/components"
import { shortenAddress } from "utils"

export type CopyAddressButtonProps = {
  address: string
}

export const CopyAddressButton = ({ address }: CopyAddressButtonProps) => {
  const [, copy] = useCopyToClipboard()

  const handleCopyClick = useCallback(() => copy(address), [address, copy])

  return (
    <div className="flex items-center gap-1">
      <span className="overflow-hidden text-ellipsis text-nowrap text-2xs leading-4 text-gray-500" title={address}>
        {shortenAddress(address)}
      </span>
      <CopyButton onClick={handleCopyClick} />
    </div>
  )
}
