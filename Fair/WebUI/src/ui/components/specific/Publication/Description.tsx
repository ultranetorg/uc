import { ButtonGhost } from "ui/components"

export type DescriptionProps = {
  text: string
  descriptionLabel: string
  showMoreLabel: string
}

export const Description = ({ text, descriptionLabel, showMoreLabel }: DescriptionProps) => (
  <div className="divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
    <div className="flex flex-col gap-6 p-6 text-gray-800">
      <span className="text-xl font-semibold leading-6">{descriptionLabel}</span>
      <span className="text-2sm leading-5">{text}</span>
    </div>
    <div className="py-4 text-center">
      <ButtonGhost className="px-4" label={showMoreLabel} />
    </div>
  </div>
)
