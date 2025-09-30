import { useCallback } from "react"

import { useModerationContext } from "app"
import { ButtonOutline } from "ui/components"

export type ButtonMembersChangeProps = {
  memberType: "author" | "moderator"
  label: string
  onDataChange: (name: string, value: string) => void
}

export const ButtonMembersChange = ({ label }: ButtonMembersChangeProps) => {
  const { openMembersChangeModal } = useModerationContext()

  const handleClick = useCallback(() => openMembersChangeModal!(), [openMembersChangeModal])

  return <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
}
