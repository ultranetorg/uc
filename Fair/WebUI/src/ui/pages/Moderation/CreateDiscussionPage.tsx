import { useSiteContext } from "app"
import { useSiteTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateDiscussionPage = () => {
  const { site } = useSiteContext()

  useSiteTitle(site?.title, "Create New Moderator Proposal")

  return <CreateProposalView proposalType="discussion" />
}
