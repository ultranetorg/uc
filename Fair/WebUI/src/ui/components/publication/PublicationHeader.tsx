import { memo, ReactNode } from "react"

import { PublicationDetailsThreeDotsMenu, SoftwarePublicationLogo } from "ui/components/specific"

export type PublicationHeaderProps = {
  id?: string
  title?: string
  logoFileId?: string
  categories?: string[]
  components?: ReactNode
}

export const PublicationHeader = memo(({ id, title, logoFileId, categories, components }: PublicationHeaderProps) => (
  <div className="flex items-center justify-between">
    <SoftwarePublicationLogo logoFileId={logoFileId} title={title} categories={categories} />
    {id && <PublicationDetailsThreeDotsMenu className="ml-auto" publicationId={id} publicationTitle={title} />}
    {components}
  </div>
))
