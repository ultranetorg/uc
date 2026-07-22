import { useStoreContext } from "app"
import { useStoreTitle } from "hooks"
import { CreateProposalView } from "ui/views"

export const CreateReferendumPage = () => {
  const { store } = useStoreContext()

  useStoreTitle(store?.title, "Create New Publisher Referendum")

  return <CreateProposalView proposalType="referendum" />
}
