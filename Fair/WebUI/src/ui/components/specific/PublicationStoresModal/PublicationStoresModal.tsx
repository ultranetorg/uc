import { memo } from "react"

import { Link } from "react-router-dom"
import { SvgX } from "assets"
import { DEFAULT_PAGE_SIZE_20 } from "config"
import { Modal, ModalProps } from "ui/components"
import { routes } from "utils"

import { useGetProductSites } from "entities"

import { PublicationStoreRow } from "./PublicationStoreRow"

export type PublicationStoresItem = {
  siteTitle: string
  publicationDate: number
}

type PublicationStoresModalBaseProps = {
  publicationId: string
}

export type PublicationStoresModalProps = Pick<ModalProps, "onClose"> & PublicationStoresModalBaseProps

export const PublicationStoresModal = memo(({ onClose, publicationId }: PublicationStoresModalProps) => {
  const { data: sites } = useGetProductSites(publicationId, 0, DEFAULT_PAGE_SIZE_20)

  if (!sites) {
    return <>LOADING</>
  }

  return (
    <Modal className="max-h-142.25 w-120 border border-gray-300 p-0">
      <div className="divide-y divide-gray-300">
        <div className="flex items-center justify-between gap-6 px-6 py-4">
          <span className="select-none text-base font-semibold leading-5">Published in: {sites.totalItems} stores</span>
          <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onClose} />
        </div>
        <div className="p-4">
          <div className="flex select-none items-center gap-3 p-2 text-2xs font-medium leading-4.25">
            <span className="w-[56%]">Store</span>
            {/* <span>Publish date</span> */}
          </div>
          <div className="max-h-[448px] overflow-y-scroll">
            {sites.items.map(x => (
              <Link to={routes.site(x.id)} title={x.title}>
                <PublicationStoreRow key={x.id} title={x.title} avatarId={x.avatarId} />
              </Link>
            ))}
            {/* {items.map((x, i) => (
                <PublicationStoreRow key={x.siteTitle + i} title={x.siteTitle} publicationDate={x.publicationDate} />
              ))} */}
          </div>
        </div>
      </div>
    </Modal>
  )
})
