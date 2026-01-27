import { memo, useMemo } from "react"
import { useTranslation } from "react-i18next"

import { TabsProvider, useUserContext } from "app"

import { EditProfileInfo } from "./EditProfileInfo"
// import { ModeratedSites } from "./ModeratedSites"
import { ProfileInfo } from "./ProfileInfo"
// import { PublicationsCollapsesLists } from "./PublicationCollapsesList"
import { TabContent } from "./TabContent"
import { TabList } from "./TabList"

export type ProfileTabsProps = {
  defaultTabKey?: string
  tabsListClassName?: string
  onTabSelect: (tab: string) => void
}

export const ProfileTabs = memo(({ defaultTabKey, tabsListClassName, onTabSelect }: ProfileTabsProps) => {
  const { t } = useTranslation("profile")

  const { user } = useUserContext()
  const { isPublisher, isModerator } = useUserContext()

  const roles = useMemo(() => {
    const roleList = []
    if (isPublisher) roleList.push(t("common:author"))
    if (isModerator) roleList.push(t("common:moderator"))
    return roleList.length > 0 ? roleList : undefined
  }, [isPublisher, isModerator, t])

  return (
    <TabsProvider defaultKey={defaultTabKey ?? "profile"}>
      <div className="flex grow gap-8">
        <div className="flex-1">
          <TabContent when="profile">
            {user && (
              <ProfileInfo nickname={user.nickname} address={user.address} roles={roles} onTabSelect={onTabSelect} />
            )}
          </TabContent>
          <TabContent when="profileSettings">
            <EditProfileInfo t={t} nickname={user?.nickname} />
          </TabContent>
          {/*
            <TabContent when="authors">
              <PublicationsCollapsesLists items={[]} />
            </TabContent>
            <TabContent when="moderation">
              <ModeratedSites />
            </TabContent>
          */}
        </div>

        <TabList
          className={tabsListClassName}
          items={[
            { key: "profile", label: "Profile" },
            { key: "profileSettings", label: "Profile settings" },
            // { key: "authors", label: "My Authors" },
            // { key: "moderation", label: "Moderation" },
          ]}
          onTabSelect={onTabSelect}
        />
      </div>
    </TabsProvider>
  )
})
