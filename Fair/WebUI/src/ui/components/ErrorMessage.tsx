import { memo } from "react"

export type ErrorInfoProps = {
  title: string
  description: string
}

export const ErrorInfo = memo(({ title, description }: ErrorInfoProps) => (
  <div className="flex flex-col gap-4 text-center text-gray-800">
    <span className="text-3.5xl font-semibold leading-10">{title}</span>
    <span className="text-2sm leading-5">{description}</span>
  </div>
))
