import { memo, useCallback, useEffect, useState } from "react"
import { TFunction } from "i18next"
import { capitalize } from "lodash"

import { AccountBase } from "types"
import { MembersPanelList } from "ui/components/proposal"

import { MemberType } from "../types"

export type CurrentMembersPanelProps = {
  t: TFunction
  currentMembers?: AccountBase[]
  isFetching: boolean
  memberType: MemberType
}

export const CurrentMembersPanel = memo(({ t, currentMembers, isFetching, memberType }: CurrentMembersPanelProps) => {
  const [search, setSearch] = useState("")
  const [items, setItems] = useState<AccountBase[] | undefined>()
  const [changedItems, setChangedItems] = useState<AccountBase[]>([])
  const [filteredItems, setFilteredItems] = useState<AccountBase[]>([])

  const handleActionClick = useCallback(
    (id: string) => {
      // Move to changed items.
      const itemsIndex = items!.findIndex(x => x.id === id)
      const filteredItemsIndex = filteredItems.findIndex(x => x.id === id)
      if (itemsIndex !== -1 || filteredItemsIndex !== -1) {
        const moveToChanged = !search ? items![itemsIndex] : filteredItems[filteredItemsIndex]
        setChangedItems(p => [...p!, moveToChanged])

        if (search) {
          setFilteredItems(p => p!.filter((_, i) => i !== filteredItemsIndex))
        }

        setItems(p => p!.filter((_, i) => i !== itemsIndex))
      }
      // Move from changed items.
      else {
        const changedItemsIndex = changedItems!.findIndex(x => x.id === id)
        const moveFromChanged = changedItems![changedItemsIndex]
        setChangedItems(p => p!.filter((_, i) => i !== changedItemsIndex))

        if (search) {
          setFilteredItems(p => [...p!, moveFromChanged])
        }

        setItems(p => [...p!, moveFromChanged])
      }
    },
    [changedItems, filteredItems, items, search],
  )

  const handleSearchChange = useCallback(
    (value: string) => {
      if (value && items) {
        const filtered = items.filter(x => x.id.includes(value) || x.nickname?.includes(value))
        setFilteredItems(filtered)
      }
      setSearch(value)
    },
    [items],
  )

  useEffect(() => {
    if (currentMembers) setItems(currentMembers)
  }, [currentMembers])

  return (
    <MembersPanelList
      behavior={"remove-items"}
      changedItems={changedItems}
      isLoading={isFetching}
      items={!search ? items : filteredItems}
      search={search}
      status={t("common:remove")}
      title={t("currentMembersTitle", { memberType: capitalize(memberType), count: currentMembers?.length ?? 0 })}
      onActionClick={handleActionClick}
      onSearchChange={handleSearchChange}
    />
  )
})
