import { AuthorDetails, Site, UserAuthors } from "types"

export const isAuthorPublisher = (site: Site | undefined, author: AuthorDetails): boolean =>
  Boolean(site?.authorsIds?.includes(author.id))

export const isAuthorModerator = (site: Site | undefined, author: AuthorDetails): boolean =>
  Boolean(site && author.ownersIds.some(owner => site.moderatorsIds.includes(owner.id)))

export const isUserPublisher = (site: Site | undefined, user: UserAuthors): boolean =>
  Boolean(site?.authorsIds?.some(id => user.authors.some(author => author.id === id)))

export const isUserModerator = (site: Site | undefined, user: UserAuthors): boolean =>
  Boolean(site?.moderatorsIds?.includes(user.id))
