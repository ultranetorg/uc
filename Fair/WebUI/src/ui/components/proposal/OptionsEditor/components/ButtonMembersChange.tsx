import { useCallback } from "react"

import { useModerationContext } from "app"
import { ButtonOutline } from "ui/components"

import { MemberType } from "../../types"

export type ButtonMembersChangeProps = {
  memberType: MemberType
  label: string
  onDataChange: (name: string, value: string) => void
}

export const ButtonMembersChange = ({ memberType, label }: ButtonMembersChangeProps) => {
  const { openMembersChangeModal } = useModerationContext()

  const handleClick = useCallback(() => openMembersChangeModal!(memberType), [memberType, openMembersChangeModal])

  return <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
}
