import { memo, useCallback, useEffect, useMemo, useState } from "react"

import { sortBy } from "lodash"
import { TagsList, TextModal } from "ui/components"
import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTabs } from "ui/components/publication"
import { ReviewsList } from "ui/components/specific"

import { ContentProps } from "../types"

import {
  buildMediaItems,
  buildSystemRequirements,
  getReleases,
  getValueFrom,
  getTags,
  getUiLanguages,
  getUrlFrom,
  getDescriptions,
  getAllSupportedPlatforms,
  getSoftwareDownloads,
} from "./utils"

const platformOrder = ["windows", "macos", "linux"]

export const GameSoftwarePublicationContent = memo(
  ({ t, siteId, productOrPublication, isPendingReviews, reviews, error, onLeaveReview }: ContentProps) => {
    const [isEulaOpen, setIsEulaOpen] = useState(false)
    const [platform, setPlatform] = useState<string | undefined>()
    const [version, setVersion] = useState<string | undefined>()

    const releases = useMemo(
      () => getReleases(productOrPublication.productFields!),
      [productOrPublication.productFields],
    )

    const releasesByVersion = useMemo(() => {
      if (!releases) return undefined
      return sortBy(
        releases.filter(x => x.version === version),
        x => {
          const i = platformOrder.indexOf(x.requirements.platform.platform)
          return i === -1 ? Infinity : i
        },
      )
    }, [releases, version])

    const descriptions = useMemo(
      () => getDescriptions(productOrPublication.productFields!),
      [productOrPublication.productFields],
    )

    const eulaText = useMemo(
      () => getValueFrom(productOrPublication.productFields!, "eula"),
      [productOrPublication.productFields],
    )

    const license = useMemo(
      () => getValueFrom(productOrPublication.productFields!, "license-type"),
      [productOrPublication.productFields],
    )

    const licensingDetailsUrl = useMemo(
      () => getUrlFrom(productOrPublication.productFields!, "licensing-details-url"),
      [productOrPublication.productFields],
    )

    const mediaItems = useMemo(
      () => buildMediaItems(productOrPublication.productFields),
      [productOrPublication.productFields],
    )

    const officialSite = useMemo(
      () => getUrlFrom(productOrPublication.productFields!, "uri"),
      [productOrPublication.productFields],
    )

    const price = useMemo(
      () => getValueFrom(productOrPublication.productFields!, "price"),
      [productOrPublication.productFields],
    )

    const softwareDownloads = useMemo(
      () =>
        releasesByVersion !== undefined && platform !== undefined
          ? getSoftwareDownloads(releasesByVersion, platform)
          : undefined,
      [platform, releasesByVersion],
    )

    const supportedPlatforms = useMemo(() => {
      if (releases === undefined) return undefined
      const platforms = getAllSupportedPlatforms(releases)
      if (!platforms) return undefined
      return platforms.map(x => t(`platforms:${x}`))
    }, [releases, t])

    const systemRequirements = useMemo(() => {
      return releasesByVersion !== undefined ? buildSystemRequirements(t, releasesByVersion) : undefined
    }, [releasesByVersion, t])

    const tags = useMemo(() => getTags(productOrPublication.productFields!), [productOrPublication.productFields])

    const uiLanguages = useMemo(
      () => getUiLanguages(productOrPublication.productFields!),
      [productOrPublication.productFields],
    )

    const handleVersionChange = useCallback((version?: string) => setVersion(version), [])
    const handlePlatformChange = useCallback((platform: string) => setPlatform(platform), [])

    useEffect(() => {
      if (systemRequirements && systemRequirements.length > 0) setPlatform(systemRequirements[0].key)
    }, [systemRequirements])

    return (
      <>
        <div className="flex flex-1 flex-col gap-8">
          {mediaItems.length > 0 && <Slider items={mediaItems} />}
          {descriptions && (
            <Description
              text={productOrPublication.description}
              descriptions={descriptions}
              showMoreLabel={t("showMore")}
              showLessLabel={t("showLess")}
              descriptionLabel={t("information")}
            />
          )}
          {systemRequirements && systemRequirements.length > 0 && (
            <SystemRequirementsTabs
              label={t("systemRequirements")}
              tabs={systemRequirements}
              activeTab={platform}
              onTabChange={handlePlatformChange}
            />
          )}
          {reviews && onLeaveReview && (
            <ReviewsList
              isPending={isPendingReviews!}
              reviews={reviews}
              error={error}
              onLeaveReviewClick={onLeaveReview}
              leaveReviewLabel={t("leaveReview")}
              noReviewsLabel={t("noReviews")}
              reviewLabel={t("review", { count: reviews?.totalItems })}
              showMoreReviewsLabel={t("showMoreReviews")}
            />
          )}
        </div>
        <div className="flex w-87.5 flex-col gap-8">
          <SoftwareInfo
            siteId={siteId!}
            productOrPublication={productOrPublication}
            supportedPlatforms={supportedPlatforms}
            price={price}
            languages={uiLanguages}
            licenseType={license}
            licensingDetailsUrl={licensingDetailsUrl}
            softwareDownloads={softwareDownloads}
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
            downloadFromIpfsLabel={t("downloadFromIpfs")}
            onVersionChange={handleVersionChange}
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
          {isEulaOpen && (
            <TextModal
              title={t("eula")}
              text={eulaText!}
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
