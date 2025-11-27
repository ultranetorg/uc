import { memo } from "react"
import { UseQueryResult } from "@tanstack/react-query"
import { ProductFieldViewModel } from "types"
import { PublicationView } from "ui/views/PublictionView"
import { Modal } from "ui/components/Modal"

interface ProductPreviewProps {
  response: UseQueryResult<ProductFieldViewModel[], Error>
  onClose: () => void
}

export const ProductPreviewModal = memo(({ response, onClose }: ProductPreviewProps) => {
  const title = "Product Fields Preview"

  return (
    <Modal className="border border-gray-300" title={title} onClose={onClose}>
      <PublicationView response={response} />
    </Modal>
  )
})
