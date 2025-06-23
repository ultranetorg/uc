import { MouseEvent, PropsWithChildren, ReactNode, memo } from "react"
import { twMerge } from "tailwind-merge"
import { createPortal } from "react-dom"

import { SvgX } from "assets"
import { PropsWithClassName } from "types"

import { Backdrop } from "./Backdrop"

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
          "items-centner max-w-190 m-auto flex flex-col justify-center gap-6 rounded-2xl bg-white p-8 shadow-md",
          className,
        )}
        onClick={(e: MouseEvent) => e.stopPropagation()}
        {...rest}
      >
        {(title || onClose) && (
          <div className="flex items-center justify-between gap-2.5">
            <div className="cursor-default select-none text-2xl font-semibold leading-7 text-gray-800">{title}</div>
            {onClose && <SvgX onClick={onClose} className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" />}
          </div>
        )}
        <div className="flex-1">{children}</div>
        {footer && <div>{footer}</div>}
      </div>
    </Backdrop>,
    document.body,
  )
})
