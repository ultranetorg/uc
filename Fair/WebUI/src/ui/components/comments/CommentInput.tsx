import { SvgEmojiLaughing, SvgSendSm } from "assets"
import { ButtonIcon, ButtonPrimary } from "ui/components"

export type CommentInputProps = {
  placeholder?: string
}

export const CommentInput = ({ placeholder = "" }: CommentInputProps) => (
  <div className="rounded-lg border border-gray-300 bg-gray-100 p-6">
    <div className="flex flex-col gap-4">
      <input type="text" placeholder={placeholder} className="bg-gray-100 outline-none" />
      <div className="flex items-center justify-between">
        <ButtonIcon icon={<SvgEmojiLaughing className="stroke-gray-500" />} />
        <ButtonPrimary
          className="h-9 w-28 self-end px-4 py-2"
          label="Submit"
          iconAfter={<SvgSendSm className="stroke-white" />}
        />
      </div>
    </div>
  </div>
)
