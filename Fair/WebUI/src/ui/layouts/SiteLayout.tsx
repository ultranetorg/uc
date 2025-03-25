import { PropsWithChildren } from "react"
import { Outlet, Link, useParams, useMatch } from "react-router-dom"

import { useSearchContext } from "app"
import { useSearchPublications } from "entities"
import { Input, Modal } from "ui/components"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const isSearchPage = useMatch("/:siteId/s")
  const { search, setSearch } = useSearchContext()
  const { isPending, data: publication } = useSearchPublications(siteId, 0, 5, search)

  return (
    <div className="min-h-screen w-full">
      <div className="flex w-full items-center justify-between gap-16 bg-gray-400">
        <h1>
          <Link to="/">📃 ALL SITES</Link>
          <Link to={`/${siteId}`}>🏡 CURRENT SITE</Link>
        </h1>
        <div className="flex gap-5">
          <Link to={`/${siteId}/a-r`}>⚖️ Referendums</Link>
          <Link to={`/${siteId}/m-d`}>🔥 Disputes</Link>
          <Link to={`/${siteId}/m`}>🔨 Moderation</Link>
          <Input placeholder="Search" onChange={setSearch} value={search} />
          <Link to={`/${siteId}/s`}>🔎</Link>
        </div>
      </div>
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
  )
}
