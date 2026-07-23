export const ENTITY_PREFIXES = {
  storeId: "store",
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

const storePath = (storeId: string): string => `/${addPrefix("storeId", storeId)}`

const withTab = (base: string, tabKey?: string): string => (tabKey ? `${base}/${tabKey}` : base)

export const routes = {
  home: () => "/",
  store: (storeId: string) => storePath(storeId),
  category: (_storeId: string, categoryId: string) => `/${addPrefix("categoryId", categoryId)}`,
  publication: (_storeId: string, publicationId: string) => `/${addPrefix("publicationId", publicationId)}`,
  search: (storeId: string) => `${storePath(storeId)}/search`,
  about: (storeId: string) => `${storePath(storeId)}/about`,
  reviewer: (storeId: string, reviewerId: string) => `${storePath(storeId)}/${addPrefix("userId", reviewerId)}`,
  publisher: (storeId: string, publisherId: string) => `${storePath(storeId)}/${addPrefix("publisherId", publisherId)}`,

  author: (authorId: string) => `/${addPrefix("authorId", authorId)}`,

  governance: {
    surveys: (storeId: string) => `${storePath(storeId)}/surveys`,
    survey: (storeId: string, surveyId: string) => `${storePath(storeId)}/surveys/${surveyId}`,

    createReferendum: (storeId: string) => `${storePath(storeId)}/referendums/new`,
    referendums: (storeId: string) => `${storePath(storeId)}/referendums`,
    referendum: (storeId: string, referendumId: string) => `${storePath(storeId)}/referendums/${referendumId}`,
  },

  moderation: {
    createProposal: (storeId: string) => `${storePath(storeId)}/proposals/new`,
    proposals: (storeId: string) => `${storePath(storeId)}/proposals`,
    proposal: (storeId: string, discussionId: string) => `${storePath(storeId)}/proposals/${discussionId}`,

    moderators: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/moderators`, tabKey),
    moderatorProposal: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/moderators/proposals/${proposalId}`,

    createPublication: (storeId: string) => `${storePath(storeId)}/publications/new`,
    preview: (storeId: string) => `${storePath(storeId)}/publications/preview`,
    publications: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/publications`, tabKey),
    moderatorPublication: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/publications/proposals/${proposalId}`,
    changedPublication: (storeId: string, publicationId: string) =>
      `${storePath(storeId)}/publications/changed/${publicationId}`,
    unpublishedPublication: (storeId: string, publicationId: string) =>
      `${storePath(storeId)}/publications/unpublished/${publicationId}`,

    publishers: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/publishers`, tabKey),
    publisherProposal: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/publishers/proposals/${proposalId}`,
    publisher: (storeId: string, publisherId: string) => `${storePath(storeId)}/publishers/details/${publisherId}`,

    reviews: (storeId: string) => `${storePath(storeId)}/reviews`,

    users: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/users`, tabKey),
    user: (storeId: string, userId: string) => `${storePath(storeId)}/users/details/${userId}`,
  },
} as const
