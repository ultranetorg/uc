import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"

import { useOperationPolicy, useSiteContext } from "app"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { ButtonPrimary, TabContent, TabsList, TabsListItem, TabsProvider } from "ui/components"
import { ModerationHeader } from "ui/components/specific"
import { routes } from "utils"

import { ChangedPublicationsTab } from "./ChangedPublicationsTab"
import { PublicationsTab } from "./PublicationsTab"
import { UnpublishedPublicationsTab } from "./UnpublishedPublicationsTab"

const routeToTabKey: Record<string, string> = {
  proposals: "proposals",
  changed: "changed",
  unpublished: "unpublished",
}

export const PublicationsPage = () => {
  const navigate = useNavigate()
  const { voterId } = useOperationPolicy("publication-creation")
  const { tabKey } = useParams()
  const siteId = useResolveSiteId()
  const { site } = useSiteContext()
  const { t } = useTranslation("publicationsPage")

  useSiteTitle(site?.title, "Publications")

  const key = routeToTabKey[tabKey!]

  const handleSearchProduct = useCallback(
    () => navigate(routes.moderation.createPublication(siteId!)),
    [navigate, siteId],
  )

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) => navigate(routes.moderation.publications(siteId!, item.route)),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "proposals", label: t("common:proposals") },
      { key: "changed", label: t("changed"), route: "changed" },
      { key: "unpublished", label: t("unpublished"), route: "unpublished" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader
        title={t("title")}
        components={
          <>
            {!!voterId && <ButtonPrimary className="w-48" label={t("searchProduct")} onClick={handleSearchProduct} />}
          </>
        }
      />
      <TabsProvider defaultKey={key || "proposals"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="proposals">
            <PublicationsTab />
          </TabContent>
          <TabContent when="changed">
            <ChangedPublicationsTab />
          </TabContent>
          <TabContent when="unpublished">
            <UnpublishedPublicationsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
