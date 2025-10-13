import { memo } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { capitalize } from "lodash"

import { useGetSiteMembers } from "entities"
import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

import { MemberType } from "../types"
import { AddMembersPanel } from "./AddMembersPanel"
import { CurrentMembersPanel } from "./CurrentMembersPanel"

type MembersChangeModalBaseProps = {
  memberType: MemberType
}

export type MembersChangeModalProps = ModalProps & MembersChangeModalBaseProps

export const MembersChangeModal = memo(({ onClose, memberType, ...rest }: MembersChangeModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("membersChangeModal")
  useEscapeKey(onClose)

  const { data: currentMembers, isFetching } = useGetSiteMembers(memberType, siteId)

  return (
    <Modal
      {...rest}
      title={t("title", { memberType: capitalize(memberType) })}
      onClose={onClose}
      className="flex h-170 w-220 max-w-220"
    >
      <div className="flex h-full w-full flex-col gap-6">
        <div className="flex h-full items-center gap-6">
          <CurrentMembersPanel t={t} currentMembers={currentMembers} isFetching={isFetching} memberType={memberType} />
          <AddMembersPanel t={t} currentMembers={currentMembers} isFetching={isFetching} memberType={memberType} />
        </div>
        <div className="flex justify-end gap-6">
          <ButtonOutline className="w-25" label="Back" />
          <ButtonPrimary className="w-25" label="Create" />
        </div>
      </div>
    </Modal>
  )
})
