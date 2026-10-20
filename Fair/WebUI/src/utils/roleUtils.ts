import { AuthorDetails, Store, UserDetails } from "types"

export const isAuthorPublisher = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store?.authorsIds?.includes(author.id))

export const isAuthorModerator = (store: Store | undefined, author: AuthorDetails): boolean =>
  Boolean(store && author.ownersIds.some(owner => store.moderatorsIds.includes(owner.id)))

export const isUserPublisher = (store: Store | undefined, user: UserDetails | undefined): boolean =>
  store && user ? Boolean(store?.authorsIds?.some(id => user.authorsIds.includes(id))) : false

export const isUserModerator = (store: Store | undefined, user: UserDetails | undefined): boolean =>
  store && user ? Boolean(store?.moderatorsIds?.includes(user.id)) : false
