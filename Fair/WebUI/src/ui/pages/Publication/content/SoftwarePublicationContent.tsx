import { memo, useMemo } from "react"

import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTabs } from "ui/components/publication"
import { ReviewsList } from "ui/components/specific"

import { ProductFieldModel } from "types"

import { buildFileUrl, ensureHttp } from "utils"

import { ContentProps } from "../types"

const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

const getValue = <TValue = string>(fields: ProductFieldModel[] | undefined, name: string) => {
  return fields?.find(x => nameEq(x.name, name))?.value as TValue | undefined
}

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

    const officialSite = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "uri")
      return raw ? ensureHttp(String(raw)) : "google.com"
    }, [publication.productFields])

    const tags = useMemo(() => {
      const raw = getValue<string>(publication.productFields, "tags")
      if (!raw) return [] as string[]
      return String(raw)
        .split(/[,;\s]+/g)
        .map(x => x.trim())
        .filter(Boolean)
    }, [publication.productFields])

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
          activationLabel={t("activation")}
          osLabel={t("os")}
          ratingLabel={t("rating")}
          lastUpdatedLabel={t("lastUpdated")}
        />
        <SiteLink to={officialSite} label={t("officialSite")} />
        {tags.length ? (
          <div className="flex flex-wrap gap-2">
            {tags.map(tag => (
              <span key={tag} className="rounded border border-[#D7DDEB] bg-[#F3F5F8] px-2 py-1 text-2xs leading-4 text-gray-800">
                {tag}
              </span>
            ))}
          </div>
        ) : null}
      </div>
    </>
    )
  },
)
