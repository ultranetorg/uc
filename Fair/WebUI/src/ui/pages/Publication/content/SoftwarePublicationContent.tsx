import { memo, useMemo, useState } from "react"
import { ProductFieldModel } from "types"

import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTab, SystemRequirementsTabs } from "ui/components/publication"
import { ReviewsList } from "ui/components/specific"

import { SvgBoxArrowUpRight } from "assets"
import { Modal } from "ui/components"

import { buildFileUrl, ensureHttp } from "utils"

import { ContentProps } from "../types"

const getValue = <TValue = string>(fields: ProductFieldModel[] | undefined, name: ProductFieldModel["name"]) => {
  return fields?.find(x => nameEq(x.name, String(name)))?.value as TValue | undefined
}

const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

const getChildren = (fields: ProductFieldModel[] | undefined, name: ProductFieldModel["name"]) => {
  return fields?.find(x => nameEq(x.name, String(name)))?.children ?? []
}

function buildMediaItems(fields: ProductFieldModel[] | undefined) {
  const arts = (fields ?? []).filter(x => nameEq(x.name, "art"))
  const items: { src: string; poster?: string; alt?: string }[] = []

  for (const art of arts) {
    const children = art.children ?? []

    const screenshotChildren = children.find(c => nameEq(c.name, "screenshot"))?.children ?? []
    const videoChildren = children.find(c => nameEq(c.name, "video"))?.children ?? []

    const screenshotId = screenshotChildren.find(c => nameEq(c.name, "id"))?.value as string | undefined
    const screenshotUrl = screenshotId ? buildFileUrl(screenshotId) : undefined

    const videoUrlRaw = (videoChildren.find(c => nameEq(c.name, "uri"))?.value as string | undefined) ?? undefined
    const videoUrl = videoUrlRaw ? ensureHttp(String(videoUrlRaw)) : undefined

    if (videoUrl) {
      items.push({ src: videoUrl, poster: screenshotUrl })
    }

    if (screenshotUrl) {
      items.push({ src: screenshotUrl })
    }
  }

  const unique = new Map<string, { src: string; poster?: string; alt?: string }>()
  for (const it of items) {
    if (!unique.has(it.src)) unique.set(it.src, it)
  }
  return Array.from(unique.values())
}

type RequirementsSection = { hardware: Record<string, string>; software: Record<string, string> }

function buildDescriptions(fields: ProductFieldModel[] | undefined): { language: string; text: string }[] {
  const descriptionMaximal = (fields ?? []).filter(x => nameEq(x.name, "description-maximal"))
  
  const descriptions: { language: string; text: string }[] = []
  
  for (const desc of descriptionMaximal) {
    const children = desc.children ?? []
    const language = getValue<string>(children, "language") ?? ""
    const text = getValue<string>(children, "value") ?? ""
    
    if (language && text) {
      descriptions.push({ language, text })
    }
  }
  
  return descriptions
}

function buildRequirementsTabs(fields: ProductFieldModel[] | undefined): SystemRequirementsTab[] {
  const releases = (fields ?? []).filter(x => nameEq(x.name, "release"))

  type PlatformEntry = { key: string; label: string; minimal?: RequirementsSection; recommended?: RequirementsSection }

  const byPlatform = new Map<string, PlatformEntry>()

  const normalizePlatform = (raw: string) => {
    const s = raw.trim()
    const k = s.toLowerCase()

    if (k.includes("win")) return { key: "windows", label: "Windows" }
    if (k.includes("mac") || k.includes("osx")) return { key: "macos", label: "MacOS" }
    if (k.includes("linux")) return { key: "linux", label: "Linux" }

    return { key: k || "unknown", label: s || "Unknown" }
  }

  const ensurePlatform = (raw: string) => {
    const { key, label } = normalizePlatform(raw)
    const existing = byPlatform.get(key)
    if (existing) return existing
    const created: PlatformEntry = { key, label }
    byPlatform.set(key, created)
    return created
  }

  const extractSection = (nodeChildren: ProductFieldModel[] | undefined): RequirementsSection => {
    const hardwareChildren = getChildren(nodeChildren, "hardware")
    const softwareChildren = getChildren(nodeChildren, "software")

    const hardware: Record<string, string> = {
      CPU: getValue<string>(hardwareChildren, "cpu") ?? "",
      GPU: getValue<string>(hardwareChildren, "gpu") ?? "",
      NPU: getValue<string>(hardwareChildren, "npu") ?? "",
      RAM: getValue<string>(hardwareChildren, "ram") ?? "",
      HDD: getValue<string>(hardwareChildren, "hdd") ?? "",
    }

    const software: Record<string, string> = {
      OS: getValue<string>(softwareChildren, "os") ?? "",
      Architecture: getValue<string>(softwareChildren, "architecture") ?? "",
      Version: getValue<string>(softwareChildren, "version") ?? "",
    }

    const filteredHardware: Record<string, string> = {}
    Object.entries(hardware).forEach(([k, v]) => {
      if (v) filteredHardware[k] = v
    })

    const filteredSoftware: Record<string, string> = {}
    Object.entries(software).forEach(([k, v]) => {
      if (v) filteredSoftware[k] = v
    })

    return { hardware: filteredHardware, software: filteredSoftware }
  }

  for (const release of releases) {
    const releaseChildren = release.children ?? []
    const requirementsRoot = releaseChildren.find(x => nameEq(x.name, "requirements"))?.children ?? []

    // Preferred (matches backend meta): requirements -> platform[] -> minimal/recommended
    const platformNodes = requirementsRoot.filter(x => nameEq(x.name, "platform"))

    if (platformNodes.length) {
      for (const p of platformNodes) {
        const platform = p.value ? String(p.value) : ""
        if (!platform) continue

        const platformEntry = ensurePlatform(platform)
        const pChildren = p.children ?? []

        const minimalNode = pChildren.find(x => nameEq(x.name, "minimal"))
        const recommendedNode = pChildren.find(x => nameEq(x.name, "recommended"))

        if (minimalNode?.children?.length) {
          platformEntry.minimal = extractSection(minimalNode.children)
        }

        if (recommendedNode?.children?.length) {
          platformEntry.recommended = extractSection(recommendedNode.children)
        }
      }

      continue
    }

    // Fallback (older parsing used in PublicationView/utils): requirements -> hardware/software
    const platform = getValue<string>(releaseChildren.find(x => nameEq(x.name, "distributive"))?.children, "platform") ?? ""

    if (platform) {
      const platformEntry = ensurePlatform(platform)
      platformEntry.minimal = extractSection(requirementsRoot)
    }
  }

  const tabs: SystemRequirementsTab[] = []

  for (const p of byPlatform.values()) {
    const sections: SystemRequirementsTab["sections"] = []

    if (p.minimal && (Object.keys(p.minimal.hardware).length || Object.keys(p.minimal.software).length)) {
      sections.push({
        key: "minimum",
        name: "Minimum",
        values: { ...p.minimal.hardware, ...p.minimal.software },
      })
    }

    if (p.recommended && (Object.keys(p.recommended.hardware).length || Object.keys(p.recommended.software).length)) {
      sections.push({
        key: "recommended",
        name: "Recommended",
        values: { ...p.recommended.hardware, ...p.recommended.software },
      })
    }

    tabs.push({ key: p.key, label: p.label, sections })
  }

  const order = ["windows", "macos", "linux"]
  tabs.sort((a, b) => {
    const ai = order.indexOf(a.key)
    const bi = order.indexOf(b.key)
    if (ai !== -1 || bi !== -1) return (ai === -1 ? 999 : ai) - (bi === -1 ? 999 : bi)
    return a.label.localeCompare(b.label)
  })

  return tabs
}

export const SoftwarePublicationContent = memo(
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const mediaItems = buildMediaItems(publication.productFields)
    const descriptions = buildDescriptions(publication.productFields)
    const tabs = buildRequirementsTabs(publication.productFields)
    const uri = getValue<string>(publication.productFields, "uri")
    const tagsRaw = getValue<string>(publication.productFields, "tags")

    const eulaText = getValue<string>(publication.productFields, "eula")
    const [isEulaOpen, setEulaOpen] = useState(false)

    const eulaButtonLabel = useMemo(() => t("eula"), [t])

    const tags = useMemo(() => {
      if (!tagsRaw) return []
      return tagsRaw
        .split(/[,;]+/g)
        .map(x => x.trim())
        .filter(Boolean)
    }, [tagsRaw])

    return (
      <>
        <div className="flex flex-1 flex-col gap-8">
          <Slider items={mediaItems} />
          <Description
            text={publication.description}
            descriptions={descriptions.length ? descriptions : undefined}
            showMoreLabel={t("showMore")}
            descriptionLabel={t("information")}
          />
          <SystemRequirementsTabs label={t("systemRequirements")} tabs={tabs} />
          <ReviewsList
            isPending={isPending || isPendingReviews}
            reviews={reviews}
            error={error}
            onLeaveReviewClick={onLeaveReview}
            leaveReviewLabel={t("leaveReview")}
            noReviewsLabel={t("noReviews")}
            reviewLabel={t("review", { count: reviews?.totalItems })}
            showMoreReviewsLabel={t("showMoreReviews")}
          />
        </div>
        <div className="flex w-87.5 flex-col gap-8">
          <SoftwareInfo
            siteId={siteId!}
            publication={publication}
            publisherLabel={t("publisher")}
            versionLabel={t("version")}
            activationLabel={t("activation")}
            osLabel={t("os")}
            licenseTypeLabel={t("licenseType")}
            priceLabel={t("price")}
            languagesLabel={t("languages")}
            ratingLabel={t("rating")}
            lastUpdatedLabel={t("lastUpdated")}
            freeLabel={t("free")}
            paidLabel={t("paid")}
            downloadFromRdnLabel={t("downloadFromRdn")}
            downloadTorrentLabel={t("downloadTorrent")}
            downloadFromWebLabel={t("downloadFromWeb")}
            downloadLabel={t("download")}
          />
          {uri ? <SiteLink to={ensureHttp(uri)} label={t("officialSite")} /> : null}
          {eulaText ? (
            <button
              type="button"
              onClick={() => setEulaOpen(true)}
              className="flex items-center justify-between rounded-lg border border-gray-300 bg-gray-100 px-6 py-4 text-2sm font-medium leading-4.5"
              title={eulaButtonLabel}
            >
              {eulaButtonLabel}
              <SvgBoxArrowUpRight className="fill-gray-800" />
            </button>
          ) : null}

          {tags.length ? (
            <div className="flex flex-wrap gap-2">
              {tags.map(tag => (
                <span key={tag} className="rounded-full border border-gray-300 bg-gray-100 px-3 py-1 text-2xs font-medium text-gray-800">
                  {tag}
                </span>
              ))}
            </div>
          ) : null}
        </div>
        {isEulaOpen && eulaText ? (
          <Modal className="w-135 max-w-[90vw] gap-4 p-6" title={eulaButtonLabel} onClose={() => setEulaOpen(false)}>
            <div className="max-h-[70vh] overflow-auto whitespace-pre-wrap text-sm text-gray-800">{eulaText}</div>
          </Modal>
        ) : null}
      </>
    )
  },
)
