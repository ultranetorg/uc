import { memo, useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { capitalize } from "lodash"
import { useDebounceValue } from "usehooks-ts"

import { SEARCH_DELAY } from "config"
import { useGetSiteMembers, useSearchAccounts } from "entities"
import { useEscapeKey } from "hooks"
import { AccountBase } from "types"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

import { MembersPanelList } from "../MembersPanelList"
import { MemberType } from "../types"

type MembersAddModalBaseProps = {
  memberType: MemberType
  changedIds?: string[]
  onChange: (value: string | string[]) => void
}

export type MembersAddModalProps = ModalProps & MembersAddModalBaseProps

export const MembersAddModal = memo(({ memberType, changedIds, onClose, onChange, ...rest }: MembersAddModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("membersChangeModal")
  useEscapeKey(onClose)

  const [search, setSearch] = useState("")
  const [debouncedSearch] = useDebounceValue(search, SEARCH_DELAY)
  const [allItems, setAllItems] = useState<AccountBase[] | undefined>()
  const [items, setItems] = useState<AccountBase[]>([])
  const [changedItems, setChangedItems] = useState<AccountBase[]>([])

  const { data: currentMembers } = useGetSiteMembers(memberType, siteId)
  const { data: searchAccounts } = useSearchAccounts(debouncedSearch)

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

  const handleConfirmClick = useCallback(() => {
    // const ids = changedItems.map(x => x.id)
    // // Added items
    // changedItems.filter(x => !changedIds?.includes(x.id)).map(x => addAccountOrIncrement(x.id, x))
    // // Removed ids
    // changedIds?.filter(x => changedItems.every(y => x !== y.id)).map(decrementOrRemove)
    // onChange(ids)
    // onClose?.()
  }, [changedIds, changedItems, onChange, onClose])

  // useEffect(() => {
  //   const previouslyStoredMembers = changedIds?.map(x => {
  //     if (storedAccounts.has(x)) {
  //       const data = storedAccounts.get(x)
  //       return data!.account
  //     }
  //   })
  //   if (previouslyStoredMembers?.length > 0) {
  //     setChangedItems(previouslyStoredMembers ?? [])
  //   }
  // }, [changedIds, storedAccounts])

  useEffect(() => {
    if (!searchAccounts) return

    const withoutCurrent = searchAccounts.filter(item => !currentMembers?.some(member => member.id === item.id))
    setAllItems(withoutCurrent)
    const withoutChanged = withoutCurrent.filter(item => !changedItems.some(changed => changed.id === item.id))
    setItems(withoutChanged)
  }, [changedItems, currentMembers, searchAccounts])

  return (
    <Modal
      {...rest}
      title={t("title", { memberType: capitalize(memberType) })}
      onClose={onClose}
      className="flex h-170 w-190"
    >
      <div className="flex h-full w-full flex-col gap-6">
        <div className="flex h-full items-center gap-6">
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
        </div>
        <div className="flex justify-end gap-6">
          <ButtonOutline className="w-25 capitalize" label={t("common:back")} onClick={onClose} />
          <ButtonPrimary
            className="w-25 capitalize"
            label={t("common:confirm")}
            onClick={handleConfirmClick}
            disabled={changedItems.length <= 0}
          />
        </div>
      </div>
    </Modal>
  )
})
