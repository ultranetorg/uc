import { memo, useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { capitalize } from "lodash"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useGetSiteMembers, useSearchAccounts } from "entities"
import { useEscapeKey } from "hooks"
import { AccountBase } from "types"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

import { MembersPanelList } from "../MembersPanelList"
import { MemberType } from "../types"

type MembersAddModalBaseProps = {
  memberType: MemberType
  candidates?: AccountBase[]
  onChange: (value: AccountBase[]) => void
}

export type MembersAddModalProps = ModalProps & MembersAddModalBaseProps

export const MembersAddModal = memo(({ memberType, candidates, onClose, onChange, ...rest }: MembersAddModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("membersChangeModal")
  useEscapeKey(onClose)

  const [search, setSearch] = useState("")
  const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)
  const [all, setAll] = useState<AccountBase[] | undefined>()
  const [available, setAvailable] = useState<AccountBase[]>([])
  const [selected, setSelected] = useState<AccountBase[]>([])

  const { data: currentMembers } = useGetSiteMembers(memberType, siteId)
  const { data: searchAccounts } = useSearchAccounts(debouncedSearch)

  const handleActionClick = useCallback(
    (id: string) => {
      const index = available!.findIndex(x => x.id === id)
      // Move to changed items.
      if (index !== -1) {
        const moveToChanged = available![index]
        setSelected(p => [...p!, moveToChanged])
        setAvailable(p => p!.filter((_, i) => i !== index))
      } else {
        // Move from changed items.
        const index = selected.findIndex(x => x.id === id)
        const moveFromChanged = selected[index]
        setSelected(p => p.filter((_, i) => i !== index))

        if (all?.some(x => x.id === id)) {
          setAvailable(p => [...p!, moveFromChanged])
        }
      }
    },
    [selected, available, all],
  )

  const handleConfirmClick = useCallback(() => {
    onChange(selected)
    onClose?.()
  }, [selected, onChange, onClose])

  useEffect(() => {
    if (candidates) {
      setSelected(candidates)
    }
  }, [candidates])

  useEffect(() => {
    if (!searchAccounts) return

    const withoutCurrent = searchAccounts.filter(item => !currentMembers?.some(member => member.id === item.id))
    setAll(withoutCurrent)
    const withoutChanged = withoutCurrent.filter(item => !selected.some(changed => changed.id === item.id))
    setAvailable(withoutChanged)
  }, [selected, currentMembers, searchAccounts])

  return (
    <Modal
      {...rest}
      title={t("title", { memberType: capitalize(memberType) })}
      onClose={onClose}
      className="flex h-170 w-190"
    >
      <div className="flex h-full w-full flex-col gap-6">
        <div className="flex h-full items-center gap-6">
          <MembersPanelList
            behavior={"add-items"}
            changedItems={selected}
            isLoading={false}
            items={available}
            search={search}
            status={t("common:add")}
            title={t("addMembersTitle", { memberType })}
            onActionClick={handleActionClick}
            onSearchChange={setSearch}
          />
        </div>
        <div className="flex justify-end gap-6">
          <ButtonOutline className="w-25 capitalize" label={t("common:back")} onClick={onClose} />
          <ButtonPrimary
            className="w-25 capitalize"
            label={t("common:confirm")}
            onClick={handleConfirmClick}
            disabled={selected.length <= 0}
          />
        </div>
      </div>
    </Modal>
  )
})
