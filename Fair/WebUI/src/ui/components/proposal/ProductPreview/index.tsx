import { memo } from "react"
import { UseQueryResult } from "@tanstack/react-query"
import { ProductFieldViewModel } from "types"
import { SvgX } from "assets"
import { PublicationView } from "../../../views/PublictionView"
import { Modal } from "../../Modal"

interface ProductPreviewProps {
  response: UseQueryResult<ProductFieldViewModel[], Error>
  onClose?: () => void
}

export const ProductPreviewModal = memo(({ response, onClose }: ProductPreviewProps) => {
  const title = "Product Fields Preview"

  return (
    <Modal className="border border-gray-300 p-0">
      <div className="divide-y divide-gray-300">
        <div className="flex items-center justify-between gap-6 px-6 py-4">
          <span className="select-none text-base font-semibold leading-5">{title}</span>
          <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onClose} />
        </div>

        <PublicationView response={response} />
      </div>
    </Modal>
  )
})
