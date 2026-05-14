import { useCallback, useEffect, useState } from "react"
import { Link, useLocation, useNavigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { SvgProfilePageClose } from "assets"
import { useGetAuthor } from "entities"
import { useEscapeKey } from "hooks"
import { AuthorPublicationsView } from "ui/views"

export const AuthorPage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { siteId, authorId } = useParams()

  const [isModalOpen, setModalOpen] = useState(false)

  const { isPending, data: author } = useGetAuthor(authorId)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Fair` : "Author | Fair")

  const state = location.state as { backgroundLocation?: Location } | undefined
  const backgroundLocation = state?.backgroundLocation

  const close = useCallback(() => navigate(-1), [navigate])

  useEffect(() => window.scrollTo({ top: 0, behavior: "smooth" }), [])

  useEscapeKey(
    useCallback(() => {
      if (isModalOpen) setModalOpen(false)
      else close()
    }, [close, isModalOpen]),
  )

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
                <AuthorPublicationsView
                  size="compact"
                  siteId={siteId!}
                  author={author}
                  isModalOpen={isModalOpen}
                  onModalOpenChange={setModalOpen}
                />
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
    </>
  )
}
