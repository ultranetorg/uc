import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useStoreContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchUsers } from "entities"
import { UserBase } from "types"
import { DropdownItem } from "ui/components/proposal/DropdownSearchMember"

import { MembersPanelList } from "./MembersPanelList"

export type RemoveModeratorPanelListProps = {
  value: UserBase[]
  onChange: (value: UserBase[]) => void
}

export const RemoveModeratorPanelList = memo(
  ({ value: selectedModerators = [], onChange }: RemoveModeratorPanelListProps) => {
    const { store } = useStoreContext()
    const { t } = useTranslation("createProposal")

    const [search, setSearch] = useState("")
    const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

    const { data: users = [] } = useSearchUsers(debouncedSearch)

    const searchItems = useMemo<DropdownItem[]>(
      () =>
        users
          .filter(x => selectedModerators.every(a => a.id !== x.id)) // Do not display moderators that have already been selected
          .filter(x => store?.moderatorsIds.some(m => m === x.id)) // Do not display users who are not moderators
          .map(x => ({ label: x.nickname ?? x.id, value: x.id, avatarId: x.id })) || [],
      [users, selectedModerators, store?.moderatorsIds],
    )

    const selectedItems = useMemo(
      () =>
        selectedModerators.map(({ id, nickname }) => ({
          id,
          title: nickname ?? id,
          avatarId: id,
        })),
      [selectedModerators],
    )

    const handleSearchItemSelect = useCallback(
      (item: DropdownItem) => {
        const accountToAdd: UserBase = {
          id: item.value,
          nickname: item.label,
          address: "",
        }
        onChange([...selectedModerators, accountToAdd])
        setSearch("")
      },
      [onChange, selectedModerators],
    )

    const handleSelectedItemRemove = useCallback(
      (id: string) => onChange(selectedModerators.filter(x => x.id !== id)),
      [onChange, selectedModerators],
    )

    return (
      <MembersPanelList
        t={t}
        memberType="moderator"
        modeType="remove"
        searchItems={searchItems}
        search={search}
        selectedItems={selectedItems}
        onSearchChange={setSearch}
        onSearchItemSelect={handleSearchItemSelect}
        onSelectedItemRemove={handleSelectedItemRemove}
      />
    )
  },
)
