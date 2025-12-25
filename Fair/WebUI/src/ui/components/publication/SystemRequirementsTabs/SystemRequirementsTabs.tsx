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
    <div className="flex flex-col gap-4 rounded-lg border border-[#D7DDEB] bg-[#F3F5F8] p-6">
      <span className="text-xl font-semibold leading-6 text-gray-900">{label}</span>
      <TabsProvider defaultKey={tabs[0].key}>
        <TabsList
          className="flex gap-6"
          itemClassName="h-6 cursor-pointer text-2sm leading-4.5 text-gray-500 hover:text-gray-900"
          activeItemClassName="border-box border-b-2 border-gray-900 pb-2 text-gray-900"
          items={tabs}
        />
        {tabs.map(tab => (
          <TabContent when={tab.key} key={tab.key}>
            {tab.sections && tab.sections.length > 0 ? (
              <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                {tab.sections.map(section => (
                  <ContentSection section={section} key={section.key} />
                ))}
              </div>
            ) : null}
          </TabContent>
        ))}
      </TabsProvider>
    </div>
  ) : null,
)
