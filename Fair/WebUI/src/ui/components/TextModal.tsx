import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps, MultilineText } from "ui/components"

export type ModalSize = "default" | "compact"

type TextModalBaseProps = {
  size?: ModalSize
  cancelLabel?: string
  confirmLabel?: string
  text: string
  onCancel?: () => void
  onConfirm?: () => void
}

export type TextModalProps = Pick<ModalProps, "className" | "title"> & TextModalBaseProps

export const TextModal = memo(
  ({ className, title, size = "default", text, onConfirm, onCancel, confirmLabel, cancelLabel }: TextModalProps) => {
    const handleClose = onCancel ?? onConfirm

    useEscapeKey(handleClose)

    return (
      <Modal
        className={twMerge("w-125 gap-6 p-8", size === "compact" && "w-135 gap-4 p-6", className)}
        title={title}
        onClose={handleClose}
      >
        <div className="max-h-[60vh] overflow-y-auto pr-1 text-sm">
          <MultilineText>{text}</MultilineText>
        </div>
        <div className={twMerge("mt-6 flex justify-end gap-4", size === "compact" && "mt-4")}>
          {!!onCancel && !!cancelLabel && (
            <ButtonOutline
              className={twMerge("h-11 min-w-25 capitalize", size === "compact" && "h-9 w-20 min-w-20")}
              label={cancelLabel}
              onClick={onCancel}
            />
          )}
          {!!onConfirm && !!confirmLabel && (
            <ButtonPrimary
              className={twMerge("h-11 min-w-25 capitalize", size === "compact" && "h-9 w-20 min-w-20")}
              label={confirmLabel}
              onClick={onConfirm}
            />
          )}
        </div>
      </Modal>
    )
  },
)
