export const ENTITY_PREFIXES = {
  siteId: "site",
  categoryId: "category",
  publicationId: "publication",
  publisherId: "publisher",
  userId: "user",
  profileId: "profile",
  authorId: "author",
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
  search: (siteId: string) => `${sitePath(siteId)}/search`,
  about: (siteId: string) => `${sitePath(siteId)}/about`,
  reviewer: (siteId: string, reviewerId: string) => `${sitePath(siteId)}/${addPrefix("userId", reviewerId)}`,
  publisher: (siteId: string, publisherId: string) => `${sitePath(siteId)}/${addPrefix("publisherId", publisherId)}`,

  author: (authorId: string) => `/${addPrefix("authorId", authorId)}`,

  governance: {
    surveys: (siteId: string) => `${sitePath(siteId)}/surveys`,
    survey: (siteId: string, surveyId: string) => `${sitePath(siteId)}/surveys/${surveyId}`,

    createReferendum: (siteId: string) => `${sitePath(siteId)}/referendums/new`,
    referendums: (siteId: string) => `${sitePath(siteId)}/referendums`,
    referendum: (siteId: string, referendumId: string) => `${sitePath(siteId)}/referendums/${referendumId}`,
  },

  moderation: {
    createProposal: (siteId: string) => `${sitePath(siteId)}/proposals/new`,
    proposals: (siteId: string) => `${sitePath(siteId)}/proposals`,
    proposal: (siteId: string, discussionId: string) => `${sitePath(siteId)}/proposals/${discussionId}`,

    moderators: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/moderators`, tabKey),
    moderatorProposal: (siteId: string, proposalId: string) => `${sitePath(siteId)}/moderators/proposals/${proposalId}`,

    createPublication: (siteId: string) => `${sitePath(siteId)}/publications/new`,
    preview: (siteId: string) => `${sitePath(siteId)}/publications/preview`,
    publications: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/publications`, tabKey),
    moderatorPublication: (siteId: string, proposalId: string) =>
      `${sitePath(siteId)}/publications/proposals/${proposalId}`,
    changedPublication: (siteId: string, publicationId: string) =>
      `${sitePath(siteId)}/publications/changed/${publicationId}`,
    unpublishedPublication: (siteId: string, publicationId: string) =>
      `${sitePath(siteId)}/publications/unpublished/${publicationId}`,

    publishers: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/publishers`, tabKey),
    publisherProposal: (siteId: string, proposalId: string) => `${sitePath(siteId)}/publishers/proposals/${proposalId}`,
    publisher: (siteId: string, publisherId: string) => `${sitePath(siteId)}/publishers/details/${publisherId}`,

    reviews: (siteId: string) => `${sitePath(siteId)}/reviews`,

    users: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/users`, tabKey),
    user: (siteId: string, userId: string) => `${sitePath(siteId)}/users/details/${userId}`,
  },
} as const
