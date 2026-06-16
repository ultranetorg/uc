import { memo, PropsWithChildren } from "react"

import { Modal, ModalProps } from "ui/components"

type InstallModalBaseProps = {
  isIccpAvailable: boolean
  onSignIn: () => void
}

export type InstallModalProps = PropsWithChildren & Pick<ModalProps, "onClose"> & InstallModalBaseProps

export const InstallModal = memo(({ children, onClose, isIccpAvailable, onSignIn }: InstallModalProps) => (
  <Modal onClose={onClose}>
    {!isIccpAvailable ? (
      children
    ) : (
      <>
        WELCOME TO ULTRANET now you can{" "}
        <span onClick={onSignIn} className="text-red-500">
          log in
        </span>{" "}
        to your account
      </>
    )}
  </Modal>
))
