import { useCallback, useEffect, useState } from "react"
import { Link, useLocation, useNavigate } from "react-router-dom"

import { SvgProfilePageClose } from "assets"
import { useGetAuthor } from "entities"
import { useEscapeKey, useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { AuthorPublicationsView } from "ui/views"
import { routes } from "utils"

export const PublisherPage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { publisherId } = useParams()
  const siteId = useResolveSiteId()

  const [isModalOpen, setModalOpen] = useState(false)

  const { isPending, data: author } = useGetAuthor(publisherId)

  useSiteTitle(author?.title ? `Publisher - ${author?.title}` : undefined)

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
                  <Link to={routes.home()}>
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
