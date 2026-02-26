import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchAuthors } from "entities"
import { AuthorBaseAvatar } from "types"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

import { MembersPanelList } from "./MembersPanelList"

export type RemoveMembersPanelListProps = {
  value: AuthorBaseAvatar[]
  onChange: (value: AuthorBaseAvatar[]) => void
}

export const RemoveMembersPanelList = memo(({ value: selectedAuthors = [], onChange }: RemoveMembersPanelListProps) => {
  const { site } = useSiteContext()
  const { t } = useTranslation("createProposal")

  const [search, setSearch] = useState("")
  const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

  const { data: publishers = [] } = useSearchAuthors(debouncedSearch)

  const searchItems = useMemo<DropdownItem[]>(
    () =>
      publishers
        .filter(x => selectedAuthors.every(a => a.id !== x.id)) // Do not display authors that have already been selected
        .filter(x => site?.authorsIds.some(m => m === x.id)) // Do not display authors who are not publishers
        .map(x => ({ label: x.name ?? x.id, value: x.id, avatarId: x.avatarId })) || [],
    [publishers, selectedAuthors, site?.authorsIds],
  )

  const selectedItems = useMemo(
    () =>
      selectedAuthors.map(({ id, name, avatarId }) => ({
        id,
        title: name ?? id,
        avatarId,
      })),
    [selectedAuthors],
  )

  const handleSearchItemSelect = useCallback(
    (item: DropdownItem) => {
      const accountToAdd: AuthorBaseAvatar = {
        id: item.value,
        name: item.label,
        avatarId: item.avatarId,
      }
      onChange([...selectedAuthors, accountToAdd])
      setSearch("")
    },
    [onChange, selectedAuthors],
  )

  const handleSelectedItemRemove = useCallback(
    (id: string) => {
      onChange(selectedAuthors.filter(x => x.id !== id))
    },
    [onChange, selectedAuthors],
  )

  return (
    <MembersPanelList
      t={t}
      modeType="remove"
      memberType="author"
      searchItems={searchItems}
      search={search}
      selectedItems={selectedItems}
      onSearchChange={setSearch}
      onSearchItemSelect={handleSearchItemSelect}
      onSelectedItemRemove={handleSelectedItemRemove}
    />
  )
})
