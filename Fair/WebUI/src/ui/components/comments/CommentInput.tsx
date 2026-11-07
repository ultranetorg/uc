import { ChangeEvent, memo, useCallback, useState } from "react"
import { SvgSendSm } from "assets"
import { ButtonOutline, ButtonPrimary } from "ui/components"

export type CommentInputProps = {
  disabled?: boolean
  loading?: boolean
  placeholder?: string
  initialText?: string
  submitLabel?: string
  cancelLabel?: string
  onCancel?: () => void
  onCommentSubmit: (comment: string) => void
}

export const CommentInput = memo(
  ({
    disabled,
    loading,
    placeholder = "",
    initialText,
    submitLabel = "Submit",
    cancelLabel = "Cancel",
    onCancel,
    onCommentSubmit,
  }: CommentInputProps) => {
    const [comment, setComment] = useState(initialText ?? "")

    const handleCommentInput = useCallback((e: ChangeEvent<HTMLInputElement>) => setComment(e.target.value), [])

    const handleCommentSubmit = useCallback(() => onCommentSubmit(comment), [comment, onCommentSubmit])

    const isUnchanged = initialText !== undefined && comment.trim() === initialText.trim()

    return (
      <div className="rounded-lg border border-gray-300 bg-gray-100 p-6">
        <div className="flex flex-col gap-4">
          <input
            disabled={loading}
            type="text"
            placeholder={placeholder}
            className="bg-gray-100 outline-hidden"
            value={comment}
            onChange={handleCommentInput}
          />
          <div className="flex items-center justify-end gap-2">
            {/* <ButtonIcon icon={<SvgEmojiLaughing className="stroke-gray-500" />} /> */}
            {onCancel && <ButtonOutline className="h-9" label={cancelLabel} disabled={loading} onClick={onCancel} />}
            <ButtonPrimary
              disabled={disabled || comment.trim().length === 0 || isUnchanged}
              loading={loading}
              className="h-9 min-w-28 self-end px-4 py-2"
              label={submitLabel}
              iconAfter={<SvgSendSm className="stroke-white" />}
              onClick={handleCommentSubmit}
            />
          </div>
        </div>
      </div>
    )
  },
)
