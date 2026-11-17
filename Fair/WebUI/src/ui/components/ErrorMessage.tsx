import { memo } from "react"

export type ErrorInfoProps = {
  title: string
  description: string
}

export const ErrorInfo = memo(({ title, description }: ErrorInfoProps) => (
  <div className="flex flex-col gap-4 px-4 text-center">
    <span className="text-2xl leading-8 font-semibold sm:text-3.5xl sm:leading-10">{title}</span>
    <span className="text-2sm leading-5">{description}</span>
  </div>
))
