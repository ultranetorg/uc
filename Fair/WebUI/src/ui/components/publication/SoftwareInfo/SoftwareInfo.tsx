import { memo, useEffect, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { Link } from "react-router-dom"
import { SvgBoxArrowUpRight, SvgStarXxs } from "assets"
import { DownloadSource, ProductDetails, PublicationDetails } from "types"
import { ButtonPrimary, DropdownSecondary, LinkFullscreen } from "ui/components"
import { formatAverageRating, formatDate, formatSupportedPlatforms, formatUiLanguages, getValue, nameEq } from "utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

export interface SoftwareDownload {
  source: DownloadSource
  link: string
}

export type SoftwareInfoProps = {
  siteId: string
  productOrPublication: ProductDetails | PublicationDetails
  languages?: string[]
  supportedPlatforms?: string[]
  price?: string
  licenseType?: string
  licensingDetailsUrl?: string
  softwareDownloads?: SoftwareDownload[]
  publisherLabel: string
  versionLabel: string
  osLabel: string
  ratingLabel: string
  lastUpdatedLabel: string
  licenseTypeLabel: string
  priceLabel: string
  languagesLabel: string
  downloadFromRdnLabel: string
  downloadFromTorrentLabel: string
  downloadFromIpfsLabel: string
  onVersionChange: (version?: string) => void
}

export const SoftwareInfo = memo(
  ({
    siteId,
    productOrPublication,
    supportedPlatforms,
    price,
    languages,
    licenseType,
    licensingDetailsUrl,
    softwareDownloads,
    publisherLabel,
    versionLabel,
    osLabel,
    ratingLabel,
    lastUpdatedLabel,
    licenseTypeLabel,
    priceLabel,
    languagesLabel,
    downloadFromRdnLabel,
    downloadFromTorrentLabel,
    downloadFromIpfsLabel,
    onVersionChange,
  }: SoftwareInfoProps) => {
    const [selectedVersion, setSelectedVersion] = useState<string | undefined>()

    const fields = productOrPublication.productFields

    const versions = useMemo(() => {
      const releases = (fields ?? []).filter(x => nameEq(x.name, "release"))
      const collected: string[] = []
      for (const r of releases) {
        const v = getValue(r.children, "version")
        if (v) collected.push(String(v))
      }
      const unique = Array.from(new Set(collected)).filter(Boolean)
      return unique
    }, [fields])

    const versionItems = useMemo(() => versions.map(v => ({ value: v, label: v })), [versions])

    const links = useMemo(() => {
      const ipfs = softwareDownloads?.find(x => x.source === "ipfs")?.link?.trim()
      const rdn = softwareDownloads?.find(x => x.source === "rdn")?.link?.trim()
      const torrent = softwareDownloads?.find(x => x.source === "torrent")?.link?.trim()
      if (!ipfs && !rdn && !torrent) return undefined

      return {
        ipfs,
        rdn,
        torrent,
      }
    }, [softwareDownloads])

    useEffect(() => {
      if (!versions.length) {
        setSelectedVersion(undefined)
      } else {
        setSelectedVersion(prev => (prev && versions.includes(prev) ? prev : versions[0]))
      }
      onVersionChange(selectedVersion)
    }, [onVersionChange, selectedVersion, versions])

    return (
      <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{publisherLabel}</span>
          <LinkFullscreen to={`/${siteId}/a/${productOrPublication.authorId}`}>
            <AuthorImageTitle title={productOrPublication.authorTitle} authorFileId={productOrPublication.authorId} />
          </LinkFullscreen>
        </div>

        {versions.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{versionLabel}</span>
            <DropdownSecondary
              isMulti={false}
              className="w-full"
              controlled={true}
              size="medium"
              items={versionItems}
              value={selectedVersion ?? versions[0]}
              onChange={x => setSelectedVersion(x.value!)}
            />
          </div>
        )}

        {supportedPlatforms && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{osLabel}</span>
            <span className={VALUE_CLASSNAME}>{formatSupportedPlatforms(supportedPlatforms)}</span>
          </div>
        )}

        {licenseType && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{licenseTypeLabel}</span>
            {licensingDetailsUrl ? (
              <Link
                className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}
                to={licensingDetailsUrl}
                title={licensingDetailsUrl}
              >
                {licenseType} <SvgBoxArrowUpRight className="fill-gray-800" />
              </Link>
            ) : (
              <span className={VALUE_CLASSNAME}>{licenseType}</span>
            )}
          </div>
        )}

        {"rating" in productOrPublication && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
            <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
              {formatAverageRating(productOrPublication.rating)} <SvgStarXxs className="fill-favorite" />
            </div>
          </div>
        )}

        {price && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{priceLabel}</span>
            <span className={VALUE_CLASSNAME}>{price}</span>
          </div>
        )}

        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
          <span className={VALUE_CLASSNAME}>{formatDate(productOrPublication.updated)}</span>
        </div>

        {languages && languages.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{languagesLabel}</span>
            <span className={VALUE_CLASSNAME}>{formatUiLanguages(languages)}</span>
          </div>
        )}

        {links && (
          <div className="flex flex-col gap-4">
            {links.rdn && (
              <Link to={links.rdn}>
                <ButtonPrimary className="w-full" label={downloadFromRdnLabel} />
              </Link>
            )}
            {links.torrent && (
              <Link to={links.torrent}>
                <ButtonPrimary className="w-full" label={downloadFromTorrentLabel} />
              </Link>
            )}
            {links.ipfs && (
              <Link to={links.ipfs}>
                <ButtonPrimary className="w-full" label={downloadFromIpfsLabel} />
              </Link>
            )}
          </div>
        )}
      </div>
    )
  },
)
