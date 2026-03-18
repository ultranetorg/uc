import { ChangeEvent, memo, useCallback, useState } from "react"
import { SvgSendSm } from "assets"
import { ButtonPrimary } from "ui/components"

export type CommentInputProps = {
  disabled?: boolean
  loading?: boolean
  placeholder?: string
  onCommentSubmit: (comment: string) => void
}

export const CommentInput = memo(({ disabled, loading, placeholder = "", onCommentSubmit }: CommentInputProps) => {
  const [comment, setComment] = useState("")

  const handleCommentInput = useCallback((e: ChangeEvent<HTMLInputElement>) => setComment(e.target.value), [])

  const handleCommentSubmit = useCallback(() => onCommentSubmit(comment), [comment, onCommentSubmit])

  return (
    <div className="rounded-lg border border-gray-300 bg-gray-100 p-6">
      <div className="flex flex-col gap-4">
        <input
          disabled={loading}
          type="text"
          placeholder={placeholder}
          className="bg-gray-100 outline-none"
          value={comment}
          onInput={handleCommentInput}
        />
        <div className="flex items-center justify-end">
          {/* <ButtonIcon icon={<SvgEmojiLaughing className="stroke-gray-500" />} /> */}
          <ButtonPrimary
            disabled={disabled || comment.trim().length === 0}
            loading={loading}
            className="h-9 w-28 self-end px-4 py-2"
            label="Submit"
            iconAfter={<SvgSendSm className="stroke-white" />}
            onClick={handleCommentSubmit}
          />
        </div>
      </div>
    </div>
  )
})
