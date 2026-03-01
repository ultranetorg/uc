import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchAuthors } from "entities"
import { AuthorBaseAvatar } from "types"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

import { MembersPanelList } from "./MembersPanelList"

export type AddAuthorPanelListProps = {
  value: AuthorBaseAvatar[]
  onChange: (value: AuthorBaseAvatar[]) => void
}

export const AddAuthorPanelList = memo(({ value: selectedAuthors = [], onChange }: AddAuthorPanelListProps) => {
  const { site } = useSiteContext()
  const { t } = useTranslation("createProposal")

  const [search, setSearch] = useState("")
  const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

  const { data: authors = [] } = useSearchAuthors(debouncedSearch)

  const accountsListItems = useMemo(
    () =>
      selectedAuthors.map(({ id, name, avatarId }) => ({
        id,
        title: name ?? id,
        avatarId,
      })),
    [selectedAuthors],
  )

  const dropdownSearchItems = useMemo<DropdownItem[]>(
    () =>
      authors
        .filter(x => selectedAuthors.every(a => a.id !== x.id)) // Do not display authors that have already been selected
        .filter(x => site?.authorsIds.every(m => m !== x.id)) // Do not display authors who are already authors
        .map(x => ({ label: x.name ?? x.id, value: x.id, avatarId: x.avatarId })) || [],
    [authors, selectedAuthors, site?.authorsIds],
  )

  const handleAccountSelect = useCallback(
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

  const handleItemRemove = useCallback(
    (id: string) => onChange(selectedAuthors.filter(x => x.id !== id)),
    [onChange, selectedAuthors],
  )

  return (
    <MembersPanelList
      t={t}
      memberType="author"
      modeType="add"
      searchItems={dropdownSearchItems}
      search={search}
      selectedItems={accountsListItems}
      onSearchChange={setSearch}
      onSearchItemSelect={handleAccountSelect}
      onSelectedItemRemove={handleItemRemove}
    />
  )
})
