import { PropsWithChildren } from "react"
import { Outlet, Link, useParams } from "react-router-dom"

import { useGetSite } from "entities"

export const SiteLayout = ({ children }: PropsWithChildren) => {
  const { siteId } = useParams()
  const { data: site, isPending } = useGetSite(siteId)

  if (isPending || !site) {
    return <>LOADING</>
  }

  return (
    <div className="min-h-screen w-full">
      <div className="flex w-full items-center justify-between gap-16 bg-gray-400">
        <h1>
          <Link to="/">🏡 ALL SITES</Link>
        </h1>
        <div className="flex gap-5">
          <h1>
            <Link to="/p">🔍 Search</Link>
          </h1>
        </div>
      </div>
      <div className="flex-1">{children ?? <Outlet />}</div>
    </div>
  )
}
