import { BrowserRouter, Route, Routes } from "react-router-dom"

import { BaseLayout } from "ui/layouts"
import { ErrorPage, IndexPage, ProductPage, ProductsPage } from "ui/pages"

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

        <Route path="/products">
          <Route index element={<ProductsPage />} />
          <Route path=":productId" element={<ProductPage />} />
        </Route>
      </Route>
    </Routes>
  </BrowserRouter>
)
