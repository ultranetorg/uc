import { MouseEvent, PropsWithChildren, ReactNode, memo } from "react"
import { twMerge } from "tailwind-merge"
import { createPortal } from "react-dom"

import { PropsWithClassName } from "types"

import { Backdrop } from "./Backdrop"
import { CloseButton } from "./CloseButton"

type ModalBaseProps = {
  title?: string
  isOpen?: boolean
  isBackdropStatic?: boolean
  footer?: ReactNode
  onClose?: () => void
}

export type ModalProps = PropsWithClassName & PropsWithChildren & ModalBaseProps

export const Modal = memo((props: ModalProps) => {
  const { className, children, title, isOpen, isBackdropStatic, footer, onClose, ...rest } = props

  if (!isOpen) {
    return null
  }

  return createPortal(
    <Backdrop onClick={isBackdropStatic ? undefined : onClose}>
      <div
        className={twMerge(
          "items-centner border-dark-blue-100 bg-dark-blue-200 m-auto flex max-h-[890px] min-h-[300px] max-w-[600px] flex-col justify-center gap-6 rounded-xl border p-6 shadow-[0_13px_18px_0_rgba(0,0,0,0.5)]",
          className,
        )}
        onClick={(e: MouseEvent) => e.stopPropagation()}
        {...rest}
      >
        <div className="flex items-center justify-between gap-8">
          <div className="cursor-default select-none text-2xl leading-7">{title}</div>
          {onClose && <CloseButton onClick={onClose} />}
        </div>
        <div className="flex-1">{children}</div>
        {footer && <div>{footer}</div>}
      </div>
    </Backdrop>,
    document.body,
  )
})
