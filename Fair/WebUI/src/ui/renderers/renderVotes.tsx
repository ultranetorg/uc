import { formatVotes } from "utils"

export const renderVotes = (votes: number[], votesRequired?: number) => {
  const formatted = formatVotes(votes)
  return (
    <div className="truncate text-center" title={formatted}>
      {votesRequired || votesRequired === -1 ? (
        <>
          {formatted} / {votesRequired}
        </>
      ) : (
        <>{formatted}</>
      )}
    </div>
  )
}
