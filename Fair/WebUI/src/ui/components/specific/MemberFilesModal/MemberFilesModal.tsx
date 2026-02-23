import { memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { TabsProvider, useUserContext } from "app"
import { SvgSpinnerXl } from "assets"
import { useGetSiteFilesInfinite } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { useInfiniteScrollWithPosition } from "hooks"
import { File as FileType, FileDeletion, FileCreation, MimeType } from "types"
import { Modal, ModalProps, TabContent, TabsList, TabsListItem, TextModal } from "ui/components"
import { FilesGrid } from "ui/components/specific"
import { fileToBase64, showToast } from "utils"

import { ModalFooter } from "./ModalFooter"
import { UploadZone } from "./UploadZone"

type MemberFilesModalBaseProps = {
  onSelect: (id: string) => void
}

export type MemberFilesModalProps = MemberFilesModalBaseProps & ModalProps

export const MemberFilesModal = memo(({ onClose, onSelect }: MemberFilesModalProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("memberFilesModal")
  const { user } = useUserContext()
  const { mutate } = useTransactMutationWithStatus()

  const [activeTab, setActiveTab] = useState("uploaded")
  const [selectedFile, setSelectedFile] = useState<FileType | undefined>()
  const [removeModalOpen, setRemoveModalOpen] = useState(false)

  const { data, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, error, refetch } =
    useGetSiteFilesInfinite(siteId)
  const allFiles = data?.pages.flatMap(p => p.items) || []

  const tabsItems: TabsListItem[] = useMemo(
    () => [
      { key: "uploaded", label: t("uploaded") },
      { key: "uploading", label: t("uploading") },
    ],
    [t],
  )

  const handleTabSelect = useCallback(() => setSelectedFile(undefined), [])

  const handleFileSelect = useCallback((file: FileType) => setSelectedFile(file), [])

  const handleSelectClick = useCallback(() => {
    onSelect(selectedFile!.id)
    onClose?.()
  }, [onClose, onSelect, selectedFile])

  const handleRemoveClick = useCallback(() => setRemoveModalOpen(true), [])

  const handleRemoveConfirmClick = useCallback(() => {
    const fileId = selectedFile!.id
    const operation = new FileDeletion(fileId)
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:fileDeleted", { fileId }), "success")
        setRemoveModalOpen(false)
        refetch()
      },
      onError: err => {
        showToast(err.toString(), "error")
      },
    })
  }, [mutate, refetch, selectedFile, t])

  const handleUpload = useCallback(
    async (file: File) => {
      const data = await fileToBase64(file)
      const mimeType: MimeType = file.type === "image/png" ? "ImagePng" : "ImageJpg"

      const operation = new FileCreation(siteId!, data, mimeType)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:fileUploaded", { fileName: file.name }), "success")
          setActiveTab("uploaded")
          refetch()
        },
        onError: err => {
          showToast(err.toString(), "error")
        },
      })
    },
    [siteId, mutate, t, refetch],
  )

  const { scrollRef, loaderRef } = useInfiniteScrollWithPosition(
    fetchNextPage,
    hasNextPage && !isFetchingNextPage,
    allFiles.length,
    isFetchingNextPage,
  )

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        if (removeModalOpen) {
          setRemoveModalOpen(false)
        } else {
          onClose?.()
        }
      }
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [onClose, removeModalOpen])

  return (
    <>
      <Modal
        title={t("title")}
        onClose={onClose}
        className="h-165 w-205 max-w-205 gap-0 overflow-hidden p-0 pt-6"
        headerClassName="px-6"
        footer={
          selectedFile && (
            <ModalFooter
              t={t}
              refs={selectedFile.refs}
              onSelectClick={handleSelectClick}
              onRemoveClick={handleRemoveClick}
            />
          )
        }
      >
        {isLoading || error || allFiles.length === 0 ? (
          <div className="flex h-full items-center justify-center">
            {isLoading ? (
              <SvgSpinnerXl className="animate-spin fill-gray-300" />
            ) : error ? (
              error.message
            ) : (
              <UploadZone showEmptyState={true} onUpload={handleUpload} t={t} />
            )}
          </div>
        ) : (
          <TabsProvider defaultKey="uploaded" activeKey={activeTab} onActiveKeyChange={setActiveTab}>
            <div className="mt-6 flex h-full flex-col">
              <TabsList
                className="flex border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
                itemClassName="h-6 cursor-pointer hover:text-gray-800 ml-6"
                activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
                items={tabsItems}
                onTabSelect={handleTabSelect}
              />

              <TabContent when="uploaded">
                <div ref={scrollRef} className="h-full overflow-y-auto p-6 pr-2">
                  <FilesGrid
                    files={allFiles}
                    hasNextPage={hasNextPage}
                    selectedFileId={selectedFile?.id}
                    onSelect={handleFileSelect}
                    notUsedLabel={t("notUsed")}
                    usedLabel={t("used")}
                    ref={loaderRef}
                  />
                </div>
              </TabContent>
              <TabContent when="uploading">
                <UploadZone showEmptyState={false} onUpload={handleUpload} t={t} />
              </TabContent>
            </div>
          </TabsProvider>
        )}
      </Modal>
      {removeModalOpen && (
        <TextModal
          title={t("removeImage")}
          text={t("removeMessage")}
          onCancel={() => setRemoveModalOpen(false)}
          onConfirm={handleRemoveConfirmClick}
          cancelLabel={t("common:cancel")}
          confirmLabel={t("common:remove")}
        />
      )}
    </>
  )
})
