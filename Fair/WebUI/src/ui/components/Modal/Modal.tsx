import { memo, MouseEvent, PropsWithChildren, ReactNode, useEffect } from "react"
import { twMerge } from "tailwind-merge"
import { createPortal } from "react-dom"

import { SvgX } from "assets"
import { PropsWithClassName } from "types"

import { Backdrop } from "./Backdrop"

type ModalBaseProps = {
  title?: string
  isBackdropStatic?: boolean
  footer?: ReactNode
  onClose?: () => void
}

export type ModalProps = PropsWithClassName & PropsWithChildren & ModalBaseProps

export const Modal = memo((props: ModalProps) => {
  const { className, children, title, isBackdropStatic, footer, onClose, ...rest } = props

  useEffect(() => {
    document.body.style.overflow = "hidden"
    return () => {
      document.body.style.overflow = ""
    }
  }, [])

  return createPortal(
    <Backdrop onClick={isBackdropStatic ? undefined : onClose}>
      <div
        className={twMerge(
          "items-centner z-61 m-auto flex max-w-190 flex-col justify-center gap-6 bg-white p-8 shadow-md",
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
        <div className="flex-1 overflow-hidden">{children}</div>
        {footer && <div>{footer}</div>}
      </div>
    </Backdrop>,
    document.body,
  )
})
