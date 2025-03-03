import { Link, useParams } from "react-router-dom"

import { useGetSite } from "entities"

export const SitePage = () => {
  const { siteId } = useParams()
  const { data: site, isPending } = useGetSite(siteId)

  if (isPending || !site) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col">
      <span>ID: {site.id}</span>
      <span>TITLE: {site.title}</span>
      {site.moderators ? (
        <>
          <span className="text-black">MODERATORS:</span>
          <ul>
            {site.moderators.map(c => (
              <ol key={c.id}>{c.address}</ol>
            ))}
          </ul>
        </>
      ) : (
        <>🚫 NO MODERATORS</>
      )}
      {site.categories ? (
        <>
          <span className="text-black">CATEGORIES:</span>
          <ul>
            {site.categories.map(c => (
              <ol key={c.id}>
                <Link to={`/${siteId}/c/${c.id}`}>{c.title}</Link>
              </ol>
            ))}
          </ul>
        </>
      ) : (
        <>🚫 NO CATEGORIES</>
      )}
    </div>
  )
}
