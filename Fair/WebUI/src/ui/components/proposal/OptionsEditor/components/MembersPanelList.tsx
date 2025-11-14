import { memo, useState } from "react"
import { useTranslation } from "react-i18next"
import { capitalize } from "lodash"

import { AccountsList, AccountsListProps, SearchDropdown } from "ui/components"

import { DropdownSearchAccount } from "./DropdownSearchAccount"

type MembersPanelListBaseProps = {
  changesType: "add" | "remove"
  memberType: string
  title: string
  selectedLabel: string
}

export type MembersPanelListProps = AccountsListProps & MembersPanelListBaseProps

export const MembersPanelList = memo(({ changesType, memberType, items, onItemRemove }: MembersPanelListProps) => {
  const { t } = useTranslation("createProposal")

  const [selectedCount, setSelectedCount] = useState(0)

  const memberTypeUpper = capitalize(memberType)

  return (
    <div className="flex flex-col divide-y divide-gray-300 rounded border border-gray-300 bg-gray-100">
      <span className="p-4 text-2sm font-semibold leading-5">
        {changesType === "add"
          ? t("selectMembersToAdd", { memberType: memberTypeUpper })
          : t("selectMembersToRemove", { memberType: memberTypeUpper })}
      </span>
      <div className="flex flex-col gap-3 p-4">
        <DropdownSearchAccount />
        <AccountsList items={items} onItemRemove={onItemRemove} />
      </div>
      <span className="px-4 py-2 text-2xs font-medium leading-4">
        {t("selected")}: {selectedCount}
      </span>
    </div>
  )
})
