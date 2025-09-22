import { memo } from "react"

export type MessageBoxProps = {
  message: string
  type: "warning"
}

export const MessageBox = memo(({ message }: MessageBoxProps) => (
  <span className="rounded-lg border border-[#E8D8B0] bg-[#FFF4DA] p-3 text-2sm leading-5 text-[#733607]">
    {message}
  </span>
))
