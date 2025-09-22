import { DetailedHTMLProps, InputHTMLAttributes } from "react"
import { twMerge } from "tailwind-merge"
import { PropsWithClassName } from "types"

export type RadioProps = PropsWithClassName &
  Pick<DetailedHTMLProps<InputHTMLAttributes<HTMLInputElement>, HTMLInputElement>, "checked">

export const Radio = ({ className, checked, ...rest }: RadioProps) => (
  <>
    <input type="radio" className="hidden" defaultChecked={checked} {...rest} />
    <span
      className={twMerge(
        "flex max-h-5 min-h-5 min-w-5 max-w-5 items-center justify-center rounded-full border border-gray-300",
        checked && "border-gray-400",
        className,
      )}
    >
      {checked && <span className="h-2.5 w-2.5 rounded-full bg-gray-800" />}
    </span>
  </>
)
