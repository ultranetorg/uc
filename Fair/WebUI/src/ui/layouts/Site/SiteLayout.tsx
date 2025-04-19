import { PropsWithChildren } from "react"
import { Outlet, Link, useParams, useMatch } from "react-router-dom"

import { useSearchContext } from "app"
import { useSearchPublications } from "entities"
import { Modal } from "ui/components"

import { Sidebar } from "./Sidebar"
import { SiteHeader } from "./SiteHeader"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const isSearchPage = useMatch("/:siteId/s")
  const { search, setSearch } = useSearchContext()
  const { isPending, data: publication } = useSearchPublications(siteId, 0, 5, search)

  return (
    <div className="flex min-h-screen w-full divide-x divide-zinc-300">
      <Sidebar className="min-w-52 pr-8" />
      <div className="w-full pl-8">
        <SiteHeader />
        <div className="flex-1">{children ?? <Outlet />}</div>
        {!isSearchPage && search !== "" && !isPending && publication && publication.items.length > 0 && (
          <Modal isOpen={true} isBackdropStatic={false} onClose={() => setSearch("")}>
            <div className="flex cursor-pointer flex-col" onClick={() => setSearch("")}>
              {publication.items.map(p => (
                <div key={p.id} className="">
                  <Link to={`/${siteId}/p/${p.id}`}>{p.productTitle}</Link>
                </div>
              ))}
            </div>
          </Modal>
        )}
      </div>
    </div>
  )
}
