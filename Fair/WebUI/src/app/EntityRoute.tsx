import { Route, Routes, useParams } from "react-router-dom"
import { ModerationLayout, UsersLayout } from "ui/layouts"

import { AboutPage, CategoryPage, ErrorPage, PublicationPage, SearchPage, SitePage, UserPage } from "ui/pages"
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
  PublisherPage,
  PublisherProposalPage,
  PublishersPage,
  ReviewsPage,
  UnpublishedPublicationPage,
  UsersPage,
} from "ui/pages/moderation"
import { CreateProposalProvider } from "ui/views"
import { ENTITY_PREFIXES } from "utils"

export const EntityRoute = () => {
  const { appEntity = "", "*": rest } = useParams()

  if (appEntity.startsWith(ENTITY_PREFIXES.siteId)) {
    return (
      <Routes>
        <Route>
          <Route index element={<SitePage />} />
          <Route path="s" element={<SearchPage />} />
          <Route path="i" element={<AboutPage />} />

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
            <Route path="a/p/:publisherId" element={<PublisherPage />} />
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

  if (appEntity.startsWith(ENTITY_PREFIXES.userId)) return <UserPage isFromModeration={false} />
  if (appEntity.startsWith(ENTITY_PREFIXES.publisherId)) return <PublisherPage isFromModeration={false} />

  return <ErrorPage />
}
