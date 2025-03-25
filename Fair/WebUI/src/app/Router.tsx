import { BrowserRouter, Route, Routes } from "react-router-dom"

import { BaseLayout, SiteLayout } from "ui/layouts"
import {
  AuthorPage,
  AuthorReferendumPage,
  AuthorReferendumsPage,
  CategoryPage,
  ErrorPage,
  ModerationPage,
  ModeratorDisputePage,
  ModeratorDisputesPage,
  ModeratorPublicationPage,
  ModeratorReviewPage,
  PublicationPage,
  SearchPage,
  SitePage,
  SitesPage,
  UserPage,
} from "ui/pages"

export const Router = () => (
  <BrowserRouter>
    <Routes>
      <Route
        path="/"
        element={<BaseLayout />}
        errorElement={
          <BaseLayout>
            <ErrorPage />
          </BaseLayout>
        }
      >
        <Route index element={<SitesPage />} />

        <Route path="/:siteId" element={<SiteLayout />}>
          <Route index element={<SitePage />} />
          <Route path="/:siteId/a/:authorId" element={<AuthorPage />} />
          <Route path="/:siteId/c/:categoryId" element={<CategoryPage />} />
          <Route path="/:siteId/p/:publicationId" element={<PublicationPage />} />
          <Route path="/:siteId/s" element={<SearchPage />} />

          <Route path="/:siteId/a-r" element={<AuthorReferendumsPage />} />
          <Route path="/:siteId/a-r/:referendumId" element={<AuthorReferendumPage />} />

          <Route path="/:siteId/m" element={<ModerationPage />} />
          <Route path="/:siteId/m-d" element={<ModeratorDisputesPage />} />
          <Route path="/:siteId/m-d/:disputeId" element={<ModeratorDisputePage />} />
          <Route path="/:siteId/m-p/:publicationId" element={<ModeratorPublicationPage />} />
          <Route path="/:siteId/m-r/:reviewId" element={<ModeratorReviewPage />} />
        </Route>

        <Route path="/u" element={<UserPage />} />
      </Route>
    </Routes>
  </BrowserRouter>
)
