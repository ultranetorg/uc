import { useModeratorContext } from "app"
import { useCallback } from "react"
import { ButtonOutline } from "ui/components"

export type ButtonMembersChangeProps = {
  memberType: "author" | "moderator"
  label: string
  onDataChange: (name: string, value: string) => void
}

export const ButtonMembersChange = ({ label }: ButtonMembersChangeProps) => {
  const { openMembersChangeModal } = useModeratorContext()

  const handleClick = useCallback(() => openMembersChangeModal!(), [openMembersChangeModal])

  return <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
}
