import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps, MultilineText } from "ui/components"

export type ButtonLayout = "default" | "compact"

type TextModalBaseProps = {
  buttonLayout?: ButtonLayout
  cancelLabel?: string
  confirmLabel?: string
  text: string
  onCancel?: () => void
  onConfirm?: () => void
}

export type TextModalProps = Pick<ModalProps, "title"> & TextModalBaseProps

export const TextModal = memo(
  ({ buttonLayout = "default", title, text, onConfirm, onCancel, confirmLabel, cancelLabel }: TextModalProps) => {
    const handleClose = onCancel ?? onConfirm

    useEscapeKey(handleClose)

    return (
      <Modal
        className={twMerge("w-135 gap-6 p-8", buttonLayout === "compact" && "gap-4 p-6")}
        title={title}
        onClose={handleClose}
      >
        <div className="max-h-[60vh] overflow-y-auto pr-1 text-sm">
          <MultilineText>{text}</MultilineText>
        </div>
        <div
          className={twMerge(`flex items-center justify-between gap-4`, buttonLayout === "compact" && "justify-end")}
        >
          {!!onCancel && !!cancelLabel && (
            <ButtonOutline
              className={twMerge("h-11 w-full capitalize", buttonLayout === "compact" && "h-9 w-20")}
              label={cancelLabel}
              onClick={onCancel}
            />
          )}
          {!!onConfirm && !!confirmLabel && (
            <ButtonPrimary
              className={twMerge("h-11 w-full capitalize", buttonLayout === "compact" && "h-9 w-20")}
              label={confirmLabel}
              onClick={onConfirm}
            />
          )}
        </div>
      </Modal>
    )
  },
)
