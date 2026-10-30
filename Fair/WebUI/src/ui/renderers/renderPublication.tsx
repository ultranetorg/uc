import { TFunction } from "i18next"

import { PublicationImageBase } from "types"
import { PublicationInfo } from "ui/components"

export const renderPublication = (publication: PublicationImageBase) => (
  <PublicationInfo avatarId={publication.imageId} categoryTitle={publication.categoryTitle} title={publication.title} />
)

export const renderRemovedPublication = (t: TFunction) => (
  <div className="flex items-center gap-2" title={t("common:publicationRemovedHint")}>
    <div className="size-8 shrink-0 rounded-lg border border-dashed border-gray-300 bg-gray-100" />
    <div className="flex flex-col overflow-hidden">
      <span className="truncate text-sm leading-4.25 text-gray-500">{t("common:publicationRemoved")}</span>
      <span className="truncate text-xs leading-3.75 text-gray-400">{t("common:publicationRemovedHint")}</span>
    </div>
  </div>
)

export const renderPublication2 = (title?: string, logoId?: string) => (
  <PublicationInfo avatarId={logoId} title={title} />
)
