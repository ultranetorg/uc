import { memo } from "react"
import { TFunction } from "i18next"
import { capitalize } from "lodash"

import { MembersChangeType } from "types"
import { AccountsList, AccountsListItemProps } from "ui/components/AccountsList"
import { DropdownItem, DropdownSearchAccount } from "ui/components/proposal"

type PanelListModeType = "add" | "remove"

export type MembersPanelListProps = {
  t: TFunction
  memberType: MembersChangeType
  modeType: PanelListModeType
  searchItems: DropdownItem[]
  search: string
  selectedItems: AccountsListItemProps[]
  onSearchChange: (value: string) => void
  onSearchItemSelect: (item: DropdownItem) => void
  onSelectedItemRemove: (id: string) => void
}

export const MembersPanelList = memo(
  ({
    t,
    memberType,
    modeType,
    searchItems,
    search,
    selectedItems,
    onSearchChange,
    onSearchItemSelect,
    onSelectedItemRemove,
  }: MembersPanelListProps) => (
    <div className="flex flex-col divide-y divide-gray-300 rounded border border-gray-300 bg-gray-100">
      <span className="p-4 text-2sm font-semibold leading-5">
        {t(modeType === "add" ? "selectMembersToAdd" : "selectMembersToRemove", { memberType: capitalize(memberType) })}
      </span>
      <div className="flex flex-col gap-3 p-4">
        <DropdownSearchAccount
          placeholder={memberType === "author" ? t("enterAuthorName") : t("enterModeratorName")}
          items={searchItems}
          inputValue={search}
          onInputChange={onSearchChange}
          onSelect={onSearchItemSelect}
          noOptionsLabel={t("userNotFound")}
        />
        <AccountsList items={selectedItems} onItemRemove={onSelectedItemRemove} />
      </div>
      <span className="px-4 py-2 text-2xs font-medium leading-4">
        {t("selected")}: {selectedItems.length}
      </span>
    </div>
  ),
)
