import { useCallback, useEffect } from "react"
import { Link, useLocation, useNavigate, useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetAuthor, useGetAuthorPublications } from "entities"
import { SvgProfilePageClose } from "assets"
import { AuthorProfile } from "ui/components/author"
import { useTranslation } from "react-i18next"

export const AuthorPage = () => {
  const location = useLocation()
  const navigate = useNavigate()
  const { siteId, authorId } = useParams()
  const { t } = useTranslation("author")

  const { isPending, data: author } = useGetAuthor(authorId)
  const { isPending: isPublicationsPending, data: publications } = useGetAuthorPublications(siteId, author?.id)

  useDocumentTitle(author?.title ? `Author - ${author?.title} | Fair` : "Author | Fair")

  const state = location.state as { backgroundLocation?: Location } | undefined
  const backgroundLocation = state?.backgroundLocation

  const close = useCallback(() => {
    navigate(-1)
  }, [navigate])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        close()
      }
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [close])

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <div className="fixed inset-0 z-50 bg-white">
      <div className="mx-auto max-w-[1240px]">
        <div className="flex pl-17">
          <div className="flex w-full gap-6">
            <div className="flex w-full flex-col gap-6 py-8">
              <AuthorProfile
                title={author.title}
                nickname={"nickname"}
                description={
                  "Electronic Arts is a cozy digital game store featuring a mix of indie gems and timeless classics. It’s easy to discover something new and exciting thanks to an active community and honest reviews. At its core, GameNest is built on democratic principles: content creators choose the moderators, and key decisions are made collectively with the users. It’s a platform truly run by its community, where passionate people create and curate the content together. At its core, GameNest is built on democratic principles: content creators choose the moderators, and key decisions are made collectively with the users. It’s a platform truly run by its community, where passionate people create and curate the content together. At its core, GameNest is built on democratic principles: content creators choose the moderators, and key decisions are made collectively with the users. It’s a platform truly run by its community, where passionate people create and curate the content together."
                }
                links={[
                  { link: "http://home.net", text: "Official Site" },
                  { link: "http://chux.net/testcom1", text: "No name social network" },
                  { link: "http://aas.com/testcom", text: "This is very very very very very very looooong name" },
                  { link: "https://facebook.com/test", text: "Facebook" },
                  { link: "https://www.youtube.com/testcom", text: "YouTube" },
                  { link: "https://www.instagram.com/testcom", text: "Instagram" },
                  { link: "https://x.com/testcom", text: "Twitter" },
                  { link: "https://discord.com/testcom", text: "Discord" },
                ]}
                registeredDate={3324}
                aboutLabel={t("about")}
                authorLabel={t("author")}
                linksLabel={t("links")}
                readLessLabel={t("readLess")}
                readMoreLabel={t("readMore")}
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
  )
}

/*
              <div className="flex flex-col text-black">
                <span>ID: {author.id}</span>
                <span>NICKNAME: {author.nickname}</span>
                <span>TITLE: {author.title}</span>
                <span>OWNER ID: {JSON.stringify(author.ownersIds)}</span>
                <h1>PUBLICATIONS:</h1>
                {isPublicationsPending || !publications ? (
                  <div>⌛ LOADING PUBLICATIONS</div>
                ) : publications.items.length === 0 ? (
                  <div>🚫 NO PUBLICATIONS</div>
                ) : (
                  <div className="flex flex-col flex-wrap">
                    {publications.items.map(p => (
                      <Link to={`/${siteId}/p/${p.id}`} key={p.id}>
                        <div className="flex flex-col border border-red-300">
                          <span>PRODUCT ID: {p.id}</span>
                          <span>TITLE: {p.title}</span>
                        </div>
                      </Link>
                    ))}
                  </div>
                )}
              </div>
*/
