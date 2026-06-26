import { ReactNode } from "react"
import { Route, Routes } from "react-router-dom"

import { useBackgroundLocation, useParams } from "hooks"
import { BaseLayout, SiteLayout } from "ui/layouts"
import { ConstrainedWidthLayout, ModerationLayout, PublishersLayout, UsersSectionLayout } from "ui/layouts/moderation"

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
  PublisherProposalPage,
  PublishersPage,
  ReviewsPage,
  UnpublishedPublicationPage,
  UsersPage,
} from "ui/pages/moderation"
import { FullscreenPageView, CreateProposalProvider } from "ui/views"
import { ENTITY_PREFIXES, EntityParam } from "utils"

import { MaybeFullscreen } from "./route"

const ERROR_PAGE = (
  <BaseLayout>
    <ErrorPage />
  </BaseLayout>
)

const SiteEntityRoute = () => {
  const { userId, publisherId } = useParams()
  const backgroundLocation = useBackgroundLocation()

  if (userId !== undefined)
    return (
      <MaybeFullscreen showFullscreen={!!backgroundLocation}>
        <ReviewerPage showDefaultBreadcrumbs={!backgroundLocation} />
      </MaybeFullscreen>
    )

  if (publisherId !== undefined)
    return (
      <MaybeFullscreen showFullscreen={!!backgroundLocation}>
        <PublisherPage showDefaultBreadcrumbs={!backgroundLocation} />
      </MaybeFullscreen>
    )

  return ERROR_PAGE
}

const ENTITY_ELEMENTS: Partial<Record<EntityParam, ReactNode>> = {
  categoryId: (
    <BaseLayout>
      <SiteLayout>
        <CategoryPage />
      </SiteLayout>
    </BaseLayout>
  ),
  publicationId: (
    <BaseLayout>
      <SiteLayout>
        <PublicationPage />
      </SiteLayout>
    </BaseLayout>
  ),
  userId: (
    <FullscreenPageView>
      <UserPage />
    </FullscreenPageView>
  ),
  publisherId: (
    <FullscreenPageView>
      <AuthorPage />
    </FullscreenPageView>
  ),
  // TODO: Add current user Profile here
}

export const EntityRoute = () => {
  const { appEntity = "", "*": rest } = useParams()

  if (appEntity.startsWith(ENTITY_PREFIXES.siteId)) {
    return (
      <Routes>
        <Route path=":subEntity" element={<SiteEntityRoute />} />

        <Route
          element={
            <BaseLayout>
              <SiteLayout />
            </BaseLayout>
          }
        >
          <Route index element={<SitePage />} />
          <Route path="search" element={<SearchPage />} />
          <Route path="about" element={<AboutPage />} />

          {/* Governance */}
          <Route
            path="referendums/new"
            element={
              <CreateProposalProvider>
                <CreateReferendumPage />
              </CreateProposalProvider>
            }
          />
          <Route path="surveys/:perpetualSurveyId" element={<PerpetualSurveyPage />} />
          <Route path="referendums/:referendumId" element={<ReferendumPage />} />
          <Route path="surveys" element={<PerpetualSurveysPage />} />
          <Route path="referendums" element={<ReferendumsPage />} />

          {/* Moderation */}
          <Route
            path="proposals/new"
            element={
              <CreateProposalProvider>
                <CreateDiscussionPage />
              </CreateProposalProvider>
            }
          />
          <Route path="publications/new" element={<ModeratorCreatePublicationPage />} />
          <Route path="publications/preview" element={<PreviewPage />} />
          <Route element={<ModerationLayout />}>
            // Proposals
            <Route path="proposals" element={<ProposalsPage />} />
            <Route path="proposals/:proposalId" element={<ProposalPage />} />
            // Moderators
            <Route path="moderators/proposals/:proposalId" element={<ModeratorProposalPage />} />
            <Route path="moderators/:tabKey?" element={<ModeratorsPage />} />
            // Publications
            <Route path="publications/proposals/:proposalId" element={<ModeratorPublicationPage />} />
            <Route path="publications/changed/:publicationId" element={<ModeratorChangedPublicationPage />} />
            <Route path="publications/unpublished/:publicationId" element={<UnpublishedPublicationPage />} />
            <Route path="publications/:tabKey?" element={<PublicationsPage />} />
            // Publishers
            <Route path="publishers/proposals/:proposalId" element={<PublisherProposalPage />} />
            <Route path="publishers/:tabKey?" element={<PublishersPage />} />
            <Route
              path="publishers/details/:publisherId"
              element={
                <PublishersLayout>
                  <ConstrainedWidthLayout>
                    <PublisherPage />
                  </ConstrainedWidthLayout>
                </PublishersLayout>
              }
            />
            // Reviews
            <Route path="reviews" element={<ReviewsPage />} />
            // Users
            <Route path="users" element={<UsersSectionLayout />}>
              <Route path=":tabKey?" element={<UsersPage />} />
              <Route
                path="details/:userId"
                element={
                  <ConstrainedWidthLayout>
                    <ReviewerPage />
                  </ConstrainedWidthLayout>
                }
              />
            </Route>
          </Route>

          {/* любой другой путь под сайтом */}
          <Route path="*" element={<ErrorPage />} />
        </Route>
      </Routes>
    )
  }

  if (rest) {
    return ERROR_PAGE
  }

  const matchedEntity = (Object.keys(ENTITY_ELEMENTS) as EntityParam[]).find(key =>
    appEntity.startsWith(ENTITY_PREFIXES[key]),
  )

  return matchedEntity ? <>{ENTITY_ELEMENTS[matchedEntity]}</> : ERROR_PAGE
}
