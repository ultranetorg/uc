import { memo } from "react"

import { TabsProvider } from "app"

import { TabContent, TabsList, TabsListItem } from "ui/components"

import { ContentSection } from "./ContentSection"
import { SystemRequirementsTabSection } from "./types"

export type SystemRequirementsTab = {
  sections: SystemRequirementsTabSection[]
} & TabsListItem

type SystemRequirementsTabsBaseProps = {
  label: string
  tabs: SystemRequirementsTab[]
}

export type SystemRequirementsTabsProps = SystemRequirementsTabsBaseProps

export const SystemRequirementsTabs = memo(({ label, tabs }: SystemRequirementsTabsProps) =>
  tabs && tabs.length > 0 ? (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
      <span className="text-xl font-semibold leading-6">{label}</span>
      <TabsProvider defaultKey={tabs[0].key}>
        <TabsList
          className="flex gap-6"
          itemClassName="text-2sm leading-4.5 text-gray-500 cursor-pointer h-6"
          activeItemClassName="text-gray-800 border-b-2  pb-2 border-box border-gray-950"
          items={tabs}
        />
        {tabs.map(tab =>
          tab.sections && tab.sections.length > 0 ? (
            <div className="flex gap-8" key={tab.key}>
              <TabContent when={tab.key}>
                {tab.sections.map(section => (
                  <ContentSection section={section} />
                ))}
              </TabContent>
            </div>
          ) : null,
        )}
      </TabsProvider>
    </div>
  ) : null,
)
