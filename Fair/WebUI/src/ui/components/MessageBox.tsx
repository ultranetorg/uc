import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { PropsWithClassName } from "types"

type MessageBoxType = "default" | "warning"

type MessageBoxBaseProps = {
  message: string
  type?: MessageBoxType
}

export type MessageBoxProps = PropsWithClassName & MessageBoxBaseProps

export const MessageBox = memo(({ className, message, type = "default" }: MessageBoxProps) => (
  <span
    className={twMerge(
      "rounded-lg border border-gray-300 bg-gray-100 p-3 text-2sm leading-5",
      type === "warning" && "border-[#E8D8B0] bg-[#FFF4DA] text-[#733607]",
      className,
    )}
  >
    {message}
  </span>
))
