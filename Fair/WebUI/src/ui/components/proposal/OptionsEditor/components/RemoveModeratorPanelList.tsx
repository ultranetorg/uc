import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchAccounts } from "entities"
import { AccountBase } from "types"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

import { MembersPanelList } from "./MembersPanelList"

export type RemoveModeratorPanelListProps = {
  value: AccountBase[]
  onChange: (value: AccountBase[]) => void
}

export const RemoveModeratorPanelList = memo(
  ({ value: selectedModerators = [], onChange }: RemoveModeratorPanelListProps) => {
    const { site } = useSiteContext()
    const { t } = useTranslation("createProposal")

    const [search, setSearch] = useState("")
    const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

    const { data: users = [] } = useSearchAccounts(debouncedSearch)

    const searchItems = useMemo<DropdownItem[]>(
      () =>
        users
          .filter(x => selectedModerators.every(a => a.id !== x.id)) // Do not display moderators that have already been selected
          .filter(x => site?.moderatorsIds.some(m => m === x.id)) // Do not display users who are not moderators
          .map(x => ({ label: x.nickname ?? x.id, value: x.id, avatarId: x.id })) || [],
      [users, selectedModerators, site?.moderatorsIds],
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
