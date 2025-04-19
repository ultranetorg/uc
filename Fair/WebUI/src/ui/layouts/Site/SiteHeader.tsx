import { Link, useParams } from "react-router-dom"

import { useSearchContext, useSite } from "app"
import { Button, Input, Logo } from "ui/components"
import { ChatXSvg, GridSvg, PersonKingSvg } from "assets"

export const SiteHeader = () => {
  const { siteId } = useParams()
  const { site } = useSite()
  const { search, setSearch } = useSearchContext()

  if (!site) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 py-8">
      <Link to={`/${siteId}`}>
        <Logo title={site.title} />
      </Link>
      <Button className="gap-2" image={<GridSvg className="stroke-zinc-700" />} label="Categories" />
      <Link to={`/${siteId}/m-d`}>
        <Button className="gap-2" image={<ChatXSvg className="fill-zinc-700 stroke-zinc-700" />} label="Disputes" />
      </Link>
      <Link to={`/${siteId}/m`}>
        <Button className="gap-2" image={<PersonKingSvg className="stroke-zinc-700" />} label="Moderation" />
      </Link>
      <Input placeholder="Search" onChange={setSearch} value={search} className="h-12 flex-grow" />
      <Link to={`/${siteId}/s`}>
        <Button className="gap-2" label="🔎" />
      </Link>
    </div>
  )
}
