import { memo, useEffect, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { Link } from "react-router-dom"
import { SvgBoxArrowUpRight, SvgStarXxs } from "assets"
import { DownloadSource, ProductDetails, PublicationDetails } from "types"
import { DropdownSecondary, LinkFullscreen } from "ui/components"
import { formatDate, formatSupportedPlatforms, formatUiLanguages, getValue, nameEq, routes } from "utils"

import { AuthorImageTitle, DownloadLinks } from "./components"

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
  noRatingsLabel: string
  lastUpdatedLabel: string
  licenseTypeLabel: string
  priceLabel: string
  languagesLabel: string
  downloadFromRdnLabel: string
  downloadFromTorrentLabel: string
  downloadFromIpfsLabel: string
  downloadFromWebLabel: string
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
    noRatingsLabel,
    lastUpdatedLabel,
    licenseTypeLabel,
    priceLabel,
    languagesLabel,
    downloadFromRdnLabel,
    downloadFromTorrentLabel,
    downloadFromIpfsLabel,
    downloadFromWebLabel,
    onVersionChange,
  }: SoftwareInfoProps) => {
    const [selectedVersion, setSelectedVersion] = useState<string | undefined>()

    const fields = productOrPublication.fields

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
      const ipfs = softwareDownloads?.filter(x => x.source === "ipfs").map(x => x.link.trim()) ?? []
      const rdn = softwareDownloads?.filter(x => x.source === "rdn").map(x => x.link.trim()) ?? []
      const torrent = softwareDownloads?.filter(x => x.source === "torrent").map(x => x.link.trim()) ?? []
      const web = softwareDownloads?.filter(x => x.source === "web").map(x => x.link.trim()) ?? []

      if (!ipfs.length && !rdn.length && !rdn.length && !web.length) return undefined

      return {
        ipfs,
        rdn,
        torrent,
        web,
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
          <LinkFullscreen to={routes.publisher(siteId, productOrPublication.authorId)}>
            <AuthorImageTitle
              title={productOrPublication.authorTitle}
              authorFileId={productOrPublication.authorLogoId}
            />
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

        {supportedPlatforms && supportedPlatforms.length > 0 && (
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

        {"rating" in productOrPublication && productOrPublication.rating !== undefined && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
            <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
              {productOrPublication.rating !== 0 ? (
                <>
                  {productOrPublication.rating} <SvgStarXxs className="fill-favorite" />
                </>
              ) : (
                noRatingsLabel
              )}
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
            <span className="text-2sm font-medium leading-5 text-gray-800">{formatUiLanguages(languages)}</span>
          </div>
        )}

        {links && (
          <div className="flex flex-col gap-4">
            <DownloadLinks links={links.rdn} label={downloadFromRdnLabel} />
            <DownloadLinks links={links.torrent} label={downloadFromTorrentLabel} />
            <DownloadLinks links={links.ipfs} label={downloadFromIpfsLabel} />
            <DownloadLinks links={links.web} label={downloadFromWebLabel} />
          </div>
        )}
      </div>
    )
  },
)
