export type PublicationViewHeaderProps = {
  title: string
  logo: string
  tags?: string
  slogan?: string
}

export const PublicationViewHeader = ({ title, logo, tags, slogan }: PublicationViewHeaderProps) => {
  return (
    <div className="flex items-center gap-4">
      <div className="size-17 overflow-hidden rounded-2xl">
        <img alt="logo" className="size-full object-cover" src={logo} />
      </div>
      <div className="flex flex-col text-gray-800">
        <span className="text-2xl font-semibold">{title}</span>
        {tags && <span className="text-2xs font-medium">{tags}</span>}
        {slogan && <span className="text-2xs font-medium">{slogan}</span>}
      </div>
    </div>
  )
}
