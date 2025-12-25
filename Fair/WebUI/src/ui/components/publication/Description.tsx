import { useState } from "react"
import { ButtonGhost, Select } from "ui/components"

export type DescriptionLanguage = {
  language: string
  text: string
}

export type DescriptionProps = {
  text?: string
  descriptions?: DescriptionLanguage[]
  descriptionLabel: string
  showMoreLabel: string
}

export const Description = ({ text, descriptions, descriptionLabel, showMoreLabel }: DescriptionProps) => {
  const hasMultipleLanguages = descriptions && descriptions.length > 1
  const [selectedLanguage, setSelectedLanguage] = useState(descriptions?.[0]?.language ?? "en")

  const currentText = descriptions?.find(d => d.language === selectedLanguage)?.text ?? text ?? ""

  const languageItems = descriptions?.map(d => ({ value: d.language, label: d.language.toUpperCase() })) ?? []

  return (
    <div className="divide-y divide-[#D7DDEB] rounded-lg border border-[#D7DDEB] bg-[#F3F5F8]">
      <div className="flex flex-col gap-6 p-6 text-gray-800">
        <div className="flex items-center justify-between">
          <span className="text-xl font-semibold leading-6">{descriptionLabel}</span>
          {hasMultipleLanguages && (
            <Select
              className="rounded border border-[#D7DDEB] bg-[#F3F5F8] px-3 py-1.5 text-2sm font-medium text-gray-800 outline-none hover:border-gray-400 focus:border-primary"
              items={languageItems}
              value={selectedLanguage}
              onChange={setSelectedLanguage}
            />
          )}
        </div>
        <span className="whitespace-pre-wrap text-2sm leading-5">{currentText}</span>
      </div>
      <div className="py-4 text-center">
        <ButtonGhost className="px-4" label={showMoreLabel} />
      </div>
    </div>
  )
}
