import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

type PreviewReviewModalBaseProps = {
  text: string
  onApproveClick: () => void
  onRejectClick: () => void
}

export type PreviewReviewModalProps = Pick<ModalProps, "onClose"> & PreviewReviewModalBaseProps

export const PreviewReviewModal = memo(({ text, onApproveClick, onRejectClick, onClose }: PreviewReviewModalProps) => {
  const { t } = useTranslation("previewReviewModal")
  useEscapeKey(onClose)

  return (
    <Modal className="w-135 gap-4 p-6" title={t("title")} onClose={onClose}>
      <span className="text-sm">{text}</span>
      <div className="flex items-center justify-end gap-4">
        <ButtonPrimary className="h-9 w-20 capitalize" label={t("common:approve")} onClick={onApproveClick} />
        <ButtonOutline className="h-9 w-20 capitalize" label={t("common:reject")} onClick={onRejectClick} />
      </div>
    </Modal>
  )
})
