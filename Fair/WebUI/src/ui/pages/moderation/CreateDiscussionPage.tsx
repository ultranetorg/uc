import { useStoreContext } from "app"
import { useStoreTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateDiscussionPage = () => {
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Create New Moderator Proposal")

  return <CreateProposalView proposalType="discussion" />
}
