import { Proposal, PublicationCreation } from "types"

export const getProductId = (proposal?: Proposal): string | undefined => {
  if (!proposal || !proposal.operation.length || proposal.options[0].operation.$type !== "publication-creation")
    return undefined

  return (proposal.options[0].operation as PublicationCreation).productId
}

export const getPublicationId = (proposal?: Proposal): string | undefined => {
  if (!proposal || !proposal.operation.length) return undefined

  if (
    proposal.options[0].operation.$type === "publication-deletion" ||
    proposal.options[0].operation.$type === "publication-publish" ||
    proposal.options[0].operation.$type === "publication-updation" ||
    proposal.options[0].operation.$type === "publication-unpublish"
  ) {
    return (proposal.options[0].operation as unknown as { publicationId: string }).publicationId
  }

  return undefined
}
