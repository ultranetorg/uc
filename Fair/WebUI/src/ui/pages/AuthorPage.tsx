import { useCallback, useEffect, useState } from "react"
import { Link, useLocation, useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { useDocumentTitle } from "usehooks-ts"

import { SvgProfilePageClose } from "assets"
import { useGetAuthor, useGetAuthorPublications } from "entities"
import { Pagination } from "ui/components"
import { AuthorProfile } from "ui/components/author"
import { PublicationsTable, PublicationStoresItem, PublicationStoresModal } from "ui/components/specific"

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

export const AuthorPage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { siteId, authorId } = useParams()
  const { t } = useTranslation("author")

  const [isModalOpen, setModalOpen] = useState(false)

  const { isPending, data: author } = useGetAuthor(authorId)
  const { data: publications } = useGetAuthorPublications(siteId, author?.id)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Fair` : "Author | Fair")

  const state = location.state as { backgroundLocation?: Location } | undefined
  const backgroundLocation = state?.backgroundLocation

  const close = useCallback(() => {
    navigate(-1)
  }, [navigate])

  useEffect(() => window.scrollTo({ top: 0, behavior: "smooth" }), [])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        if (isModalOpen) {
          setModalOpen(false)
        } else {
          close()
        }
      }
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [close, isModalOpen])

  const handlePublicationStoresClick = useCallback((_id: string) => setModalOpen(true), [])

  const handleModalClose = useCallback(() => setModalOpen(false), [])

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <>
      <div className="absolute inset-0 z-50 min-h-screen w-full bg-white">
        <div className="mx-auto max-w-[1240px]">
          <div className="flex pl-17">
            <div className="flex w-full gap-6">
              <div className="flex w-full flex-col gap-6 py-8">
                <AuthorProfile
                  t={t}
                  title={author.title}
                  nickname={author.nickname}
                  avatar={author.avatar}
                  description={author.description}
                  links={author.links}
                  registeredDate={3324}
                />
                <div className="flex items-center justify-between">
                  <span className="text-3.5xl font-semibold leading-10">42 products</span>
                  <Pagination pagesCount={3} onPageChange={page => console.log(page)} page={2} />
                </div>
                {publications?.items && (
                  <PublicationsTable
                    className="flex flex-col rounded-lg border border-gray-300 bg-gray-100"
                    items={publications.items}
                    onPublicationStoresClick={handlePublicationStoresClick}
                  />
                )}
              </div>
              <div className="pt-7.5">
                {backgroundLocation ? (
                  <SvgProfilePageClose className="cursor-pointer" onClick={close} />
                ) : (
                  <Link to={`/`}>
                    <SvgProfilePageClose className="cursor-pointer" />
                  </Link>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
      {isModalOpen && <PublicationStoresModal items={TEST_ITEMS} onClose={handleModalClose} />}
    </>
  )
}
