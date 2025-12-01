import { memo } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { PublicationDetails } from "types"

import { Description, SiteLink, Slider, SoftwareInfo, SystemRequirementsTabs } from "ui/components/publication"

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

export interface PublicationSoftwarePageProps {
  publication: PublicationDetails
}

export const PublicationSoftwarePage = memo(({ publication }: PublicationSoftwarePageProps) => {
  const { t } = useTranslation("publication")
  const { siteId } = useParams()

  return (
    <>
      <div className="flex flex-1 flex-col gap-8">
        <Slider />
        <Description text={publication.description} showMoreLabel={t("showMore")} descriptionLabel={t("information")} />
        <SystemRequirementsTabs label={t("systemRequirements")} tabs={TEST_TAB_ITEMS} />
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
        <SiteLink to={"google.com"} label={t("officialSite")} />
      </div>
    </>
  )
})
