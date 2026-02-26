import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchAccounts } from "entities"
import { AccountBase } from "types"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

import { MembersPanelList } from "./MembersPanelList"

export type AddModeratorPanelListProps = {
  value: AccountBase[]
  onChange: (value: AccountBase[]) => void
}

export const AddModeratorPanelList = memo(
  ({ value: selectedModerators = [], onChange }: AddModeratorPanelListProps) => {
    const { site } = useSiteContext()
    const { t } = useTranslation("createProposal")

    const [search, setSearch] = useState("")
    const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

    const { data: users = [] } = useSearchAccounts(debouncedSearch)

    const dropdownSearchItems = useMemo<DropdownItem[]>(
      () =>
        users
          .filter(x => selectedModerators.every(a => a.id !== x.id)) // Do not display moderators that have already been selected
          .filter(x => site?.moderatorsIds.every(m => m !== x.id)) // Do not display moderators who are already moderators
          .map(x => ({ label: x.nickname ?? x.id, value: x.id, avatarId: x.id })) || [],
      [users, selectedModerators, site?.moderatorsIds],
    )

    const handleAccountSelect = useCallback(
      (item: DropdownItem) => {
        const accountToAdd: AccountBase = {
          id: item.value,
          nickname: item.label,
          address: "",
        }
        onChange([...selectedModerators, accountToAdd])
        setSearch("")
      },
      [onChange, selectedModerators],
    )

    const accountsListItems = useMemo(
      () =>
        selectedModerators.map(({ id, nickname }) => ({
          id,
          title: nickname ?? id,
          avatarId: id,
        })),
      [selectedModerators],
    )

    const handleItemRemove = useCallback(
      (id: string) => onChange(selectedModerators.filter(x => x.id !== id)),
      [onChange, selectedModerators],
    )

    return (
      <MembersPanelList
        t={t}
        memberType="moderator"
        modeType="add"
        searchItems={dropdownSearchItems}
        search={search}
        selectedItems={accountsListItems}
        onSearchChange={setSearch}
        onSearchItemSelect={handleAccountSelect}
        onSelectedItemRemove={handleItemRemove}
      />
    )
  },
)
