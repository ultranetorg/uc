import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { Accordion, Card } from "ui/components"
import { ensureHttp } from "utils"
import { SvgBoxArrowUpRight } from "assets"
import { IPublicationRelease } from "../types"
import { PublicationMetaItemView, PublicationMetaItemViewSimple } from "./PublicationMetaItemView"

interface PublicationViewReleaseProps {
  data: IPublicationRelease[]
}

const SectionView = ({ label, children }: { label: string; children: React.ReactNode }) => (
  <div className="flex flex-col">
    <span className="mb-4 font-semibold leading-6 underline">{label}</span>
    <div className="flex flex-col gap-6">{children}</div>
  </div>
)

const DistributiveView = (distributive: IPublicationRelease["distributive"]) => {
  const { t } = useTranslation("productPreview", { keyPrefix: "distributive" })
  return (
    <SectionView label={t("title")}>
      <PublicationMetaItemViewSimple label={t("version")} value={distributive.version} />
      <PublicationMetaItemViewSimple label={t("platform")} value={distributive.platform} />
      <PublicationMetaItemViewSimple label={t("date")} value={distributive.date} />
      <PublicationMetaItemViewSimple label={t("deployment")} value={distributive.deployment} />
      <PublicationMetaItemView label={t("download")}>
        <Link
          to={ensureHttp(distributive.download.uri)}
          target="_blank"
          className="flex items-center justify-between gap-1 hover:bg-white"
          title={distributive.download.uri}
        >
          {distributive.download.uri}
          <SvgBoxArrowUpRight className="fill-gray-800" />
        </Link>
      </PublicationMetaItemView>
    </SectionView>
  )
}

const RequirementsView = (requirements: IPublicationRelease["requirements"]) => {
  const { t } = useTranslation("productPreview", { keyPrefix: "requirements" })
  return (
    <SectionView label={t("title")}>
      <PublicationMetaItemViewSimple label={t("hardware.cpu")} value={requirements.hardware.cpu} />
      <PublicationMetaItemViewSimple label={t("hardware.gpu")} value={requirements.hardware.gpu} />
      <PublicationMetaItemViewSimple label={t("hardware.npu")} value={requirements.hardware.npu} />
      <PublicationMetaItemViewSimple label={t("hardware.ram")} value={requirements.hardware.ram} />
      <PublicationMetaItemViewSimple label={t("hardware.hdd")} value={requirements.hardware.hdd} />
      <PublicationMetaItemViewSimple label={t("software.os")} value={requirements.software.os} />
      <PublicationMetaItemViewSimple label={t("software.architecture")} value={requirements.software.architecture} />
      <PublicationMetaItemViewSimple label={t("software.version")} value={requirements.software.version} />
    </SectionView>
  )
}

export const PublicationViewRelease = ({ data }: PublicationViewReleaseProps) => {
  const { t } = useTranslation("productPreview")

  const accordionItems = data.map(({ version, distributive, requirements }) => ({
    title: "#" + version,
    content: (
      <div className="flex flex-col gap-6">
        <DistributiveView {...distributive} />
        <RequirementsView {...requirements} />
      </div>
    ),
  }))

  return (
    <Card label={t("releases")}>
      <Accordion items={accordionItems} />
    </Card>
  )
}
