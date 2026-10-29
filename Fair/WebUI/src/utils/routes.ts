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
  category: (categoryId: string) => `/${addPrefix("categoryId", categoryId)}`,
  publication: (publicationId: string) => `/${addPrefix("publicationId", publicationId)}`,
  search: (storeId: string) => `${storePath(storeId)}/search`,
  about: (storeId: string) => `${storePath(storeId)}/about`,
  reviewer: (storeId: string, reviewerId: string) => `${storePath(storeId)}/${addPrefix("userId", reviewerId)}`,
  publisher: (storeId: string, publisherId: string) => `${storePath(storeId)}/${addPrefix("publisherId", publisherId)}`,

  author: (authorId: string) => `/${addPrefix("authorId", authorId)}`,

  moderation: {
    createProposal: (storeId: string) => `${storePath(storeId)}/proposals/new`,

    preview: (storeId: string) => `${storePath(storeId)}/publications/preview`,

    // Policy Surveys
    policySurveys: (storeId: string) => `${storePath(storeId)}/policy-surveys`,
    policySurveyDetails: (storeId: string, surveyId: string) => `${storePath(storeId)}/policy-surveys/${surveyId}`,

    // Referendums
    referendums: (storeId: string) => `${storePath(storeId)}/referendums`,
    referendum: (storeId: string, referendumId: string) => `${storePath(storeId)}/referendums/${referendumId}`,
    // Proposals
    proposals: (storeId: string) => `${storePath(storeId)}/proposals`,
    proposal: (storeId: string, proposalId: string) => `${storePath(storeId)}/proposals/${proposalId}`,

    // Moderators
    moderators: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/moderators`, tabKey),
    moderatorProposal: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/moderators/proposals/${proposalId}`,
    // Publishers
    publishers: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/publishers`, tabKey),
    publisherProposal: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/publishers/proposals/${proposalId}`,
    publisher: (storeId: string, publisherId: string) => `${storePath(storeId)}/publishers/details/${publisherId}`,
    // Users
    users: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/users`, tabKey),
    user: (storeId: string, userId: string) => `${storePath(storeId)}/users/details/${userId}`,

    // Publications
    createPublication: (storeId: string) => `${storePath(storeId)}/publications/new`,
    publications: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/publications`, tabKey),
    moderatorPublication: (storeId: string, proposalId: string) =>
      `${storePath(storeId)}/publications/proposals/${proposalId}`,
    changedPublication: (storeId: string, publicationId: string) =>
      `${storePath(storeId)}/publications/changed/${publicationId}`,
    unpublishedPublication: (storeId: string, publicationId: string) =>
      `${storePath(storeId)}/publications/unpublished/${publicationId}`,
    // Reviews
    reviews: (storeId: string, tabKey?: string) => withTab(`${storePath(storeId)}/reviews`, tabKey),
  },
} as const
