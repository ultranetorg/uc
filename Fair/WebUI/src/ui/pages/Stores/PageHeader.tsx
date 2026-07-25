import { MultilineText } from "ui/components"

export type PageHeaderProps = {
  title: string
  description: string
}

export const PageHeader = ({ title, description }: PageHeaderProps) => (
  <div className="flex flex-col gap-4 text-center">
    <h1>
      <MultilineText>{title}</MultilineText>
    </h1>
    <h5>
      <MultilineText>{description}</MultilineText>
    </h5>
  </div>
)
