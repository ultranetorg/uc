import { createBrowserRouter, createHashRouter, RouteObject, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, SiteLayout } from "ui/layouts"
import {
  AboutPage,
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
  ProfilePage,
  PublicationPage,
  SearchPage,
  SitePage,
  SitesPage,
} from "ui/pages"

import { SiteProvider } from "./SiteContext"

const { VITE_APP_SERVERLESS_BUILD: SERVERLESS_BUILD } = import.meta.env

const routes: RouteObject[] = [
  {
    path: "/",
    element: <BaseLayout />,
    errorElement: (
      <AppLayout>
        <BaseLayout>
          <ErrorPage />
        </BaseLayout>
      </AppLayout>
    ),
    children: [
      {
        index: true,
        element: <SitesPage />,
      },

      {
        path: "/:siteId",
        element: (
          <SiteProvider>
            <SiteLayout />
          </SiteProvider>
        ),
        children: [
          {
            index: true,
            element: <SitePage />,
          },
          {
            path: "/:siteId/a/:authorId",
            element: <AuthorPage />,
          },
          {
            path: "/:siteId/c/:categoryId",
            element: <CategoryPage />,
          },
          {
            path: "/:siteId/p/:publicationId",
            element: <PublicationPage />,
          },
          {
            path: "/:siteId/s",
            element: <SearchPage />,
          },
          {
            path: "/:siteId/i",
            element: <AboutPage />,
          },

          {
            path: "/:siteId/a-r",
            element: <AuthorReferendumsPage />,
          },
          {
            path: "/:siteId/a-r/:referendumId",
            element: <AuthorReferendumPage />,
          },

          {
            path: "/:siteId/m",
            element: <ModerationPage />,
          },
          {
            path: "/:siteId/m-d",
            element: <ModeratorDisputesPage />,
          },
          {
            path: "/:siteId/m-d/:disputeId",
            element: <ModeratorDisputePage />,
          },
          {
            path: "/:siteId/m-p/:publicationId",
            element: <ModeratorPublicationPage />,
          },
          {
            path: "/:siteId/m-r/:reviewId",
            element: <ModeratorReviewPage />,
          },
        ],
      },

      {
        path: "/p/:address",
        element: <ProfilePage />,
      },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes, { basename: "/" })
const router = SERVERLESS_BUILD === "1" ? hashRouter : browserRouter
export const Router = () => <RouterProvider router={router} />
