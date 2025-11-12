import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useGetFiles } from "entities"
import { useEscapeKey } from "hooks"
import { Modal, ModalProps, Pagination } from "ui/components"
import { FilesGrid } from "ui/components/specific"

const { VITE_APP_USER_ID: USER_ID } = import.meta.env

type MemberFilesModalBaseProps = {
  onSelect: (id: string) => void
}

export type MemberFilesModalProps = MemberFilesModalBaseProps & ModalProps

export const MemberFilesModal = memo(({ onClose, onSelect }: MemberFilesModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("memberFilesModal")
  useEscapeKey(onClose)

  const [page, setPage] = useState(0)

  const { data: files, isFetching } = useGetFiles(siteId, USER_ID ?? "", page)
  const pagesCount = files?.totalItems && files.totalItems > 0 ? Math.ceil(files.totalItems / DEFAULT_PAGE_SIZE_20) : 0

  const handleSelect = useCallback(
    (id: string) => {
      onSelect?.(id)
      onClose?.()
    },
    [onClose, onSelect],
  )

  return (
    <Modal title={t("title")} onClose={onClose} className="w-205 h-165 max-w-205 flex">
      <div className="flex h-full flex-col gap-3">
        <Pagination className="self-end" onPageChange={setPage} page={page} pagesCount={pagesCount} />
        <div className="flex-1 overflow-y-scroll">
          <FilesGrid isLoading={isFetching} filesIds={files?.items} onSelect={handleSelect} />
        </div>
      </div>
    </Modal>
  )
})
