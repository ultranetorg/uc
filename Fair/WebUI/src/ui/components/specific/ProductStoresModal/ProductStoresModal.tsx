import { memo } from "react"
import { Link } from "react-router-dom"

import { SvgX } from "assets"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { useEscapeKey } from "hooks"
import { Modal, ModalProps } from "ui/components"
import { routes } from "utils"

import { useGetProductStores } from "entities"

import { ProductStoreRow } from "./ProductStoreRow"
import { ProductStoresModalSkeleton } from "./ProductStoresModal.Skeleton"

export type ProductStoresItem = {
  storeTitle: string
  publicationDate: number
}

type ProductStoresModalBaseProps = {
  productId: string
}

export type ProductStoresModalProps = Pick<ModalProps, "onClose"> & ProductStoresModalBaseProps

export const ProductStoresModal = memo(({ onClose, productId }: ProductStoresModalProps) => {
  const { data: stores } = useGetProductStores(productId, 0, DEFAULT_PAGE_SIZE_20)

  useEscapeKey(onClose)

  if (!stores) return <ProductStoresModalSkeleton onClose={onClose} />

  return (
    <Modal className="max-h-142.25 w-120 border border-gray-300 p-0">
      <div className="divide-y divide-gray-300">
        <div className="flex items-center justify-between gap-6 px-6 py-4">
          <span className="text-base leading-5 font-semibold select-none">
            Published in: {stores.totalItems} stores
          </span>
          <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onClose} />
        </div>
        <div className="p-4">
          <div className="flex items-center gap-3 p-2 text-2xs leading-4.25 font-medium select-none">
            <span className="w-[56%]">Store</span>
          </div>
          <div className="max-h-112 overflow-y-scroll">
            {stores.items.map(x => (
              <Link to={routes.publication(x.publicationId)} title={x.title} key={x.storeId}>
                <ProductStoreRow key={x.storeId} title={x.title} avatarId={x.avatarId} />
              </Link>
            ))}
          </div>
        </div>
      </div>
    </Modal>
  )
})
