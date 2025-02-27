import { BrowserRouter, Route, Routes } from "react-router-dom"

import { BaseLayout, SiteLayout } from "ui/layouts"
import {
  AuthorPage,
  CategoryPage,
  ErrorPage,
  PublicationPage,
  PublicationsPage,
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
          <Route path="/:siteId/p">
            <Route index element={<PublicationsPage />} />
            <Route path=":publicationId" element={<PublicationPage />} />
          </Route>
        </Route>

        <Route path="/u" element={<UserPage />} />
      </Route>
    </Routes>
  </BrowserRouter>
)
