import { memo } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { useEscapeKey } from "hooks"
import { ButtonOutline, ButtonPrimary, Modal, MultilineText, TextModalProps } from "ui/components"
import { formatRating } from "utils"

export type ReviewModalProps = TextModalProps & {
  rating: number
}

export const ReviewModal = memo(
  ({
    className,
    title,
    size = "default",
    text,
    rating,
    onConfirm,
    onCancel,
    onClose,
    confirmLabel,
    cancelLabel,
  }: ReviewModalProps) => {
    const { t } = useTranslation()
    const handleClose = onClose ?? onCancel ?? onConfirm

    useEscapeKey(handleClose)

    return (
      <Modal
        className={twMerge("w-125 gap-6 p-8", size === "compact" && "w-135 gap-4 p-6", className)}
        title={title}
        onClose={handleClose}
      >
        <div className="flex items-center gap-1 text-sm font-medium">
          {t("labels:rating")} {formatRating(rating)} <SvgStarXxs className="fill-favorite" />
        </div>
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
