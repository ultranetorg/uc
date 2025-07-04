import { memo } from "react"

import { SystemRequirementsTabSection } from "./types"

export type ContentSectionProps = {
  section: SystemRequirementsTabSection
}

export const ContentSection = memo(({ section }: ContentSectionProps) => (
  <div className="flex flex-col gap-4 text-2sm leading-4.5" key={section.key}>
    <span className="font-semibold">{section.name}</span>
    {Object.entries(section.values).map(([key, value]) => (
      <div key={key}>
        <span className="font-medium">{key}:</span>
        &nbsp;
        <span>{value}</span>
      </div>
    ))}
  </div>
))
