import { memo, useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { capitalize } from "lodash"

import { useGetSiteMembers } from "entities"
import { useEscapeKey } from "hooks"
import { AccountBase } from "types"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

import { MembersPanelList } from "../MembersPanelList"
import { MemberType } from "../types"

type MembersRemoveModalBaseProps = {
  memberType: MemberType
  changedIds?: string[]
  onChange: (value: string | string[]) => void
}

export type MembersRemoveModalProps = ModalProps & MembersRemoveModalBaseProps

export const MembersRemoveModal = memo(
  ({ memberType, changedIds, onClose, onChange, ...rest }: MembersRemoveModalProps) => {
    const { siteId } = useParams()
    const { t } = useTranslation("membersChangeModal")
    useEscapeKey(onClose)

    const { data: currentMembers, isFetching } = useGetSiteMembers(memberType, siteId)

    const [search, setSearch] = useState("")
    const [selected, setSelected] = useState<AccountBase[]>([])
    const [available, setAvailable] = useState<AccountBase[] | undefined>()
    const [filteredItems, setFilteredItems] = useState<AccountBase[]>([])

    const handleActionClick = useCallback(
      (id: string) => {
        // Move to changed items.
        const itemsIndex = available!.findIndex(x => x.id === id)
        const filteredItemsIndex = filteredItems.findIndex(x => x.id === id)
        if (itemsIndex !== -1 || filteredItemsIndex !== -1) {
          const moveToChanged = !search ? available![itemsIndex] : filteredItems[filteredItemsIndex]
          setSelected(p => [...p!, moveToChanged])

          if (search) {
            setFilteredItems(p => p!.filter((_, i) => i !== filteredItemsIndex))
          }

          setAvailable(p => p!.filter((_, i) => i !== itemsIndex))
        }
        // Move from changed items.
        else {
          const changedItemsIndex = selected!.findIndex(x => x.id === id)
          const moveFromChanged = selected![changedItemsIndex]
          setSelected(p => p!.filter((_, i) => i !== changedItemsIndex))

          if (search) {
            setFilteredItems(p => [...p!, moveFromChanged])
          }

          setAvailable(p => [...p!, moveFromChanged])
        }
      },
      [selected, filteredItems, available, search],
    )

    const handleConfirmClick = useCallback(() => {
      const changedIds = selected.map(x => x.id)
      onChange(changedIds)
      onClose?.()
    }, [selected, onChange, onClose])

    const handleSearchChange = useCallback(
      (value: string) => {
        if (value && available) {
          const filtered = available.filter(x => x.id.includes(value) || x.nickname?.includes(value))
          setFilteredItems(filtered)
        }
        setSearch(value)
      },
      [available],
    )

    useEffect(() => {
      if (!currentMembers) return

      const idSet = new Set(changedIds ?? [])
      setSelected(currentMembers.filter(x => idSet.has(x.id)))
      setAvailable(currentMembers.filter(x => !idSet.has(x.id)))
    }, [changedIds, currentMembers])

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
              behavior={"remove-items"}
              changedItems={selected}
              isLoading={isFetching}
              items={!search ? available : filteredItems}
              search={search}
              status={t("common:remove")}
              title={t("currentMembersTitle", {
                memberType: capitalize(memberType),
                count: currentMembers?.length ?? 0,
              })}
              onActionClick={handleActionClick}
              onSearchChange={handleSearchChange}
            />
          </div>
          <div className="flex justify-end gap-6">
            <ButtonOutline className="w-25 capitalize" label={t("common:back")} onClick={onClose} />
            <ButtonPrimary
              className="w-25 capitalize"
              label={t("common:confirm")}
              onClick={handleConfirmClick}
              //disabled={changedItems.length <= 0}
            />
          </div>
        </div>
      </Modal>
    )
  },
)
