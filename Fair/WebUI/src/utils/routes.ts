/**
 * Centralized application route builders.
 *
 * Single source of truth for constructing in-app navigation paths used in
 * `<Link to>`, `navigate()`, breadcrumbs, redirects, etc. The route tree itself
 * is declared in `app/Router.tsx`; this module mirrors it for path generation.
 *
 * Note: trailing slashes are intentionally omitted — react-router matches paths
 * with and without a trailing slash identically.
 */

const withTab = (base: string, tabKey?: string): string => (tabKey ? `${base}/${tabKey}` : base)

export const routes = {
  home: () => "/",
  site: (siteId: string) => `/${siteId}`,
  category: (siteId: string, categoryId: string) => `/${siteId}/c/${categoryId}`,
  publication: (siteId: string, publicationId: string) => `/${siteId}/p/${publicationId}`,
  search: (siteId: string) => `/${siteId}/s`,
  about: (siteId: string) => `/${siteId}/i`,
  user: (siteId: string, userId: string) => `/${siteId}/u/${userId}`,
  publisher: (siteId: string, publisherId: string) => `/${siteId}/e/${publisherId}`,
  author: (siteId: string, authorId: string) => `/${siteId}/a/${authorId}`,
  profile: (address: string) => `/p/${address}`,

  governance: {
    create: (siteId: string) => `/${siteId}/g/new`,
    surveys: (siteId: string) => `/${siteId}/g/p`,
    survey: (siteId: string, surveyId: string) => `/${siteId}/g/p/${surveyId}`,
    referendums: (siteId: string) => `/${siteId}/g/r`,
    referendum: (siteId: string, referendumId: string) => `/${siteId}/g/r/${referendumId}`,
  },

  moderation: {
    root: (siteId: string) => `/${siteId}/m`,
    create: (siteId: string) => `/${siteId}/m/new`,
    createPublication: (siteId: string) => `/${siteId}/m/new-publication`,
    proposal: (siteId: string, discussionId: string) => `/${siteId}/m/p/${discussionId}`,

    moderators: (siteId: string, tabKey?: string) => withTab(`/${siteId}/m/m`, tabKey),
    moderatorProposal: (siteId: string, proposalId: string) => `/${siteId}/m/m/p/${proposalId}`,

    publications: (siteId: string, tabKey?: string) => withTab(`/${siteId}/m/c`, tabKey),
    moderatorPublication: (siteId: string, proposalId: string) => `/${siteId}/m/c/p/${proposalId}`,
    changedPublication: (siteId: string, publicationId: string) => `/${siteId}/m/c/c/${publicationId}`,
    unpublishedPublication: (siteId: string, publicationId: string) => `/${siteId}/m/c/u/${publicationId}`,

    publishers: (siteId: string, tabKey?: string) => withTab(`/${siteId}/m/a`, tabKey),
    publisherProposal: (siteId: string, proposalId: string) => `/${siteId}/m/a/r/${proposalId}`,
    publisher: (siteId: string, publisherId: string) => `/${siteId}/m/a/p/${publisherId}`,

    reviews: (siteId: string) => `/${siteId}/m/r`,

    users: (siteId: string, tabKey?: string) => withTab(`/${siteId}/m/u`, tabKey),
    user: (siteId: string, userId: string) => `/${siteId}/m/u/u/${userId}`,

    preview: (siteId: string) => `/${siteId}/m/v`,
  },
} as const
