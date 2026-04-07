import { ReactNode } from "react"

import { ModeratorOptionsMenu, SoftwarePublicationLogo } from "ui/components/specific"

export type SoftwarePublicationHeaderProps = {
  id?: string
  title?: string
  logoFileId?: string
  categories?: string[]
  components?: ReactNode
}

export const SoftwarePublicationHeader = ({
  id,
  title,
  logoFileId,
  categories,
  components,
}: SoftwarePublicationHeaderProps) => (
  <div className="flex items-center justify-between">
    <SoftwarePublicationLogo logoFileId={logoFileId} title={title} categories={categories} />
    {id && <ModeratorOptionsMenu className="ml-auto" publicationId={id} />}
    {components}
  </div>
)
