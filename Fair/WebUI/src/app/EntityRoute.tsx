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
            <Route
              path="a/p/:publisherId"
              element={
                <PublishersLayout>
                  <ConstrainedWidthLayout>
                    <PublisherPage />
                  </ConstrainedWidthLayout>
                </PublishersLayout>
              }
            />
            <Route path="r" element={<ReviewsPage />} />
            <Route path="u" element={<UsersSectionLayout />}>
              <Route path=":tabKey?" element={<UsersPage />} />
              <Route
                path="u/:userId"
                element={
                  <ConstrainedWidthLayout>
                    <ReviewerPage />
                  </ConstrainedWidthLayout>
                }
              />
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
    return ERROR_PAGE
  }

  const matchedEntity = (Object.keys(ENTITY_ELEMENTS) as EntityParam[]).find(key =>
    appEntity.startsWith(ENTITY_PREFIXES[key]),
  )

  return matchedEntity ? <>{ENTITY_ELEMENTS[matchedEntity]}</> : ERROR_PAGE
}
