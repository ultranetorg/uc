import { useSiteContext } from "app"
import { useSiteTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateReferendumPage = () => {
  const { site } = useSiteContext()

  useSiteTitle(site?.title, "Create New Publisher Referendum")

  return <CreateProposalView proposalType="referendum" />
}
