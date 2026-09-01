import { memo } from "react"

export interface AvatarMenuItemProps {
  text: string
  onClick: () => void
}

export const AvatarMenuItem = memo(({ text, onClick }: AvatarMenuItemProps) => {
  return (
    <div
      className="flex h-10 select-none items-center overflow-hidden truncate px-4 text-2sm leading-5 text-white hover:bg-gray-550"
      onClick={onClick}
    >
      {text}
    </div>
  )
})
