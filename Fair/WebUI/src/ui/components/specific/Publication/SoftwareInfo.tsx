import { SvgStarXxs } from "assets"
import { twMerge } from "tailwind-merge"
import { TEST_PUBLISHER_SRC } from "testConfig"

import { PublicationDetails } from "types"
import { ButtonPrimary } from "ui/components/ButtonPrimary"
import { formatAverageRating } from "utils"

const labelClassName = "leading-4 text-gray-500 text-2xs"
const valueClassName = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

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
      <span className={labelClassName}>{publisherLabel}</span>
      <div className="flex items-center gap-2">
        <div className="h-8 w-8 overflow-hidden rounded-full">
          <img src={TEST_PUBLISHER_SRC} className="h-full w-full object-cover" />
        </div>
        <span className={twMerge(valueClassName, "cursor-pointer hover:font-semibold")}>{publication.title}</span>
      </div>
    </div>

    <div className="flex flex-col gap-2">
      <span className={labelClassName}>{versionLabel}</span>
      <span className={valueClassName}>2.16.4.1870</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={labelClassName}>{activationLabel}</span>
      <span className={valueClassName}>Free</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={labelClassName}>{osLabel}</span>
      <span className={valueClassName}>Windows / MacOS / Linux</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={labelClassName}>{ratingLabel}</span>
      <div className={twMerge(valueClassName, "flex items-center gap-1")}>
        {formatAverageRating(publication.averageRating)} <SvgStarXxs className="fill-favorite" />
      </div>
    </div>

    <div className="flex flex-col gap-2">
      <span className={labelClassName}>{lastUpdatedLabel}</span>
      <span className={valueClassName}>{publication.productUpdated}</span>
    </div>

    <div className="flex flex-col gap-4">
      <ButtonPrimary label="Download from RDN" />
      <ButtonPrimary label="Download from torrent" />
      <ButtonPrimary label="Download from torrent" />
      <ButtonPrimary label="Download from web" />
    </div>
  </div>
)
