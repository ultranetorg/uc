import { useCallback, useState } from "react"

import { ProductStoresModal } from "ui/components/specific"

import { PublicationsCollapse, PublicationsCollapseProps } from "./PublicationsCollapse"

export type PublicationsCollapsesListsProps = {
  items: PublicationsCollapseProps[]
}

export const PublicationsCollapsesLists = ({ items }: PublicationsCollapsesListsProps) => {
  const [activeCollapseId, setActiveCollapseId] = useState("")
  const [isModalOpen, setModalOpen] = useState(false)
  const [selectedPublicationId, setPublicationId] = useState<string | undefined>()

  const handleExpand = useCallback((id: string) => setActiveCollapseId(prev => (prev !== id ? id : "")), [])

  const handlePublicationStoresClick = useCallback((id: string) => {
    setModalOpen(true)
    setPublicationId(id)
  }, [])

  const handleModalClose = useCallback(() => setModalOpen(false), [])

  return (
    <>
      <div className="flex flex-col gap-4">
        {items?.map(x => (
          <PublicationsCollapse
            key={x.id}
            {...x}
            expanded={activeCollapseId === x.id}
            onExpand={handleExpand}
            onPublicationStoresClick={handlePublicationStoresClick}
          />
        ))}
      </div>
      {isModalOpen && <ProductStoresModal onClose={handleModalClose} productId={selectedPublicationId!} />}
    </>
  )
}
