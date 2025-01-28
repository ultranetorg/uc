export type PublicationCardProps = {
  publicationName: string
  authorTitle: string
}

export const PublicationCard = ({ publicationName, authorTitle }: PublicationCardProps) => {
  return (
    <div className="flex flex-col border p-4">
      <div className="h-28 w-28 border bg-purple-300" />
      <span>{publicationName}</span>
      <span>{authorTitle}</span>
    </div>
  )
}
