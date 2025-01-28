import { BrowserRouter, Route, Routes } from "react-router-dom"

import { BaseLayout } from "ui/layouts"
import { AuthorPage, CategoryPage, ErrorPage, IndexPage, PublicationPage, PublicationsPage } from "ui/pages"

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
        <Route index element={<IndexPage />} />

        <Route path="/a/:authorId" element={<AuthorPage />} />

        <Route path="/c/:categoryId" element={<CategoryPage />} />

        <Route path="/p">
          <Route index element={<PublicationsPage />} />
          <Route path=":publicationId" element={<PublicationPage />} />
        </Route>
      </Route>
    </Routes>
  </BrowserRouter>
)
