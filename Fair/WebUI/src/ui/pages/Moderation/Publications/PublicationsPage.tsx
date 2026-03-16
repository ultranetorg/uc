import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { TabsProvider } from "app"
import { ButtonPrimary, TabContent, TabsList, TabsListItem } from "ui/components"
import { ModerationHeader } from "ui/components/specific"

import { ChangedPublicationsTab } from "./ChangedPublicationsTab"
import { PublicationsTab } from "./PublicationsTab"
import { UnpublishedProductsTab } from "./UnpublishedProductsTab"

const routeToTabKey: Record<string, string> = {
  p: "publications",
  c: "changed",
  u: "unpublished",
}

export const PublicationsPage = () => {
  const navigate = useNavigate()
  const { siteId, tabKey } = useParams()
  const { t } = useTranslation("publicationsPage")

  const key = routeToTabKey[tabKey!]

  const handleSearchProduct = useCallback(() => navigate(`/${siteId}/m/new-publication`), [navigate, siteId])

  const handleTabSelect = useCallback(
    (item: TabsListItem & { route?: string }) =>
      navigate(item.route ? `/${siteId}/m/c/${item.route}` : `/${siteId}/m/c`),
    [navigate, siteId],
  )

  const tabsItems: (TabsListItem & { route?: string })[] = useMemo(
    () => [
      { key: "publications", label: t("common:publications"), route: "p" },
      { key: "changed", label: t("changed"), route: "c" },
      { key: "unpublished", label: t("unpublished"), route: "u" },
    ],
    [t],
  )

  return (
    <>
      <ModerationHeader
        title={t("title")}
        parentBreadcrumbs={{ path: `/${siteId}/m`, title: t("common:proposals") }}
        components={<ButtonPrimary className="w-48" label={t("searchProduct")} onClick={handleSearchProduct} />}
      />
      <TabsProvider defaultKey={key || "publications"}>
        <div className="flex flex-col gap-6">
          <TabsList
            className="flex gap-6 border-b border-y-gray-300 text-2sm leading-4.5 text-gray-500"
            itemClassName="h-6 cursor-pointer hover:text-gray-800 first-letter:uppercase"
            activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
            onTabSelect={handleTabSelect}
            items={tabsItems}
          />

          <TabContent when="changed">
            <ChangedPublicationsTab />
          </TabContent>
          <TabContent when="publications">
            <PublicationsTab />
          </TabContent>
          <TabContent when="unpublished">
            <UnpublishedProductsTab />
          </TabContent>
        </div>
      </TabsProvider>
    </>
  )
}
