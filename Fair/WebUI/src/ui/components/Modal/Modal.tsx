import { memo, MouseEvent, PropsWithChildren, ReactNode, useEffect, useRef } from "react"
import { twMerge } from "tailwind-merge"
import { createPortal } from "react-dom"

import { SvgX } from "assets"
import { useIsMultilineText } from "hooks"
import { PropsWithClassName } from "types"
import { getClass } from "utils"

import { Backdrop } from "./Backdrop"

type ModalBaseProps = {
  headerClassName?: string
  titleClassName?: string
  title?: string
  isBackdropStatic?: boolean
  footer?: ReactNode
  onClose?: () => void
}

export type ModalProps = PropsWithClassName & PropsWithChildren & ModalBaseProps

export const Modal = memo((props: ModalProps) => {
  const { className, children, headerClassName, titleClassName, title, isBackdropStatic, footer, onClose, ...rest } =
    props

  const gapClass = getClass(className, "gap")

  const titleRef = useRef<HTMLDivElement>(null)
  const isMultilineTitle = useIsMultilineText(titleRef, title)

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
          "items-centner z-61 m-auto flex max-w-190 flex-col justify-center gap-6 rounded-2xl bg-white p-8 shadow-md",
          className,
        )}
        onClick={(e: MouseEvent) => e.stopPropagation()}
        {...rest}
      >
        {(title || onClose) && (
          <div
            className={twMerge(
              "flex justify-between gap-2.5",
              isMultilineTitle ? "items-start" : "items-center",
              headerClassName,
            )}
          >
            <div
              ref={titleRef}
              className={twMerge(
                "cursor-default select-none text-2xl font-semibold leading-7 text-gray-800",
                titleClassName,
              )}
            >
              {title}
            </div>
            {onClose && (
              <div className="shrink-0">
                <SvgX onClick={onClose} className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" />
              </div>
            )}
          </div>
        )}
        <div className={twMerge("flex-1 flex-col overflow-hidden", gapClass)}>{children}</div>
        {footer}
      </div>
    </Backdrop>,
    document.body,
  )
})
