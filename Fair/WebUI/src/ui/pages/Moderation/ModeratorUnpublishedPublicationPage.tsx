import { useParams } from "react-router-dom"
import { ProductFields } from "ui/components/proposal"
import { ModeratorPublicationHeader } from "ui/components/specific"

export const ModeratorUnpublishedPublicationPage = () => {
  const { productId } = useParams()

  if (!productId) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      {/* <ModeratorPublicationHeader /> */}
      <ProductFields productIds={[productId]} />
    </div>
  )
}
