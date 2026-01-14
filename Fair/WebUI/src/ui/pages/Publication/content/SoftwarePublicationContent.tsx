import { memo, useMemo, useState } from "react"

import { ProductField } from "types"
import { TagsList, TextModal } from "ui/components"
import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTabs } from "ui/components/publication"
import {
  RequirementPlatform,
  getChildren,
  getRequirementPlatforms,
  getValue,
  nameEq,
} from "ui/components/publication/utils"
import { ReviewsList } from "ui/components/specific"
import { buildFileUrl, ensureHttp } from "utils"

import { ContentProps } from "../types"

function buildDescriptions(fields: ProductField[] | undefined): { language: string; text: string }[] {
  const descriptionMaximal = (fields ?? []).filter(x => nameEq(x.name, "description-maximal"))

  const descriptions: { language: string; text: string }[] = []

  for (const desc of descriptionMaximal) {
    const children = desc.children ?? []
    const language = (getValue<string>(children, "language") ?? "").trim()
    const text = (getValue<string>(children, "value") ?? "").trim()

    if (language && text) {
      descriptions.push({ language, text })
    }
  }

  return descriptions
}

function buildMediaItems(fields: ProductField[] | undefined) {
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

type SystemRequirementsTabSectionLike = {
  key: string
  name: string
  values: Record<string, string>
}

type SystemRequirementsTabLike = {
  key: string
  label: string
  sections: SystemRequirementsTabSectionLike[]
}

function buildSection(
  platform: RequirementPlatform,
  sectionKey: string,
  sectionName: string,
  children: ProductField[] | undefined,
): SystemRequirementsTabSectionLike | null {
  if (!children?.length) return null

  const hardware = getChildren(children, "hardware")
  const software = getChildren(children, "software")

  const values: Record<string, string> = {}

  const cpu = getValue(hardware, "cpu")
  if (cpu) values.CPU = String(cpu)

  const ram = getValue(hardware, "ram")
  if (ram) values.RAM = String(ram)

  const gpu = getValue(hardware, "gpu")
  if (gpu) values.GPU = String(gpu)

  const hdd = getValue(hardware, "hdd")
  if (hdd) values.HDD = String(hdd)

  const os = getValue(software, "os")
  if (os) values.OS = String(os)

  const architecture = getValue(software, "architecture")
  if (architecture) values.Architecture = String(architecture)

  if (!Object.keys(values).length) return null

  return {
    key: `${platform.key}-${sectionKey}`,
    name: sectionName,
    values,
  }
}

function buildSystemRequirementsTabs(fields: ProductField[] | undefined): SystemRequirementsTabLike[] {
  const platforms = getRequirementPlatforms(fields)
  const tabs: SystemRequirementsTabLike[] = []

  for (const platform of platforms) {
    const platformChildren = platform.node.children ?? []

    const minimalChildren = getChildren(platformChildren, "minimal")
    const recommendedChildren = getChildren(platformChildren, "recommended")

    const sections: SystemRequirementsTabSectionLike[] = []

    const minimalSection = buildSection(platform, "minimum", "Minimum", minimalChildren)
    if (minimalSection) sections.push(minimalSection)

    const recommendedSection = buildSection(platform, "recommended", "Recommended", recommendedChildren)
    if (recommendedSection) sections.push(recommendedSection)

    if (sections.length) {
      tabs.push({
        key: platform.key,
        label: platform.label,
        sections,
      })
    }
  }

  return tabs
}

export const SoftwarePublicationContent = memo(
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const [isEulaOpen, setIsEulaOpen] = useState(false)
    const mediaItems = useMemo(() => buildMediaItems(publication.productFields), [publication.productFields])
    const descriptions = useMemo(() => buildDescriptions(publication.productFields), [publication.productFields])
    const systemRequirementsTabs = useMemo(
      () => buildSystemRequirementsTabs(publication.productFields),
      [publication.productFields],
    )

    const hasDescription = !!publication.description || descriptions.length > 0

    const officialSite = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "uri")
      if (!raw) return undefined
      const normalized = String(raw).trim()
      return normalized ? ensureHttp(normalized) : undefined
    }, [publication.productFields])

    const eulaText = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "eula")
      if (!raw) return undefined
      const normalized = String(raw).trim()
      return normalized || undefined
    }, [publication.productFields])

    const tags = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "tags")
      if (!raw) return undefined
      const parsed = String(raw)
        .split(/[,;\s]+/g)
        .map(x => x.trim())
        .filter(Boolean)
      return parsed.length ? parsed : undefined
    }, [publication.productFields])

    return (
      <>
        <div className="flex flex-1 flex-col gap-8">
          {mediaItems.length > 0 && <Slider items={mediaItems} />}
          {hasDescription && (
            <Description
              text={publication.description}
              descriptions={descriptions.length ? descriptions : undefined}
              showMoreLabel={t("showMore")}
              showLessLabel={t("showLess")}
              descriptionLabel={t("information")}
            />
          )}
          {systemRequirementsTabs.length > 0 && (
            <SystemRequirementsTabs label={t("systemRequirements")} tabs={systemRequirementsTabs} />
          )}
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
            osLabel={t("os")}
            ratingLabel={t("rating")}
            lastUpdatedLabel={t("lastUpdated")}
            licenseTypeLabel={t("licenseType")}
            priceLabel={t("price")}
            languagesLabel={t("languages")}
            downloadFromRdnLabel={t("downloadFromRdn")}
            downloadFromTorrentLabel={t("downloadFromTorrent")}
            downloadFromWebLabel={t("downloadFromWeb")}
          />
          {officialSite && <SiteLink to={officialSite} label={t("officialSite")} />}
          {eulaText && (
            <button
              type="button"
              className="flex items-center justify-between rounded-lg border border-[#D7DDEB] bg-[#F3F5F8] px-6 py-4 text-left text-2sm font-medium leading-4.5 text-gray-800"
              onClick={() => setIsEulaOpen(true)}
            >
              {t("eula")}
            </button>
          )}
          {isEulaOpen && eulaText && (
            <TextModal
              title={t("eula")}
              text={eulaText}
              confirmLabel={t("common:ok")}
              onConfirm={() => setIsEulaOpen(false)}
            />
          )}
          {tags && <TagsList tags={tags} />}
        </div>
      </>
    )
  },
)
