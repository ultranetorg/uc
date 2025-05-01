import { useCallback, useMemo, useState } from "react"
import { Link, useNavigate, useParams } from "react-router-dom"

import { useSearchContext, useSiteContext } from "app"
import { useSearchLightPublications } from "entities"
import { Button, Logo, SearchDropdown, SearchDropdownItem } from "ui/components"
import { ChatXSvg, PersonKingSvg } from "assets"

import { CategoriesButton } from "./components"

export const SiteHeader = () => {
  const [query, setQuery] = useState("")
  const [isDropdownHidden, setDropdownHidden] = useState(false)

  const { siteId } = useParams()
  const navigate = useNavigate()
  const { site } = useSiteContext()
  const { search, setSearch } = useSearchContext()
  const { data: publication } = useSearchLightPublications(siteId, query)

  const items = useMemo(() => publication?.items.map(x => ({ id: x.id, value: x.title })) ?? [], [publication])

  const handleChange = useCallback(
    (value: string) => {
      setQuery(value)
      setSearch(value)
    },
    [setSearch],
  )

  const handleKeyDown = useCallback(
    (key: string) => {
      if (key === "Enter" && !!search) {
        setDropdownHidden(true)
        navigate(`/${siteId}/s`)
      }
    },
    [navigate, search, siteId],
  )

  const handleSelectItem = useCallback(
    (item: SearchDropdownItem) => {
      setQuery("")
      navigate(`/${siteId}/p/${item.id}`)
    },
    [siteId, navigate],
  )

  if (!site) {
    return null
  }

  return (
    <div className="flex items-center justify-between gap-8 py-8">
      <Link to={`/${siteId}`}>
        <Logo title={site.title} />
      </Link>
      <CategoriesButton siteId={siteId!} />
      <Link to={`/${siteId}/m-d`}>
        <Button className="gap-2" image={<ChatXSvg className="fill-zinc-700 stroke-zinc-700" />} label="Disputes" />
      </Link>
      <Link to={`/${siteId}/m`}>
        <Button className="gap-2" image={<PersonKingSvg className="stroke-zinc-700" />} label="Moderation" />
      </Link>
      <SearchDropdown
        className="flex-grow"
        isDropdownHidden={isDropdownHidden}
        items={items}
        value={query}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        onSelectItem={handleSelectItem}
      />
    </div>
  )
}
