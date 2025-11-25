import { memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { capitalize } from "lodash"

import { useGetSiteMembers } from "entities"
import { AccountBase, MembersChangeType } from "types"
import { AccountsList, AccountsListItemProps } from "ui/components"
import { DropdownSearchAccount } from "ui/components/proposal"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

export type RemoveMembersPanelListProps = {
  memberType: MembersChangeType
  value: string[]
  onChange: (value: string[]) => void
}

export const RemoveMembersPanelList = memo(
  ({ memberType, value: selectedIds = [], onChange }: RemoveMembersPanelListProps) => {
    const { siteId } = useParams()
    const { t } = useTranslation("createProposal")

    const [allMembers, setAllMembers] = useState<AccountBase[]>([])
    const [dropdownItemsIds, setDropdownItemsIds] = useState<string[]>([])

    const { data: members } = useGetSiteMembers(memberType, siteId)

    const dropdownSearchItems = useMemo<DropdownItem[]>(
      () =>
        allMembers
          ?.filter(x => dropdownItemsIds.includes(x.id) && !selectedIds.includes(x.id))
          .map(x => ({ label: x.nickname ?? x.id, value: x.id, address: x.address })) || [],
      [allMembers, dropdownItemsIds, selectedIds],
    )

    const accountsListItems = useMemo<AccountsListItemProps[]>(() => {
      return allMembers
        .filter(x => selectedIds.includes(x.id))
        .map(x => ({
          id: x.id,
          title: x.nickname ?? x.id,
          avatarId: x.id,
        }))
    }, [allMembers, selectedIds])

    const handleAccountSelect = useCallback(
      (value: DropdownItem) => onChange([...selectedIds, value.value]),
      [onChange, selectedIds],
    )

    const handleItemRemove = useCallback(
      (id: string) => onChange(selectedIds.filter(x => x !== id)),
      [onChange, selectedIds],
    )

    useEffect(() => {
      if (members) {
        setAllMembers(members)
        setDropdownItemsIds(members.map(x => x.id))
      }
    }, [members])

    return (
      <div className="flex flex-col divide-y divide-gray-300 rounded border border-gray-300 bg-gray-100">
        <span className="p-4 text-2sm font-semibold leading-5">
          {t("selectMembersToRemove", { memberType: capitalize(memberType) })}
        </span>
        <div className="flex flex-col gap-3 p-4">
          <DropdownSearchAccount
            placeholder={t("enterNickname")}
            items={dropdownSearchItems}
            onSelect={handleAccountSelect}
            noOptionsLabel={t("userNotFound")}
          />
          <AccountsList items={accountsListItems} onItemRemove={handleItemRemove} />
        </div>
        <span className="px-4 py-2 text-2xs font-medium leading-4">
          {t("selected")}: {selectedIds.length}
        </span>
      </div>
    )
  },
)
