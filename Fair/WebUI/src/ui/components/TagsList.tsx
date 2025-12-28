export type TagsListProps = {
  tags?: string[]
}

export const TagsList = ({ tags }: TagsListProps) =>
  tags && tags.length > 0 ? (
    <div className="flex select-none flex-wrap gap-2">
      {tags.map(tag => (
        <span key={tag} className="rounded bg-gray-200 px-2 py-1.5 text-2xs leading-4 text-gray-800">
          {tag}
        </span>
      ))}
    </div>
  ) : null
