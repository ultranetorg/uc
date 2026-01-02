import { memo } from "react"

import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

type TextModalBaseProps = {
  text: string
  onConfirm: () => void
  confirmLabel: string
  onCancel?: () => void
  cancelLabel?: string
}

export type TextModalProps = Pick<ModalProps, "title"> & TextModalBaseProps

export const TextModal = memo(({ title, text, onConfirm, onCancel, confirmLabel, cancelLabel }: TextModalProps) => {
  const handleClose = onCancel ?? onConfirm
  const hasCancel = !!onCancel && !!cancelLabel

  useEscapeKey(handleClose)

  return (
    <Modal className="w-135 gap-4 p-6" title={title} onClose={handleClose}>
      <div className="max-h-[60vh] overflow-y-auto text-sm pr-1">
        {text}
      </div>
      <div className={`${hasCancel ? "" : "mt-3"} flex items-center justify-end gap-4`}>
        <ButtonPrimary className="h-9 w-20 capitalize" label={confirmLabel} onClick={onConfirm} />
        {hasCancel && (
          <ButtonOutline className="h-9 w-20 capitalize" label={cancelLabel} onClick={onCancel} />
        )}
      </div>
    </Modal>
  )
})
