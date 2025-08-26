import { DetailedHTMLProps, InputHTMLAttributes } from "react"
import { twMerge } from "tailwind-merge"

export const Radio = ({
  className,
  ...rest
}: DetailedHTMLProps<InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>) => (
  <>
    <input type="radio" className="peer hidden" {...rest} />
    <span
      className={twMerge(
        "flex h-5 w-5 items-center justify-center rounded-full border border-gray-300 peer-checked:border-gray-400",
        className,
      )}
    >
      <span className="h-2 w-2 scale-0 rounded-full bg-gray-800 transition-transform peer-checked:scale-100"></span>
    </span>
  </>
)
