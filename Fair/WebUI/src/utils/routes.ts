export const ENTITY_PREFIXES = {
  siteId: "site-",
  categoryId: "category-",
  publicationId: "publication-",
  publisherId: "publisher-",
  userId: "user-",
  profileId: "profile-",
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

  user: (userId: string) => `/${addPrefix("userId", userId)}`,
  profile: (userId: string) => `/${addPrefix("profileId", userId)}`, // TODO: fix later.
  author: (authorId: string) => `/${addPrefix("publisherId", authorId)}`,

  governance: {
    createReferendum: (siteId: string) => `${sitePath(siteId)}/governance/new-referendum`,
    surveys: (siteId: string) => `${sitePath(siteId)}/governance/surveys`,
    survey: (siteId: string, surveyId: string) => `${sitePath(siteId)}/governance/surveys/${surveyId}`,
    referendums: (siteId: string) => `${sitePath(siteId)}/governance/referendums`,
    referendum: (siteId: string, referendumId: string) => `${sitePath(siteId)}/governance/referendums/${referendumId}`,
  },

  moderation: {
    proposals: (siteId: string) => `${sitePath(siteId)}/moderation/proposals`,
    createProposal: (siteId: string) => `${sitePath(siteId)}/moderation/new-proposal`,

    proposal: (siteId: string, discussionId: string) => `${sitePath(siteId)}/moderation/proposals/${discussionId}`,

    moderators: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/moderation/moderators`, tabKey),
    moderatorProposal: (siteId: string, proposalId: string) =>
      `${sitePath(siteId)}/moderation/moderators/proposals/${proposalId}`,

    createPublication: (siteId: string) => `${sitePath(siteId)}/moderation/publications/new`,
    preview: (siteId: string) => `${sitePath(siteId)}/moderation/publications/preview`,
    publications: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/moderation/publications`, tabKey),
    moderatorPublication: (siteId: string, proposalId: string) =>
      `${sitePath(siteId)}/moderation/publications/proposals/${proposalId}`,
    changedPublication: (siteId: string, publicationId: string) =>
      `${sitePath(siteId)}/moderation/publications/changed/${publicationId}`,
    unpublishedPublication: (siteId: string, publicationId: string) =>
      `${sitePath(siteId)}/moderation/publications/unpublished/${publicationId}`,

    publishers: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/moderation/publishers`, tabKey),
    publisherProposal: (siteId: string, proposalId: string) =>
      `${sitePath(siteId)}/moderation/publishers/proposals/${proposalId}`,
    publisher: (siteId: string, publisherId: string) =>
      `${sitePath(siteId)}/moderation/publishers/details/${publisherId}`,

    reviews: (siteId: string) => `${sitePath(siteId)}/moderation/reviews`,

    users: (siteId: string, tabKey?: string) => withTab(`${sitePath(siteId)}/moderation/users`, tabKey),
    user: (siteId: string, userId: string) => `${sitePath(siteId)}/moderation/users/details/${userId}`,
  },
} as const
