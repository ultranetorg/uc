export const ENTITY_PREFIXES = {
  siteId: "s-",
  categoryId: "c-",
  publicationId: "p-",
  userId: "u-",
  publisherId: "pb-",
} as const

export type EntityParam = keyof typeof ENTITY_PREFIXES

export const addPrefix = (key: EntityParam, value: string): string =>
  `${ENTITY_PREFIXES[key]}${stripPrefix(key, value)}`

export const stripPrefix = (key: EntityParam, value: string | undefined): string | undefined =>
  value?.startsWith(ENTITY_PREFIXES[key]) ? value.slice(ENTITY_PREFIXES[key].length) : value

const sitePath = (siteId: string): string => `/${addPrefix("siteId", siteId)}`

const withTab = (base: string, tabKey?: string): string => (tabKey ? `${base}/${tabKey}` : base)

export const routes = {
  home: () => "/",
  site: (siteId: string) => sitePath(siteId),
  category: (_siteId: string, categoryId: string) => `/${addPrefix("categoryId", categoryId)}`,
  publication: (_siteId: string, publicationId: string) => `/${addPrefix("publicationId", publicationId)}`,
  search: (siteId: string) => `${sitePath(siteId)}/s`,
  about: (siteId: string) => `${sitePath(siteId)}/i`,

  user: (siteId: string, userId: string) => `/${siteId}/u/${userId}`,
  publisher: (siteId: string, publisherId: string) => `/${siteId}/e/${publisherId}`,
  author: (siteId: string, authorId: string) => `/${siteId}/a/${authorId}`,
  profile: (address: string) => `/p/${address}`,

  governance: {
    create: (siteId: string) => `${sitePath(siteId)}/g/new`,
    surveys: (siteId: string) => `${sitePath(siteId)}/g/p`,
    survey: (siteId: string, surveyId: string) => `${sitePath(siteId)}/g/p/${surveyId}`,
    referendums: (siteId: string) => `${sitePath(siteId)}/g/r`,
    referendum: (siteId: string, referendumId: string) => `${sitePath(siteId)}/g/r/${referendumId}`,
  },

  moderation: {
    root: (siteId: string) => `${sitePath(siteId)}/m`,
    create: (siteId: string) => `${sitePath(siteId)}/m/new`,
    createPublication: (siteId: string) => `${sitePath(siteId)}/m/new-publication`,
    proposal: (siteId: string, discussionId: string) => `${sitePath(siteId)}/m/p/${discussionId}`,

    moderators: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/m/m`, tabKey),
    moderatorProposal: (siteId: string, proposalId: string) => `${sitePath(siteId)}/m/m/p/${proposalId}`,

    publications: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/m/c`, tabKey),
    moderatorPublication: (siteId: string, proposalId: string) => `${sitePath(siteId)}/m/c/p/${proposalId}`,
    changedPublication: (siteId: string, publicationId: string) => `${sitePath(siteId)}/m/c/c/${publicationId}`,
    unpublishedPublication: (siteId: string, publicationId: string) => `${sitePath(siteId)}/m/c/u/${publicationId}`,

    publishers: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/m/a`, tabKey),
    publisherProposal: (siteId: string, proposalId: string) => `${sitePath(siteId)}/m/a/r/${proposalId}`,
    publisher: (siteId: string, publisherId: string) => `${sitePath(siteId)}/m/a/p/${publisherId}`,

    reviews: (siteId: string) => `${sitePath(siteId)}/m/r`,

    users: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/m/u`, tabKey),
    user: (siteId: string, userId: string) => `${sitePath(siteId)}/m/u/u/${userId}`,

    preview: (siteId: string) => `${sitePath(siteId)}/m/v`,
  },
} as const
