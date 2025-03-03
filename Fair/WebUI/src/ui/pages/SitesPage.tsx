import { Link } from "react-router-dom"

import { useGetSites } from "entities"

type SiteCardProps = {
  title: string
}

const SiteCard = ({ title }: SiteCardProps) => (
  <div className="flex h-24 w-48 items-center justify-center rounded-md bg-gray-400 hover:font-semibold">{title}</div>
)

export const SitesPage = () => {
  const { data: sites, isPending } = useGetSites()

  if (isPending || !sites || !sites.items) {
    return <div>LOADING</div>
  }

  return (
    <div className="flex flex-col">
      {sites.items.length === 0 ? (
        <>🚫 NO SITES</>
      ) : (
        <div>
          <h1>
            <center>LIST OF ALL SITES</center>
          </h1>
          <div className="flex h-full w-full flex-wrap">
            {sites.items.map(x => (
              <Link key={x.id} to={`/${x.id}`}>
                <SiteCard title={x.title} />
              </Link>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
