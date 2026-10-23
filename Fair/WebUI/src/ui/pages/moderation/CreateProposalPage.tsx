import { useStoreContext } from "app"
import { useStoreTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateProposalPage = () => {
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Create New Proposal")

  return <CreateProposalView />
}
