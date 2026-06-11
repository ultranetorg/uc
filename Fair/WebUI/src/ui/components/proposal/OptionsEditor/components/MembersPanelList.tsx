import { memo } from "react"
import { TFunction } from "i18next"
import { capitalize } from "lodash"

import authorFallback from "assets/fallback/author-16.png"
import userFallback from "assets/fallback/user-16.png"
import { MembersChangeType } from "types"
import { AccountsList, AccountsListItemProps } from "ui/components/AccountsList"
import { DropdownItem, DropdownSearchMember } from "ui/components/proposal"
import { buildUserAvatarUrl, buildFileUrl } from "utils"

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
        <DropdownSearchMember
          placeholder={memberType === "author" ? t("enterAuthorName") : t("enterModeratorName")}
          items={searchItems}
          getAvatarUrl={memberType === "moderator" ? buildUserAvatarUrl : buildFileUrl}
          avatarFallbackSrc={memberType === "moderator" ? userFallback : authorFallback}
          inputValue={search}
          onInputChange={onSearchChange}
          onSelect={onSearchItemSelect}
          noOptionsLabel={t("userNotFound")}
        />
        <AccountsList
          items={selectedItems}
          fallbackSrc={memberType === "moderator" ? userFallback : authorFallback}
          onItemRemove={onSelectedItemRemove}
        />
      </div>
      <span className="px-4 py-2 text-2xs font-medium leading-4">
        {t("selected")}: {selectedItems.length}
      </span>
    </div>
  ),
)
