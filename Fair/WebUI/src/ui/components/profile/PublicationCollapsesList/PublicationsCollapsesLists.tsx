import { useCallback, useState } from "react"

import { PublicationStoresItem, PublicationStoresModal } from "../PublicationStoresModal"

import { PublicationsCollapse, PublicationsCollapseProps } from "./PublicationsCollapse"

const TEST_ITEMS: PublicationStoresItem[] = [
  { siteTitle: "GameNest", publicationDate: 1 },
  { siteTitle: "PixelPioneers", publicationDate: 2 },
  { siteTitle: "QuestCraft", publicationDate: 3 },
  { siteTitle: "CodeCrafters", publicationDate: 4 },
  { siteTitle: "LevelUpAcademy", publicationDate: 5 },
  { siteTitle: "EpicVentures", publicationDate: 6 },
  { siteTitle: "PixelVerse", publicationDate: 7 },
  { siteTitle: "DreamForge", publicationDate: 8 },
  { siteTitle: "FlyBear", publicationDate: 9 },
  { siteTitle: "Prussia", publicationDate: 10 },
]

export type PublicationsCollapsesListsProps = {
  items: PublicationsCollapseProps[]
}

export const PublicationsCollapsesLists = ({ items }: PublicationsCollapsesListsProps) => {
  const [activeCollapseId, setActiveCollapseId] = useState("")
  const [isModalOpen, setModalOpen] = useState(false)
  console.log(isModalOpen)

  const handleExpand = useCallback((id: string) => setActiveCollapseId(prev => (prev !== id ? id : "")), [])

  const handlePublicationStoresClick = useCallback((id: string) => setModalOpen(true), [])

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
      {isModalOpen && <PublicationStoresModal items={TEST_ITEMS} />}
    </>
  )
}
