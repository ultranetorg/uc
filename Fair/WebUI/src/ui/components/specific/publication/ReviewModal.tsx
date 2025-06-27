import { memo, useCallback, useEffect, useState } from "react"

import { SvgCheckCircleFill3XLColored, SvgX } from "assets"
import { ButtonOutline, ButtonPrimary, Modal, ModalProps, Textarea } from "ui/components"

import { YourRatingBar } from "./YourRatingBar"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-800 select-none"

type ReviewModalBaseProps = {
  onSubmit(): void
  cancelLabel: string
  submitLabel: string
  thankYouLabel: string
  writeReviewLabel: string
  yourRatingLabel: string
  yourReviewLabel: string
}

export type ReviewModalProps = ModalProps & ReviewModalBaseProps

export const ReviewModal = memo(
  ({
    title,
    onClose,
    onSubmit,
    cancelLabel,
    submitLabel,
    thankYouLabel,
    writeReviewLabel,
    yourRatingLabel,
    yourReviewLabel,
    ...rest
  }: ReviewModalProps) => {
    const [step, setStep] = useState(0)
    const [reviewText, setReviewText] = useState("")

    useEffect(() => {
      const handleKeyDown = (e: KeyboardEvent) => {
        if (step == 0 && e.key === "Escape") {
          onClose?.()
        }
      }

      document.addEventListener("keydown", handleKeyDown)

      return () => {
        document.removeEventListener("keydown", handleKeyDown)
      }
    }, [onClose, step])

    const handleSubmit = useCallback(() => {
      setStep(1)
      setTimeout(() => {
        onSubmit?.()
      }, 2000)
    }, [onSubmit])

    return (
      <Modal {...rest} className="h-97.5 w-190">
        {step == 0 ? (
          <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between gap-2.5">
              <div className="cursor-default select-none text-2xl font-semibold leading-7 text-gray-800">{title}</div>
              {onClose && <SvgX onClick={onClose} className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" />}
            </div>
            <div className="flex flex-col gap-2">
              <span className={LABEL_CLASSNAME}>{yourRatingLabel}</span>
              <YourRatingBar value={3} />
            </div>
            <div className="flex flex-col gap-2">
              <span className={LABEL_CLASSNAME}>{yourReviewLabel}</span>
              <Textarea onChange={setReviewText} value={reviewText} rows={3} placeholder={writeReviewLabel} />
            </div>
            <div className="flex justify-end gap-6">
              <ButtonOutline className="px-4" label={cancelLabel} onClick={onClose} />
              <ButtonPrimary className="px-6" label={submitLabel} onClick={handleSubmit} />
            </div>
          </div>
        ) : (
          <div className="flex h-full items-center justify-center">
            <div className="flex flex-col items-center gap-6">
              <SvgCheckCircleFill3XLColored />
              <span className="text-2xl font-medium leading-7 text-gray-800">{thankYouLabel}</span>
            </div>
          </div>
        )}
      </Modal>
    )
  },
)
