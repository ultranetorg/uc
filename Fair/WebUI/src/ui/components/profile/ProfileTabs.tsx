import { memo } from "react"

import { TabsProvider } from "app"

import { TabList } from "./TabList"
import { TabContent } from "./TabContent"

export type ProfileTabsProps = {
  tabsListClassName?: string
}

export const ProfileTabs = memo(({ tabsListClassName }: ProfileTabsProps) => {
  return (
    <TabsProvider defaultKey="profile">
      <div className="flex flex-grow gap-8">
        <div className="flex-1">
          <TabContent when="profile">👤 Profile info</TabContent>
          <TabContent when="settings">⚙️ Settings</TabContent>
          <TabContent when="authors">✍️ Authors</TabContent>
          <TabContent when="moderation">🛡️ Moderation</TabContent>
        </div>

        <TabList
          className={tabsListClassName}
          items={[
            { key: "profile", label: "Profile" },
            { key: "settings", label: "Settings" },
            { key: "authors", label: "My Authors" },
            { key: "moderation", label: "Moderation" },
          ]}
        />
      </div>
    </TabsProvider>
  )
})
