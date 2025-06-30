import { memo } from "react"

import { TabsProvider } from "app"

import { TabList } from "./TabList"
import { TabContent } from "./TabContent"
import { AuthorsList } from "./AuthorsList"
import { ModeratedSites } from "./ModeratedSites"
import { ProfileInfo } from "./ProfileInfo"

export type ProfileTabsProps = {
  tabsListClassName?: string
}

export const ProfileTabs = memo(({ tabsListClassName }: ProfileTabsProps) => {
  return (
    <TabsProvider defaultKey="profile">
      <div className="flex flex-grow gap-8">
        <div className="flex-1">
          <TabContent when="profile">
            <ProfileInfo
              nickname="nickname"
              address="0x292b...CCEE"
              roles={["Publisher", "Moderator"]}
              registrationDay={124}
            />
          </TabContent>
          <TabContent when="authors">
            <AuthorsList />
          </TabContent>
          <TabContent when="moderation">
            <ModeratedSites />
          </TabContent>
        </div>

        <TabList
          className={tabsListClassName}
          items={[
            { key: "profile", label: "Profile" },
            { key: "authors", label: "My Authors" },
            { key: "moderation", label: "Moderation" },
          ]}
        />
      </div>
    </TabsProvider>
  )
})
