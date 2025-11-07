import { useCallback, useState } from "react"

import { AccountBase } from "types"
import { ButtonOutline } from "ui/components"
import { MembersAddModal, MembersRemoveModal } from "ui/components/proposal"

import { MemberType } from "../../types"

export type ChangeAction = "add" | "remove"

export type ButtonMembersChangeProps = {
  changeAction: ChangeAction
  memberType: MemberType
  label: string
  value?: string[] | AccountBase[]
  onChange: (value: string[] | AccountBase[]) => void
}

export const ButtonMembersChange = ({ changeAction, memberType, label, value, onChange }: ButtonMembersChangeProps) => {
  const [isMembersChangeModalOpen, setMembersChangeModalOpen] = useState(false)

  const handleClick = useCallback(() => setMembersChangeModalOpen(true), [])
  const handleModalClose = useCallback(() => setMembersChangeModalOpen(false), [])

  return (
    <>
      <ButtonOutline className="h-10 w-full" label={label} onClick={handleClick} />
      {isMembersChangeModalOpen &&
        (changeAction === "add" ? (
          <MembersAddModal
            memberType={memberType}
            candidates={value as AccountBase[]}
            onClose={handleModalClose}
            onChange={onChange}
          />
        ) : (
          <MembersRemoveModal
            memberType={memberType}
            membersIds={value as string[]}
            onClose={handleModalClose}
            onChange={onChange}
          />
        ))}
    </>
  )
}
