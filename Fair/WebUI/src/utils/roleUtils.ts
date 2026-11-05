import { AuthorDetails, Store } from "types"

export const isAuthorPublisher = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store?.authorsIds?.includes(author.id))

export const isAuthorModerator = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store && author.ownersIds.some(owner => store.moderatorsIds.includes(owner.id)))

export const isUserPublisher = (store?: Store, userAuthorsIds?: string[]): boolean =>
  store && userAuthorsIds ? Boolean(store?.authorsIds?.some(id => userAuthorsIds?.includes(id))) : false

export const isUserModerator = (store: Store | undefined, userId?: string): boolean =>
  store && userId ? Boolean(store?.moderatorsIds?.includes(userId)) : false
