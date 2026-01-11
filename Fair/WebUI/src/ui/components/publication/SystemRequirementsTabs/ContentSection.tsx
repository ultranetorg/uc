import { memo } from "react"

import { SystemRequirementsTabSection } from "./types"

export type ContentSectionProps = {
  section: SystemRequirementsTabSection
}

export const ContentSection = memo(({ section }: ContentSectionProps) => (
  <div className="flex flex-col gap-4 text-2sm leading-4.5" key={section.key}>
    <span className="font-semibold text-gray-900">{section.name}</span>
    <div className="flex flex-col gap-2">
      {Object.entries(section.values).map(([key, value]) => (
        <div key={key} className="text-gray-900">
          <span>{key}: </span>
          <span>{value}</span>
        </div>
      ))}
    </div>
  </div>
))
