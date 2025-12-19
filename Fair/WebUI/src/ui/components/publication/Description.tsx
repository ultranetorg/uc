import { useMemo, useState } from "react"

import { ButtonGhost, Select } from "ui/components"

export type DescriptionProps = {
  text: string
  descriptions?: { language: string; text: string }[]
  descriptionLabel: string
  showMoreLabel: string
}

export const Description = ({ text, descriptions, descriptionLabel, showMoreLabel }: DescriptionProps) => {
  const items = useMemo(() => {
    const unique = new Map<string, string>()
    ;(descriptions ?? []).forEach(d => {
      const key = (d.language ?? "").trim() || "unknown"
      if (!unique.has(key) && d.text) unique.set(key, d.text)
    })
    return [...unique.entries()].map(([language]) => ({ value: language, label: language.toUpperCase() }))
  }, [descriptions])

  const defaultLang = items[0]?.value
  const [lang, setLang] = useState<string | undefined>(defaultLang)

  const resolvedText = useMemo(() => {
    if (!descriptions || descriptions.length === 0) return text
    const target = (lang ?? "").toLowerCase()
    const found = descriptions.find(d => (d.language ?? "").toLowerCase() === target)?.text
    return found ?? descriptions[0]?.text ?? text
  }, [descriptions, lang, text])

  return (
    <div className="divide-y divide-gray-300 rounded-lg border border-gray-300 bg-gray-100">
      <div className="flex flex-col gap-6 p-6 text-gray-800">
        <div className="flex items-center justify-between gap-4">
          <span className="text-xl font-semibold leading-6">{descriptionLabel}</span>
          {items.length > 1 && (
            <Select
              className="h-9 rounded border border-gray-300 bg-gray-0 px-3 text-2sm text-gray-800"
              items={items}
              value={lang}
              onChange={setLang}
            />
          )}
        </div>
        <span className="text-2sm leading-5">{resolvedText}</span>
      </div>
      <div className="py-4 text-center">
        <ButtonGhost className="px-4" label={showMoreLabel} />
      </div>
    </div>
  )
}
