import { Route, Routes, useLocation } from "react-router-dom"

import { useParams } from "hooks"
import { ModerationLayout, UsersLayout } from "ui/layouts"

import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  ErrorPage,
  PublicationPage,
  PublisherPage,
  ReviewerPage,
  SearchPage,
  SitePage,
  UserPage,
} from "ui/pages"
import {
  CreateReferendumPage,
  PerpetualSurveyPage,
  PerpetualSurveysPage,
  ReferendumPage,
  ReferendumsPage,
} from "ui/pages/governance"
import {
  CreateDiscussionPage,
  ModeratorChangedPublicationPage,
  ModeratorCreatePublicationPage,
  ModeratorProposalPage,
  ModeratorPublicationPage,
  ModeratorsPage,
  PreviewPage,
  ProposalPage,
  ProposalsPage,
  PublicationsPage,
  PublisherPage as ModerationPublisherPage,
  PublisherProposalPage,
  PublishersPage,
  ReviewsPage,
  UnpublishedPublicationPage,
  UsersPage,
} from "ui/pages/moderation"
import { CreateProposalProvider } from "ui/views"
import { FullscreenPageView } from "ui/views/FullscreenPageView"
import { ENTITY_PREFIXES } from "utils"

const SiteEntityRoute = () => {
  const location = useLocation()
  const { appEntity = "" } = useParams()

  const state = location.state as { backgroundLocation?: Location; defaultTabKey?: string } | undefined

  if (appEntity.startsWith(ENTITY_PREFIXES.userId)) {
    return state?.backgroundLocation ? (
      <FullscreenPageView>
        <ReviewerPage />
      </FullscreenPageView>
    ) : (
      <ReviewerPage />
    )
  }

  if (appEntity.startsWith(ENTITY_PREFIXES.publisherId))
    return state?.backgroundLocation ? (
      <FullscreenPageView>
        <PublisherPage />
      </FullscreenPageView>
    ) : (
      <PublisherPage />
    )

  return <ErrorPage />
}

export const EntityRoute = () => {
  const { appEntity = "", "*": rest } = useParams()

  if (appEntity.startsWith(ENTITY_PREFIXES.siteId)) {
    return (
      <Routes>
        <Route>
          <Route index element={<SitePage />} />
          <Route path="s" element={<SearchPage />} />
          <Route path="i" element={<AboutPage />} />
          <Route path=":appEntity" element={<SiteEntityRoute />} />

          {/* Governance */}
          <Route
            path="g/new"
            element={
              <CreateProposalProvider>
                <CreateReferendumPage />
              </CreateProposalProvider>
            }
          />
          <Route path="g/p/:perpetualSurveyId" element={<PerpetualSurveyPage />} />
          <Route path="g/r/:referendumId" element={<ReferendumPage />} />
          <Route path="g/p" element={<PerpetualSurveysPage />} />
          <Route path="g/r" element={<ReferendumsPage />} />

          {/* Moderation */}
          <Route
            path="m/new"
            element={
              <CreateProposalProvider>
                <CreateDiscussionPage />
              </CreateProposalProvider>
            }
          />
          <Route path="m/new-publication" element={<ModeratorCreatePublicationPage />} />
          <Route path="m" element={<ModerationLayout />}>
            <Route index element={<ProposalsPage />} />
            <Route path="p/:discussionId" element={<ProposalPage />} />
            <Route path="m/p/:proposalId" element={<ModeratorProposalPage />} />
            <Route path="m/:tabKey?" element={<ModeratorsPage />} />
            <Route path="c/p/:proposalId" element={<ModeratorPublicationPage />} />
            <Route path="c/c/:publicationId" element={<ModeratorChangedPublicationPage />} />
            <Route path="c/u/:publicationId" element={<UnpublishedPublicationPage />} />
            <Route path="c/:tabKey?" element={<PublicationsPage />} />
            <Route path="a/r/:proposalId" element={<PublisherProposalPage />} />
            <Route path="a/:tabKey?" element={<PublishersPage />} />
            <Route path="a/p/:publisherId" element={<ModerationPublisherPage />} />
            <Route path="r" element={<ReviewsPage />} />
            <Route path="u" element={<UsersLayout />}>
              <Route path=":tabKey?" element={<UsersPage />} />
              <Route path="u/:userId" element={<UserPage />} />
            </Route>
            <Route path="v" element={<PreviewPage />} />
          </Route>

          {/* любой другой путь под сайтом */}
          <Route path="*" element={<ErrorPage />} />
        </Route>
      </Routes>
    )
  }

  if (rest) {
    return <ErrorPage />
  }

  if (appEntity.startsWith(ENTITY_PREFIXES.categoryId)) return <CategoryPage />
  if (appEntity.startsWith(ENTITY_PREFIXES.publicationId)) return <PublicationPage />

  if (appEntity.startsWith(ENTITY_PREFIXES.userId)) {
    return (
      <FullscreenPageView>
        <UserPage />
      </FullscreenPageView>
    )
  }

  if (appEntity.startsWith(ENTITY_PREFIXES.publisherId)) {
    return (
      <FullscreenPageView>
        <AuthorPage />
      </FullscreenPageView>
    )
  }

  // Profile

  return <ErrorPage />
}
