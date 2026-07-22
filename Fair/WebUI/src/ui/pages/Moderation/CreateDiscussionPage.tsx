import { useStoreContext } from "app"
import { useStoreTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateDiscussionPage = () => {
  const { store: site } = useStoreContext()

  useStoreTitle(site?.title, "Create New Moderator Proposal")

  return <CreateProposalView proposalType="discussion" />
}
