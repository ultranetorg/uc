import { memo, ReactNode } from "react"

import { ModeratorOptionsMenu, SoftwarePublicationLogo } from "ui/components/specific"

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
    {id && <ModeratorOptionsMenu className="ml-auto" publicationId={id} />}
    {components}
  </div>
))
