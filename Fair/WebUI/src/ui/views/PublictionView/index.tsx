import { UseQueryResult } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { ProductFieldViewModel } from "types"
import { SpinnerRowSvg } from "assets"
import { usePublicationOwner } from "ui/views/Proposal/providers/publicationOwner"
import { PublicationViewHeader } from "./components/PublicationViewHeader"
import { toPublicationData } from "./utils"
import { PublicationViewDescription } from "./components/PublicationViewDescription"
import { PublicationViewMeta } from "./components/PublicationViewMeta"
import PublicationViewArts, { MediaItem } from "./components/PublicationViewArts"
import { PublicationViewRelease } from "./components/PublicationViewRelease"

export interface PublicationViewProps {
  response: UseQueryResult<ProductFieldViewModel[], Error>
}

const Loader = (locale: string) => (
  <div className="flex items-center gap-2 text-slate-500">
    <SpinnerRowSvg />
    {locale}
  </div>
)
const NoData = (locale: string) => <div className="flex items-center gap-2 text-slate-500">{locale}</div>
const Error = (locale: string) => <div className="text-red-700">{locale}</div>

export const PublicationView = ({ response }: PublicationViewProps) => {
  const { t } = useTranslation("publicationView")
  const { error, data, isPending } = response
  const owner = usePublicationOwner()

  if (isPending) {
    return Loader(t("loading"))
  }

  if (error) {
    return Error(t("loadError"))
  }

  if (!data?.length) {
    return NoData(t("noData"))
  }

  const publication = toPublicationData(data)
  const contents = publication.arts.map(
    art =>
      ({
        src: art.video.uri,
        poster: art.screenshot.uri,
        title: art.screenshot.description,
        description: art.video.description,
      }) as MediaItem,
  )

  return (
    <div className="flex flex-col gap-4 p-6">
      <PublicationViewHeader
        logo={publication.logo}
        title={publication.title}
        tags={publication.tags}
        slogan={publication.slogan}
      />
      <div className="flex gap-4">
        <div className="flex flex-col gap-6">
          <PublicationViewArts items={contents} />
          <PublicationViewDescription data={publication.descriptionMin} label="descriptionMin" />
          <PublicationViewDescription data={publication.descriptionMax} label="descriptionMax" />
        </div>
        <div>
          <PublicationViewMeta
            owner={owner}
            version={publication.metadata.version}
            price={publication.price}
            license={publication.license}
            uri={publication.uri}
          />
          <PublicationViewRelease data={publication.releases} />
        </div>
      </div>
    </div>
  )
}
