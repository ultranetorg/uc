import { AuthorDetails, Store, UserAuthors } from "types"

export const isAuthorPublisher = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store?.authorsIds?.includes(author.id))

export const isAuthorModerator = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store && author.ownersIds.some(owner => store.moderatorsIds.includes(owner.id)))

export const isUserPublisher = (store: Store | undefined, user: UserAuthors): boolean =>
  Boolean(store?.authorsIds?.some(id => user.authors.some(author => author.id === id)))

export const isUserModerator = (store: Store | undefined, user: UserAuthors): boolean =>
  Boolean(store?.moderatorsIds?.includes(user.id))
