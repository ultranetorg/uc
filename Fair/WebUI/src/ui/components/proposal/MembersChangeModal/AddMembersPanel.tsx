import { memo, useCallback, useEffect, useState } from "react"
import { TFunction } from "i18next"
import { useDebounceValue } from "usehooks-ts"

import { SvgPlusCircle9xl } from "assets"
import { SEARCH_DELAY } from "config"
import { useSearchAccounts } from "entities"
import { AccountBase } from "types"

import { MembersPanelList } from "../MembersPanelList"
import { MemberType } from "../types"

export type AddMembersPanelProps = {
  t: TFunction
  currentMembers?: AccountBase[]
  isFetching: boolean
  memberType: MemberType
}

export const AddMembersPanel = memo(({ t, currentMembers, isFetching, memberType }: AddMembersPanelProps) => {
  const [clicked, setClicked] = useState(false)

  const [search, setSearch] = useState("")
  const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)
  const [allItems, setAllItems] = useState<AccountBase[] | undefined>()
  const [items, setItems] = useState<AccountBase[]>([])
  const [changedItems, setChangedItems] = useState<AccountBase[]>([])

  const { data } = useSearchAccounts(debouncedSearch)

  const handleActionClick = useCallback(
    (id: string) => {
      const index = items!.findIndex(x => x.id === id)
      // Move to changed items.
      if (index !== -1) {
        const moveToChanged = items![index]
        setChangedItems(p => [...p!, moveToChanged])
        setItems(p => p!.filter((_, i) => i !== index))
      } else {
        // Move from changed items.
        const index = changedItems.findIndex(x => x.id === id)
        const moveFromChanged = changedItems[index]
        setChangedItems(p => p.filter((_, i) => i !== index))

        if (allItems?.some(x => x.id === id)) {
          setItems(p => [...p!, moveFromChanged])
        }
      }
    },
    [changedItems, items, allItems],
  )

  useEffect(() => {
    if (!data) return

    const withoutCurrent = data.filter(item => !currentMembers?.some(member => member.id === item.id))
    setAllItems(withoutCurrent)
    const withoutChanged = withoutCurrent.filter(item => !changedItems.some(changed => changed.id === item.id))
    setItems(withoutChanged)
  }, [data, changedItems, currentMembers])

  return clicked ? (
    <MembersPanelList
      behavior={"add-items"}
      changedItems={changedItems}
      isLoading={false}
      items={items}
      search={search}
      status={t("common:add")}
      title={t("addMembersTitle", { memberType })}
      onActionClick={handleActionClick}
      onSearchChange={setSearch}
    />
  ) : (
    <div
      className="flex h-full w-full cursor-pointer items-center justify-center rounded-lg border border-gray-300 bg-gray-100"
      onClick={() => setClicked(true)}
    >
      <div className="flex flex-col items-center justify-center gap-2">
        <SvgPlusCircle9xl className="fill-gray-500" />
        <span className="text-center text-2base font-medium leading-5">{t("addMembers", { memberType })}</span>
      </div>
    </div>
  )
})
