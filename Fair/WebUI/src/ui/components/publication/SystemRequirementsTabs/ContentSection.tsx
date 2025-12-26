import { memo } from "react"

import { SystemRequirementsTabSection } from "./types"

export type ContentSectionProps = {
  section: SystemRequirementsTabSection
}

export const ContentSection = memo(({ section }: ContentSectionProps) => (
  <div className="flex flex-col gap-4 text-2sm leading-4.5" key={section.key}>
    <span className="font-semibold text-gray-900">{section.name}</span>
    <div className="grid grid-cols-2 gap-x-6 gap-y-2">
      {Object.entries(section.values).map(([key, value]) => (
        <div key={key} className="contents">
          <span className="text-gray-500">{key}</span>
          <span className="text-gray-900">{value}</span>
        </div>
      ))}
    </div>
  </div>
))
