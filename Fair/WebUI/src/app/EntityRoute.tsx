import { ReactNode } from "react"
import { Route, Routes } from "react-router-dom"

import { ApiError } from "api"
import { useBackgroundLocation, useParams } from "hooks"
import { BaseLayout, StoreLayout } from "ui/layouts"
import { ConstrainedWidthLayout, ModerationLayout, PublishersLayout, UsersSectionLayout } from "ui/layouts/moderation"
import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  PublicationPage,
  PublisherPage,
  ReviewerPage,
  SearchPage,
  StorePage,
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

const NotFoundRoute = (): ReactNode => {
  throw new ApiError(404, "Not Found")
}

const StoreEntityRoute = () => {
  const { userId, publisherId } = useParams()
  const backgroundLocation = useBackgroundLocation()

  // store123-4/user234-5
  if (userId !== undefined)
    return (
      <MaybeFullscreen showFullscreen={!!backgroundLocation}>
        <ReviewerPage showDefaultBreadcrumbs={!backgroundLocation} />
      </MaybeFullscreen>
    )

  // store123-4/publisher234-5
  if (publisherId !== undefined)
    return (
      <MaybeFullscreen showFullscreen={!!backgroundLocation}>
        <PublisherPage showDefaultBreadcrumbs={!backgroundLocation} />
      </MaybeFullscreen>
    )

  throw new ApiError(404, "Not Found")
}

const ENTITY_ELEMENTS: Partial<Record<EntityParam, ReactNode>> = {
  // fair.net/category123-4
  categoryId: (
    <BaseLayout>
      <StoreLayout>
        <CategoryPage />
      </StoreLayout>
    </BaseLayout>
  ),
  // fair.net/publication234-5
  publicationId: (
    <BaseLayout>
      <StoreLayout>
        <PublicationPage />
      </StoreLayout>
    </BaseLayout>
  ),
  // fair.net/author345-6
  authorId: (
    <FullscreenPageView>
      <AuthorPage />
    </FullscreenPageView>
  ),
  // TODO: Add current user Profile here
}

export const EntityRoute = () => {
  const { appEntity = "", "*": rest } = useParams()

  if (appEntity.startsWith(ENTITY_PREFIXES.storeId)) {
    return (
      <Routes>
        <Route path=":subEntity" element={<StoreEntityRoute />} />

        <Route
          element={
            <BaseLayout>
              <StoreLayout />
            </BaseLayout>
          }
        >
          <Route index element={<StorePage />} />
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

          <Route path="surveys" element={<PerpetualSurveysPage />} />
          <Route path="surveys/:perpetualSurveyId" element={<PerpetualSurveyPage />} />
          <Route path="referendums" element={<ReferendumsPage />} />
          <Route path="referendums/:referendumId" element={<ReferendumPage />} />

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
          <Route path="*" element={<NotFoundRoute />} />
        </Route>
      </Routes>
    )
  }

  if (rest) {
    throw new ApiError(404, "Not Found")
  }

  const matchedEntity = (Object.keys(ENTITY_ELEMENTS) as EntityParam[]).find(key =>
    appEntity.startsWith(ENTITY_PREFIXES[key]),
  )

  if (!matchedEntity) {
    throw new ApiError(404, "Not Found")
  }

  return <>{ENTITY_ELEMENTS[matchedEntity]}</>
}
