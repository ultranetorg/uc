import { InputHTMLAttributes, memo } from "react"

export type ButtonBaseProps = {
  onClick: () => void
}

export type ButtonProps = Pick<InputHTMLAttributes<HTMLInputElement>, "value"> & ButtonBaseProps

export const Button = memo(({ value, onClick }: ButtonProps) => (
  <input type="button" className="bg-red-400" onClick={onClick} value={value} />
))
