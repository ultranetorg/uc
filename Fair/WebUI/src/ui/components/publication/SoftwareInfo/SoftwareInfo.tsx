import { memo, useEffect, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { PublicationDetails } from "types"
import { ButtonPrimary, DropdownTertiary, LinkFullscreen } from "ui/components"
import { formatAverageRating, formatDate } from "utils"
import { getChildren, getValue, nameEq } from "ui/components/publication/utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

export type SoftwareInfoProps = {
  siteId: string
  publication: PublicationDetails
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
  downloadFromWebLabel: string
}

export const SoftwareInfo = memo(
  ({
    siteId,
    publication,
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
    downloadFromWebLabel,
  }: SoftwareInfoProps) => {
    const [selectedVersion, setSelectedVersion] = useState<string | undefined>()

    const fields = publication.productFields

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

    const licenseType = useMemo(() => getValue(fields, "license-type"), [fields])

    const price = useMemo(() => getValue(fields, "price"), [fields])

    const languages = useMemo(() => {
      const uiLanguages = getChildren(fields, "uilanguages")
      const codes = uiLanguages
        .filter(x => nameEq(x.name, "language"))
        .map(x => String(x.value).toUpperCase())
        .filter(Boolean)
      const unique = Array.from(new Set(codes))
      return unique.length ? unique.join(", ") : undefined
    }, [fields])

    useEffect(() => {
      if (!versions.length) {
        setSelectedVersion(undefined)
        return
      }
      setSelectedVersion(prev => (prev && versions.includes(prev) ? prev : versions[0]))
    }, [versions])

    return (
      <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{publisherLabel}</span>
          <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
            <AuthorImageTitle title={publication.authorTitle} authorAvatar={publication.authorAvatar} />
          </LinkFullscreen>
        </div>

        {versions.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{versionLabel}</span>
            <DropdownTertiary
              isMulti={false}
              className="w-full"
              controlled={true}
              size="medium"
              items={versionItems}
              value={selectedVersion ?? versions[0]}
              onChange={x => setSelectedVersion(x.value)}
            />
          </div>
        )}

        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{osLabel}</span>
          <span className={VALUE_CLASSNAME}>Windows / MacOS / Linux</span>
        </div>

        {licenseType && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{licenseTypeLabel}</span>
            <span className={VALUE_CLASSNAME}>{licenseType}</span>
          </div>
        )}

        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
          <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
            {formatAverageRating(publication.rating)} <SvgStarXxs className="fill-favorite" />
          </div>
        </div>

        {price && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{priceLabel}</span>
            <span className={VALUE_CLASSNAME}>{price}</span>
          </div>
        )}

        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
          <span className={VALUE_CLASSNAME}>{formatDate(publication.productUpdated)}</span>
        </div>

        {languages && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{languagesLabel}</span>
            <span className={VALUE_CLASSNAME}>{languages}</span>
          </div>
        )}

        <div className="flex flex-col gap-4">
          <ButtonPrimary className="w-full" label={downloadFromRdnLabel} />
          <ButtonPrimary className="w-full" label={downloadFromTorrentLabel} />
          <ButtonPrimary className="w-full" label={downloadFromWebLabel} />
        </div>
      </div>
    )
  },
)
