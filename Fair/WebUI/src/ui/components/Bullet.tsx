import { twMerge } from "tailwind-merge"
import { PropsWithClassName } from "types"

export const Bullet = ({ className }: PropsWithClassName) => (
  <div className={twMerge("h-2 w-2 rounded-full bg-black", className)}></div>
)
