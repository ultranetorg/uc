import { useStorePoliciesContext, useStoreRolesContext } from "app"
import { OperationType } from "types"
import { isReferendum } from "utils"

/**
 * Moderator can comment Referendum as well as Proposal. Publisher can only comment Referendum.
 * @param operation Proposal operation.
 * @returns `true` if current user can create comment, otherwise `false`.
 */
export const useCanCreateComment = (operation?: OperationType) => {
  const { policies } = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()

  return isModerator || (isPublisher && isReferendum(operation, policies))
}
