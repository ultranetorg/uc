import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { PublicationDetails } from "types"
import { ButtonPrimary, LinkFullscreen } from "ui/components"
import { formatAverageRating } from "utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

export type SoftwareInfoProps = {
  siteId: string
  publication: PublicationDetails
  publisherLabel: string
  versionLabel: string
  activationLabel: string
  osLabel: string
  ratingLabel: string
  lastUpdatedLabel: string
}

export const SoftwareInfo = ({
  siteId,
  publication,
  publisherLabel,
  versionLabel,
  activationLabel,
  osLabel,
  ratingLabel,
  lastUpdatedLabel,
}: SoftwareInfoProps) => (
  <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{publisherLabel}</span>
      <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
        <AuthorImageTitle title={publication.authorTitle} authorAvatar={publication.authorAvatar} />
      </LinkFullscreen>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{versionLabel}</span>
      <span className={VALUE_CLASSNAME}>2.16.4.1870</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{activationLabel}</span>
      <span className={VALUE_CLASSNAME}>Free</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{osLabel}</span>
      <span className={VALUE_CLASSNAME}>Windows / MacOS / Linux</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
      <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
        {formatAverageRating(publication.rating)} <SvgStarXxs className="fill-favorite" />
      </div>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
      <span className={VALUE_CLASSNAME}>{publication.productUpdated}</span>
    </div>

    <div className="flex flex-col gap-4">
      <ButtonPrimary label="Download from RDN" />
      <ButtonPrimary label="Download from torrent" />
      <ButtonPrimary label="Download from torrent" />
      <ButtonPrimary label="Download from web" />
    </div>
  </div>
)
