import { memo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetAuthorFiles } from "entities"
import { useEscapeKey } from "hooks"
import { Modal, ModalProps, Pagination } from "ui/components"
import { FilesList } from "ui/components/specific"

export type MemberFilesModalProps = ModalProps

export const MemberFilesModal = memo(({ onClose }: MemberFilesModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("memberFilesModal")
  useEscapeKey(onClose)

  const [page, setPage] = useState(0)

  const { data: files } = useGetAuthorFiles(siteId, "67465-0", page)
  console.log(files)

  return (
    <Modal title={t("title")} onClose={onClose}>
      <Pagination onPageChange={setPage} page={page} pagesCount={10} />
      <FilesList />
    </Modal>
  )
})
