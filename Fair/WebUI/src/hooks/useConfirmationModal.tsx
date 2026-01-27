import { useCallback } from "react"
import { TextModal, TextModalProps } from "ui/components"
import { useModalOpen, UseModalOpenReturn } from "./useModalOpen"

export type UseConfirmationModalBaseReturn = {
  renderModal: () => JSX.Element | null
}

export type UseConfirmationModalReturn = Pick<UseModalOpenReturn, "open"> & UseConfirmationModalBaseReturn

export type UseConfirmationModalProps = Pick<
  TextModalProps,
  "cancelLabel" | "confirmLabel" | "title" | "text" | "onConfirm"
>

export const useConfirmationModal = ({ ...modalProps }: UseConfirmationModalProps): UseConfirmationModalReturn => {
  const { isOpen, open, close } = useModalOpen()

  const renderModal = useCallback(
    () => (isOpen ? <TextModal onCancel={() => close()} {...modalProps} /> : null),
    [close, isOpen, modalProps],
  )

  return {
    renderModal,
    open,
  }
}
