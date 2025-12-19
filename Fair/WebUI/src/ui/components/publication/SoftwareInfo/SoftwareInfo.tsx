import { SvgStarXxs } from "assets"
import { useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { ProductFieldModel, PublicationDetails } from "types"
import { ButtonPrimary, LinkFullscreen, Select } from "ui/components"
import { ensureHttp, formatAverageRating, formatDate, formatSecDate } from "utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

const getValue = <TValue = string>(fields: ProductFieldModel[] | undefined, name: ProductFieldModel["name"]) => {
  return fields?.find(x => x.name === name)?.value as TValue | undefined
}

function buildReleases(fields: ProductFieldModel[] | undefined) {
  const releases = (fields ?? []).filter(x => x.name === "release")
  return releases
    .map(r => {
      const children = r.children ?? []
      const version = getValue<string>(children, "version")

      const distributiveChildren = children.find(c => c.name === "distributive")?.children ?? []
      const platform = getValue<string>(distributiveChildren, "platform")
      const date = getValue<number>(distributiveChildren, "date")
      const distribution = getValue<string>(distributiveChildren, "distribution")
      const downloadChildren = distributiveChildren.find(c => c.name === "download")?.children ?? []
      const downloadUri = getValue<string>(downloadChildren, "uri")

      return {
        version: version ?? "",
        platform: platform ?? "",
        date,
        distribution: distribution ?? "",
        downloadUri: downloadUri ?? "",
      }
    })
    .filter(r => r.version || r.downloadUri || r.platform)
}

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
}: SoftwareInfoProps) => {
  const productFields = publication.productFields

  const releases = useMemo(() => buildReleases(productFields), [productFields])
  const versionItems = useMemo(
    () => releases.filter(r => r.version).map(r => ({ value: r.version, label: r.version })),
    [releases],
  )

  const [selectedVersion, setSelectedVersion] = useState<string | undefined>(versionItems[0]?.value)

  const selectedRelease = useMemo(() => {
    if (!selectedVersion) return releases[0]
    return releases.find(r => r.version === selectedVersion) ?? releases[0]
  }, [releases, selectedVersion])

  const licenseType = getValue<string>(productFields, "license-type")
  const licensingDetailsUrl = getValue<string>(productFields, "licensing-details-url")
  const price = getValue<unknown>(productFields, "price")

  const languages = useMemo(() => {
    const uiLanguages = (productFields ?? []).find(x => x.name === "ui-languages")
    const children = uiLanguages?.children ?? []
    return children
      .filter(x => x.name === "language")
      .map(x => (x.value as string) ?? "")
      .filter(x => !!x)
  }, [productFields])

  const platforms = useMemo(() => {
    const unique = new Set<string>()
    releases.forEach(r => {
      if (r.platform) unique.add(r.platform)
    })
    return [...unique]
  }, [releases])

  const lastUpdated =
    selectedRelease?.date != null ? formatSecDate(selectedRelease.date) : formatDate(publication.productUpdated)

  const downloads = useMemo(() => {
    return releases
      .filter(r => r.downloadUri)
      .map(r => {
        const labelBase = r.distribution ? `Download (${r.distribution})` : "Download"
        const label = r.platform ? `${labelBase} - ${r.platform}` : labelBase
        return { label, uri: ensureHttp(r.downloadUri) }
      })
  }, [releases])

  return (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{publisherLabel}</span>
        <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
          <AuthorImageTitle title={publication.authorTitle} authorAvatar={publication.authorAvatar} />
        </LinkFullscreen>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{versionLabel}</span>
        {versionItems.length > 1 ? (
          <Select
            className="h-9 rounded border border-gray-300 bg-gray-0 px-3 text-2sm text-gray-800"
            items={versionItems}
            value={selectedVersion}
            onChange={setSelectedVersion}
          />
        ) : (
          <span className={VALUE_CLASSNAME}>{selectedVersion ?? versionItems[0]?.value ?? ""}</span>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{activationLabel}</span>
        {licensingDetailsUrl ? (
          <a
            className={twMerge(VALUE_CLASSNAME, "hover:underline")}
            href={ensureHttp(licensingDetailsUrl)}
            target="_blank"
            rel="noreferrer"
          >
            {licenseType ?? ""}
          </a>
        ) : (
          <span className={VALUE_CLASSNAME}>{licenseType ?? ""}</span>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{osLabel}</span>
        <span className={VALUE_CLASSNAME}>{platforms.length > 0 ? platforms.join(" / ") : ""}</span>
      </div>

      {languages.length > 0 && (
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>Languages</span>
          <span className={VALUE_CLASSNAME}>{languages.join(", ")}</span>
        </div>
      )}

      {price != null && (
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>Price</span>
          <span className={VALUE_CLASSNAME}>{typeof price === "object" ? JSON.stringify(price) : String(price)}</span>
        </div>
      )}

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
        <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
          {formatAverageRating(publication.rating)} <SvgStarXxs className="fill-favorite" />
        </div>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
        <span className={VALUE_CLASSNAME}>{lastUpdated}</span>
      </div>

      {downloads.length > 0 && (
        <div className="flex flex-col gap-4">
          {downloads.map((d, idx) => (
            <ButtonPrimary key={idx} label={d.label} onClick={() => window.open(d.uri, "_blank")} />
          ))}
        </div>
      )}
    </div>
  )
}
