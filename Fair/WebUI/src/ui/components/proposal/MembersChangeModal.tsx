import { useEffect } from "react"
import { Modal, ModalProps } from "ui/components"

export type MembersChangeModalProps = ModalProps

export const MembersChangeModal = ({ onClose, ...rest }: MembersChangeModalProps) => {
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose?.()
      }
    }

    document.addEventListener("keydown", handleKeyDown)

    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [onClose])

  return (
    <Modal {...rest} title="hello" onClose={onClose} className="w-220 h-170">
      <div className="flex h-full items-center gap-6">
        <div className="h-full w-full bg-gray-500"></div>
        <div className="h-full w-full bg-gray-500"></div>
      </div>
    </Modal>
  )
}
