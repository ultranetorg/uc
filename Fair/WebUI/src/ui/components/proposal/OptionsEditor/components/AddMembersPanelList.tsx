import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { capitalize } from "lodash"
import { useDebounceValue } from "usehooks-ts"

import { useSiteContext } from "app"
import { SEARCH_DELAY } from "config"
import { useSearchAuthors } from "entities"
import { MembersChangeType } from "types"
import { AccountsList } from "ui/components"
import { DropdownSearchAccount } from "ui/components/proposal"
import { DropdownItem } from "ui/components/proposal/DropdownSearchAccount"

import { AuthorAvatar } from "../types"

export type AddMembersPanelListProps = {
  memberType: MembersChangeType
  value: AuthorAvatar[]
  onChange: (value: AuthorAvatar[]) => void
}

export const AddMembersPanelList = memo(
  ({ memberType, value: selectedAccounts = [], onChange }: AddMembersPanelListProps) => {
    const { t } = useTranslation("createProposal")
    const { site } = useSiteContext()

    const [search, setSearch] = useState("")
    const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)

    const { data: searchResult = [] } = useSearchAuthors(debouncedSearch)

    const accountsListItems = useMemo(
      () =>
        selectedAccounts.map(({ id, name, avatarId }) => ({
          id,
          title: name ?? id,
          avatarId,
        })),
      [selectedAccounts],
    )

    const dropdownSearchItems = useMemo<DropdownItem[]>(
      () =>
        searchResult
          .filter(x => selectedAccounts.every(a => a.id !== x.id)) // Do not display authors that have already been selected
          .filter(x => site?.authorsIds.every(m => m !== x.id)) // Do not display authors who are already authors
          .map(x => ({ label: x.name ?? x.id, value: x.id, avatarId: x.avatarId })) || [],
      [searchResult, selectedAccounts, site?.authorsIds],
    )

    const handleAccountSelect = useCallback(
      (item: DropdownItem) => {
        const accountToAdd: AuthorAvatar = {
          id: item.value,
          name: item.label,
          avatarId: item.avatarId,
        }
        onChange([...selectedAccounts, accountToAdd])
        setSearch("")
      },
      [onChange, selectedAccounts],
    )

    const handleItemRemove = useCallback(
      (id: string) => {
        onChange(selectedAccounts.filter(x => x.id !== id))
      },
      [onChange, selectedAccounts],
    )

    return (
      <div className="flex flex-col divide-y divide-gray-300 rounded border border-gray-300 bg-gray-100">
        <span className="p-4 text-2sm font-semibold leading-5">
          {t("selectMembersToAdd", { memberType: capitalize(memberType) })}
        </span>
        <div className="flex flex-col gap-3 p-4">
          <DropdownSearchAccount
            placeholder={t("enterNickname")}
            items={dropdownSearchItems}
            inputValue={search}
            onInputChange={setSearch}
            onSelect={handleAccountSelect}
            noOptionsLabel={t("userNotFound")}
          />
          <AccountsList items={accountsListItems} onItemRemove={handleItemRemove} />
        </div>
        <span className="px-4 py-2 text-2xs font-medium leading-4">
          {t("selected")}: {selectedAccounts.length}
        </span>
      </div>
    )
  },
)
