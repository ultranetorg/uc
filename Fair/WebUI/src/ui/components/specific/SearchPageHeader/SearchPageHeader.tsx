export type SearchPageHeaderProps = {
  searchResultsCount: number
  searchResultsLabel: string
}

export const SearchPageHeader = ({ searchResultsCount, searchResultsLabel }: SearchPageHeaderProps) => {
  return (
    <div>
      <div className="text-3.5xl leading-9.75 font-semibold">
        <span className="mr-2 text-gray-800">{searchResultsLabel}</span>
        <span className="text-gray-400">{searchResultsCount}</span>
      </div>
    </div>
  )
}
