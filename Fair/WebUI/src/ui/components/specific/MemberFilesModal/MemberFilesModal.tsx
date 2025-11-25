import { memo, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetFilesInfinite } from "entities"
import { useEscapeKey, useInfiniteScrollWithPosition } from "hooks"
import { Modal, ModalProps } from "ui/components"
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

  const { data, fetchNextPage, hasNextPage, isFetchingNextPage } = useGetFilesInfinite(siteId, USER_ID)
  const allFiles = data?.pages.flatMap(p => p.items) || []

  const { scrollRef, loaderRef } = useInfiniteScrollWithPosition(
    fetchNextPage,
    hasNextPage && !isFetchingNextPage,
    allFiles.length,
    isFetchingNextPage,
  )

  const handleSelect = useCallback(
    (id: string) => {
      onSelect?.(id)
      onClose?.()
    },
    [onClose, onSelect],
  )

  return (
    <Modal title={t("title")} onClose={onClose} className="flex h-165 w-212.5 max-w-212.5 gap-8">
      <div className="flex h-full flex-col gap-3">
        <div ref={scrollRef} className="flex-1 overflow-y-auto">
          <FilesGrid
            filesIds={allFiles}
            hasNextPage={hasNextPage}
            onSelect={handleSelect}
            ref={loaderRef}
            noFilesLabel={t("noFiles")}
          />
        </div>
      </div>
    </Modal>
  )
})
