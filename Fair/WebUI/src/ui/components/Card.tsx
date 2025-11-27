import { ReactElement } from "react"

export type CardProps = {
  label?: string
  className?: string
  children: ReactElement | ReactElement[]
}

export const Card = ({ children, label, className }: CardProps) => (
  <div className={`flex flex-col gap-6 border border-gray-300 bg-gray-100 p-6 ${className ?? ""}`}>
    {label && <span className="text-xl font-semibold leading-6">{label}</span>}
    {children}
  </div>
)
