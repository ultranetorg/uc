import { memo } from "react"

import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps } from "ui/components"

type TextModalBaseProps = {
  text: string
  onConfirm: () => void
  onCancel: () => void
  confirmLabel: string
  cancelLabel: string
}

export type TextModalProps = Pick<ModalProps, "title"> & TextModalBaseProps

export const TextModal = memo(({ title, text, onConfirm, onCancel, confirmLabel, cancelLabel }: TextModalProps) => {
  useEscapeKey(onCancel)

  return (
    <Modal className="w-135 gap-4 p-6" title={title} onClose={onCancel}>
      <span className="text-sm">{text}</span>
      <div className="flex items-center justify-end gap-4">
        <ButtonPrimary className="h-9 w-20 capitalize" label={confirmLabel} onClick={onConfirm} />
        <ButtonOutline className="h-9 w-20 capitalize" label={cancelLabel} onClick={onCancel} />
      </div>
    </Modal>
  )
})
