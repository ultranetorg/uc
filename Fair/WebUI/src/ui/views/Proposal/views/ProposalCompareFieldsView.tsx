import { memo, useMemo } from "react"

import { useGetProductCompareFields } from "entities"
import { ProductFieldsDiff } from "ui/components/specific"
import { PublicationUpdation } from "types"

import { ProposalTypeViewProps } from "./types"

const CompareFieldsForPublication = ({ id, version }: { id: string; version: number }) => {
  const { isFetching, data } = useGetProductCompareFields(id, version)

  if (isFetching || !data) return <div>LOADING</div>

  return <ProductFieldsDiff productFieldsFrom={data.from} productFieldsTo={data.to} />
}

export const ProposalCompareFieldsView = memo(({ proposal }: ProposalTypeViewProps) => {
  const publications = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationUpdation => operation.$type === "publication-updation")
        .map(operation => ({
          id: operation.publicationId,
          version: operation.version,
        })),
    [proposal],
  )

  if (!publications?.length) return null

  return (
    <>
      {publications.map(publication => (
        <CompareFieldsForPublication
          key={`${publication.id}_${publication.version}`}
          id={publication.id}
          version={publication.version}
        />
      ))}
    </>
  )
})
