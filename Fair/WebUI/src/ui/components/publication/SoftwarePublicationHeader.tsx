import { ModeratorOptionsMenu, SoftwarePublicationLogo } from "ui/components/specific"

export type SoftwarePublicationHeaderProps = {
  id: string
  title: string
  logoFileId?: string
  categories: string[]
}

export const SoftwarePublicationHeader = ({ id, title, logoFileId, categories }: SoftwarePublicationHeaderProps) => (
  <div className="flex items-center justify-between">
    <SoftwarePublicationLogo logoFileId={logoFileId} title={title} categories={categories} />
    <ModeratorOptionsMenu className="ml-auto" publicationId={id} />
  </div>
)
