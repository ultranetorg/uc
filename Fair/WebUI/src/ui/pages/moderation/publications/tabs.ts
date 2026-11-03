import { OperationType } from "types"

/** Tab keys of the moderation publications page (also route segments) for the publication operations proposals. */
export const PUBLICATION_PROPOSALS_TAB_KEYS: Partial<Record<OperationType, string>> = {
  "publication-creation": "creations",
  "publication-deletion": "deletions",
  "publication-publish": "publishes",
  "publication-unpublish": "unpublishes",
  "publication-updation": "updations",
}
