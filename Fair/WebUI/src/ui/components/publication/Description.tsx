import { useMemo, useState } from "react"
import { DropdownTertiary, ShowMoreButton } from "ui/components"

export type DescriptionLanguage = {
  language: string
  text: string
}

export type DescriptionProps = {
  text?: string
  descriptions?: DescriptionLanguage[]
  descriptionLabel: string
  showMoreLabel: string
  showLessLabel: string
}

export const Description = ({
  text,
  descriptions,
  descriptionLabel,
  showMoreLabel,
  showLessLabel,
}: DescriptionProps) => {
  const hasMultipleLanguages = descriptions && descriptions.length > 1
  const [selectedLanguage, setSelectedLanguage] = useState(descriptions?.[0]?.language ?? "en")
  const [expanded, setExpanded] = useState(false)

  const currentText = useMemo(
    () => descriptions?.find(d => d.language === selectedLanguage)?.text ?? text ?? "",
    [descriptions, selectedLanguage, text],
  )
  const isLong = currentText.length > 400
  const displayedText = useMemo(
    () => (isLong && !expanded ? `${currentText.slice(0, 320)}...` : currentText),
    [currentText, isLong, expanded],
  )

  const languageItems = descriptions?.map(d => ({ value: d.language, label: d.language.toUpperCase() })) ?? []

  return (
    <div className="divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
      <div className="flex flex-col gap-6 p-6 text-gray-800">
        <div className="flex items-center justify-between">
          <span className="text-xl font-semibold leading-6">{descriptionLabel}</span>
          {hasMultipleLanguages && (
            <DropdownTertiary
              isMulti={false}
              className="w-14"
              controlled={true}
              size="medium"
              items={languageItems}
              value={selectedLanguage}
              onChange={item => setSelectedLanguage(item.value)}
            />
          )}
        </div>
        <div>
          <span className="whitespace-pre-wrap text-2sm leading-5">{displayedText}</span>
        </div>
      </div>
      {isLong && (
        <div className="flex justify-center py-4">
          <ShowMoreButton
            className="px-4 text-2xs text-gray-800"
            isExpanded={expanded}
            onExpand={setExpanded}
            showMoreLabel={showMoreLabel}
            showLessLabel={showLessLabel}
          />
        </div>
      )}
    </div>
  )
}
