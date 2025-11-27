import { useTranslation } from "react-i18next"
import { Link, useParams } from "react-router-dom"
import { AccountBaseAvatar } from "types"
import { SvgBoxArrowUpRight } from "assets"
import { ensureHttp } from "utils"
import { Card } from "ui/components/Card"
import { AuthorImageTitle } from "ui/components/publication/SoftwareInfo/components"
import { LinkFullscreen } from "../../../components"
import { PublicationMetaItemView, PublicationMetaItemViewSimple } from "./PublicationMetaItemView"

export type PublicationViewBSideProps = {
  owner?: AccountBaseAvatar
  version: string
  license: string
  uri: string
  price?: number
}

const SiteLink = ({ to, label }: { to: string; label: string }) => (
  <Link
    to={ensureHttp(to)}
    target="_blank"
    className="flex items-center justify-between border border-gray-300 px-4 py-2 text-2sm font-medium"
    title={to.toString()}
  >
    {label}
    <SvgBoxArrowUpRight className="fill-gray-800" />
  </Link>
)

export const PublicationViewMeta = ({ owner, version, price, license, uri }: PublicationViewBSideProps) => {
  const { t } = useTranslation("productPreview")
  const { siteId } = useParams()

  return (
    <Card>
      <div className="flex flex-col gap-6">
        {owner && (
          <PublicationMetaItemView label={t("publisher")}>
            <LinkFullscreen to={`/${siteId}/a/${owner.id}`}>
              <AuthorImageTitle title={owner.nickname ?? ""} authorAvatar={owner.avatar} />
            </LinkFullscreen>
          </PublicationMetaItemView>
        )}
        <PublicationMetaItemViewSimple label={t("version")} value={version} />
        <PublicationMetaItemViewSimple label={t("license")} value={license} />
        {price && <PublicationMetaItemViewSimple label={t("price")} value={price} />}
        {uri && <SiteLink to={uri} label={t("viewSite")} />}
      </div>
    </Card>
  )
}
