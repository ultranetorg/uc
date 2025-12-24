import { useEffect, useMemo, useState } from "react"

import { SvgStarXxs } from "assets"
import { twMerge } from "tailwind-merge"

import { ProductFieldModel, PublicationDetails } from "types"
import { ButtonPrimary, Dropdown, LinkFullscreen } from "ui/components"
import { ensureHttp, formatAverageRating, formatDate } from "utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

const getValue = <TValue = string>(fields: ProductFieldModel[] | undefined, name: ProductFieldModel["name"]) => {
  return fields?.find(x => x.name === name)?.value as TValue | undefined
}

const getChildren = (fields: ProductFieldModel[] | undefined, name: ProductFieldModel["name"]) => {
  return fields?.find(x => x.name === name)?.children ?? []
}

const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

type ReleaseDistributive = {
  platform?: string
  distribution?: string
  uri?: string
}

type ReleaseInfo = {
  version: string
  distributives: ReleaseDistributive[]
}

function parseReleases(fields: ProductFieldModel[] | undefined): ReleaseInfo[] {
  const releases = (fields ?? []).filter(x => x.name === "release")

  return releases
    .map(release => {
      const children = release.children ?? []
      const version = getValue<string>(children, "version") ?? ""

      const distributiveNodes = children.filter(x => x.name === "distributive")
      const distributives: ReleaseDistributive[] = distributiveNodes.map(node => {
        const dc = node.children ?? []
        const downloadChildren = dc.find(x => x.name === "download")?.children ?? []

        return {
          platform: getValue<string>(dc, "platform"),
          distribution: getValue<string>(dc, "distribution") ?? getValue<string>(dc, "deployment"),
          uri: getValue<string>(downloadChildren, "uri"),
        }
      })

      return version ? { version, distributives } : undefined
    })
    .filter((x): x is ReleaseInfo => !!x)
}

const uniq = (arr: string[]) => Array.from(new Set(arr.filter(Boolean)))

const compareVersionsDesc = (a: string, b: string) => {
  const pa = a.split(/[^0-9]+/g).filter(Boolean).map(Number)
  const pb = b.split(/[^0-9]+/g).filter(Boolean).map(Number)

  const len = Math.max(pa.length, pb.length)
  for (let i = 0; i < len; i++) {
    const na = pa[i] ?? 0
    const nb = pb[i] ?? 0
    if (na !== nb) return nb - na
  }

  return b.localeCompare(a)
}

function formatPrice(value: number | undefined, freeLabel: string): string {
  if (!value || value <= 0) return freeLabel
  // Backend Money is int64; depending on storage it might be cents. Heuristic: if too large, divide by 100.
  const normalized = value >= 1000 ? value / 100 : value
  return `$${normalized}`
}

export type SoftwareInfoProps = {
  siteId: string
  publication: PublicationDetails
  publisherLabel: string
  versionLabel: string
  activationLabel: string
  osLabel: string
  licenseTypeLabel: string
  priceLabel: string
  languagesLabel: string
  ratingLabel: string
  lastUpdatedLabel: string
  freeLabel: string
  paidLabel: string
  downloadFromRdnLabel: string
  downloadTorrentLabel: string
  downloadFromWebLabel: string
  downloadLabel: string
}

export const SoftwareInfo = ({
  siteId,
  publication,
  publisherLabel,
  versionLabel,
  activationLabel,
  osLabel,
  licenseTypeLabel,
  priceLabel,
  languagesLabel,
  ratingLabel,
  lastUpdatedLabel,
  freeLabel,
  paidLabel,
  downloadFromRdnLabel,
  downloadTorrentLabel,
  downloadFromWebLabel,
  downloadLabel,
}: SoftwareInfoProps) => {
  const fields = publication.productFields

  const releases = useMemo(() => parseReleases(fields), [fields])
  const releaseVersions = useMemo(() => uniq(releases.map(r => r.version)).sort(compareVersionsDesc), [releases])

  const [selectedVersion, setSelectedVersion] = useState(releaseVersions[0] ?? "")
  const selectedRelease = releases.find(r => r.version === selectedVersion) ?? releases[0]

  useEffect(() => {
    if (!releaseVersions.length) return
    if (!selectedVersion || !releaseVersions.includes(selectedVersion)) {
      setSelectedVersion(releaseVersions[0])
    }
  }, [releaseVersions, selectedVersion])

  const licenseType = getValue<string>(fields, "license-type")
  const licensingDetailsUrl = getValue<string>(fields, "licensing-details-url")

  const price = getValue<number>(fields, "price")
  const priceText = formatPrice(price, freeLabel)

  const languages = useMemo(() => {
    const uiLangNodes = (fields ?? []).filter(x => nameEq(x.name, "ui-languages"))
    const fromNodes = uiLangNodes
      .map(node => getValue<string>(node.children ?? [], "language"))
      .filter((x): x is string => !!x)

    const fromChildrenArray = getChildren(fields, "ui-languages")
      .filter(x => x.name === "language")
      .map(x => (x.value ? String(x.value) : ""))
      .filter(Boolean)

    const fromDescriptions = (fields ?? [])
      .filter(x => nameEq(x.name, "description-maximal"))
      .map(node => getValue<string>(node.children ?? [], "language"))
      .filter((x): x is string => !!x)

    return uniq([...fromNodes, ...fromChildrenArray, ...fromDescriptions])
  }, [fields])

  const platforms = useMemo(() => {
    const ps = releases
      .flatMap(r => r.distributives)
      .map(d => d.platform ?? "")
      .filter(Boolean)
    return uniq(ps)
  }, [releases])

  const activation = licenseType || (price && price > 0 ? paidLabel : freeLabel)
  const osText = platforms.length ? platforms.join(" / ") : "—"

  const downloadButtons = (selectedRelease?.distributives ?? []).filter(d => !!d.uri)

  function buildDownloadLabelI18n(d: ReleaseDistributive): string {
    const dist = (d.distribution ?? "").toLowerCase()
    if (dist.includes("rdn")) return downloadFromRdnLabel
    if (dist.includes("torrent")) return downloadTorrentLabel
    if (dist.includes("web")) return downloadFromWebLabel
    if (d.platform) return `${downloadLabel} (${d.platform})`
    return downloadLabel
  }

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
        {releaseVersions.length ? (
          <Dropdown
            controlled={true}
            isMulti={false}
            className="w-full"
            items={releaseVersions.map(v => ({ value: v, label: v }))}
            value={selectedVersion}
            onChange={(item) => setSelectedVersion(item.value)}
          />
        ) : (
          <span className={VALUE_CLASSNAME}>—</span>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{activationLabel}</span>
        <span className={VALUE_CLASSNAME}>{activation || "—"}</span>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{osLabel}</span>
        <span className={VALUE_CLASSNAME}>{osText}</span>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{licenseTypeLabel}</span>
        {licenseType ? (
          licensingDetailsUrl ? (
            <a
              href={ensureHttp(licensingDetailsUrl)}
              target="_blank"
              rel="noreferrer"
              className={twMerge(VALUE_CLASSNAME, "text-primary hover:underline")}
              title={licensingDetailsUrl}
            >
              {licenseType}
            </a>
          ) : (
            <span className={VALUE_CLASSNAME}>{licenseType}</span>
          )
        ) : (
          <span className={VALUE_CLASSNAME}>—</span>
        )}
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{priceLabel}</span>
        <span className={VALUE_CLASSNAME}>{priceText}</span>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{languagesLabel}</span>
        <span className={VALUE_CLASSNAME}>{languages.length ? languages.join(", ") : "—"}</span>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
        <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
          {formatAverageRating(publication.rating)} <SvgStarXxs className="fill-favorite" />
        </div>
      </div>

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
        <span className={VALUE_CLASSNAME}>{formatDate(publication.productUpdated)}</span>
      </div>

      {downloadButtons.length ? (
        <div className="flex flex-col gap-4">
          {downloadButtons.map((d, i) => (
            <ButtonPrimary
              key={`${d.uri}-${i}`}
              label={buildDownloadLabelI18n(d)}
              onClick={() => {
                if (!d.uri) return
                window.open(ensureHttp(d.uri), "_blank", "noopener,noreferrer")
              }}
            />
          ))}
        </div>
      ) : null}
    </div>
  )
}
