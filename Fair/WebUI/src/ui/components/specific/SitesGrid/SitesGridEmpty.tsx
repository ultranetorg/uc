export type SitesGridEmptyProps = {
  message: string
}

export const SitesGridEmpty = ({ message }: SitesGridEmptyProps) => (
  <span className="py-6 text-lg leading-5 text-gray-800">{message}</span>
)
