import { memo } from "react"

import { TabsProvider } from "app"

import { EditProfileInfo } from "./EditProfileInfo"
import { ModeratedSites } from "./ModeratedSites"
import { ProfileInfo } from "./ProfileInfo"
import { PublicationsCollapsesLists } from "./PublicationCollapsesList"
import { TabContent } from "./TabContent"
import { TabList } from "./TabList"

// import { PublicationsCollapseProps } from "./PublicationCollapsesList"
// const TEST_ITEMS: Omit<PublicationsCollapseProps, "expanded" | "onExpand">[] = [
//   {
//     id: "abc",
//     nickname: "ThisIsNickname911",
//     title: "Microsoft",
//     items: [
//       {
//         categoryId: "aaa-1",
//         categoryTitle: "Software",
//         publicationId: "aaa-2",
//         publicationTitle: "Visual Studio",
//         publicationsCount: 3,
//       },
//       {
//         categoryId: "bbb-1",
//         categoryTitle: "Software",
//         publicationId: "bbb-2",
//         publicationTitle: "Principle",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "ccc-1",
//         categoryTitle: "Software",
//         publicationId: "ccc-2",
//         publicationTitle: "Software Name v1.4.5",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "ddd-1",
//         categoryTitle: "Software",
//         publicationId: "ddd-2",
//         publicationTitle: "Adobe illustrator 2025",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "eee-1",
//         categoryTitle: "Software",
//         publicationId: "eee-2",
//         publicationTitle: "Visual Studio Legacy",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "fff-1",
//         categoryTitle: "Software",
//         publicationId: "fff-2",
//         publicationTitle: "Chrome 4.5.15",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "ggg-1",
//         categoryTitle: "Software",
//         publicationId: "ggg-2",
//         publicationTitle: "3D Modeling Tool v4.5.0",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "hhh-1",
//         categoryTitle: "Software",
//         publicationId: "hhh-2",
//         publicationTitle: "Graphic Designer Pro",
//         publicationsCount: 0,
//       },
//     ],
//   },
//   { id: "bcd", nickname: "Google", title: "Microsoft", items: [] },
//   {
//     id: "cde",
//     nickname: "Habr",
//     title: "Habrahabr",
//     items: [
//       {
//         categoryId: "iii-1",
//         categoryTitle: "Software",
//         publicationId: "iii-2",
//         publicationTitle: "Adobe illustrator 2025",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "ggg-1",
//         categoryTitle: "Software",
//         publicationId: "ggg-2",
//         publicationTitle: "Sublime Text 4.0",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "kkk-1",
//         categoryTitle: "Software",
//         publicationId: "kkk-2",
//         publicationTitle: "Open vpn",
//         publicationsCount: 0,
//       },
//       {
//         categoryId: "lll-1",
//         categoryTitle: "Software",
//         publicationId: "lll-2",
//         publicationTitle: "Sketch",
//         publicationsCount: 0,
//       },
//     ],
//   },
// ]
// publicationId: string
// publicationTitle: string
// categoryId: string
// categoryTitle: string
// publicationsCount: number

export type ProfileTabsProps = {
  tabsListClassName?: string
  onTabSelect?: (tab: string) => void
}

export const ProfileTabs = memo(({ tabsListClassName, onTabSelect }: ProfileTabsProps) => (
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
        <TabContent when="profileSettings">
          <EditProfileInfo />
        </TabContent>
        <TabContent when="authors">
          <PublicationsCollapsesLists items={[]} />
        </TabContent>
        <TabContent when="moderation">
          <ModeratedSites />
        </TabContent>
      </div>

      <TabList
        className={tabsListClassName}
        items={[
          { key: "profile", label: "Profile" },
          { key: "profileSettings", label: "Profile settings" },
          { key: "authors", label: "My Authors" },
          { key: "moderation", label: "Moderation" },
        ]}
        onTabSelect={onTabSelect}
      />
    </div>
  </TabsProvider>
))
