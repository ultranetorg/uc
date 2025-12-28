import { memo, useMemo } from "react"

import { ProductFieldModel } from "types"
import { TagsList } from "ui/components"
import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTabs } from "ui/components/publication"
import { getValue, nameEq } from "ui/components/publication/utils"
import { ReviewsList } from "ui/components/specific"
import { buildFileUrl, ensureHttp } from "utils"

import { ContentProps } from "../types"

function buildDescriptions(fields: ProductFieldModel[] | undefined): { language: string; text: string }[] {
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

const TEST_TAB_ITEMS = [
  {
    key: "windows",
    label: "Windows",
    sections: [
      {
        key: "Minimum",
        name: "Minimum",
        values: {
          CPU: "AMD Ryzen 5 1600 or Intel Core i5 6600K",
          RAM: "8 GB",
          "Video Card": "AMD Radeon RX 570 or Nvidia GeForce GTX 1050 Ti",
          "Dedicated Video RAM": "4096 MB",
          OS: "Windows 10 / 11 - 64-Bit (Latest Update)",
          "Free Disk Space": "100 GB",
        },
      },
      {
        key: "recommended",
        name: "Recommended",
        values: {
          CPU: "AMD Ryzen 7 2700X or Intel Core i7 6700",
          RAM: "12 GB",
          "Video Card": "AMD Radeon RX 570 or Nvidia GeForce GTX 1050 Ti",
          "Dedicated Video RAM": "4096 MB",
          OS: "Windows 10 / 11 - 64-Bit (Latest Update)",
          "Free Disk Space": "100 GB",
        },
      },
    ],
  },
  { key: "linux", label: "Linux", sections: [] },
  { key: "macos", label: "macOS", sections: [] },
]

export const SoftwarePublicationContent = memo(
  ({ t, siteId, publication, isPending, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const mediaItems = useMemo(() => buildMediaItems(publication.productFields), [publication.productFields])
    const descriptions = useMemo(() => buildDescriptions(publication.productFields), [publication.productFields])

    const hasDescription = !!publication.description || descriptions.length > 0

    const officialSite = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "uri")
      if (!raw) return undefined
      const normalized = String(raw).trim()
      return normalized ? ensureHttp(normalized) : undefined
    }, [publication.productFields])

    const eulaUrl = useMemo(() => {
      const raw =
        getValue<string>(publication.productFields, "licensing-details-url") ??
        getValue<string>(publication.productFields, "eula")
      if (!raw) return undefined
      const normalized = String(raw).trim()
      return normalized ? ensureHttp(normalized) : undefined
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
          <SystemRequirementsTabs label={t("systemRequirements")} tabs={TEST_TAB_ITEMS} />
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
          {eulaUrl && <SiteLink to={eulaUrl} label={t("eula")} />}
          {tags && <TagsList tags={tags} />}
        </div>
      </>
    )
  },
)
