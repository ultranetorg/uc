import { useCallback, useState } from "react"

import { ButtonOutline } from "ui/components"
import { MembersChangeModal } from "ui/components/proposal"

import { MemberType } from "../../types"

export type ButtonMembersChangeProps = {
  memberType: MemberType
  label: string
  onChange: (value: string | string[]) => void
}

export const ButtonMembersChange = ({ memberType, label }: ButtonMembersChangeProps) => {
  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)

  const handleClick = useCallback(() => setMembersChangeModalOpen(true), [])
  const handleModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <>
      <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
      {isMembersChangeModalOpen && <MembersChangeModal memberType={memberType} onClose={handleModalClose} />}
    </>
  )
}
