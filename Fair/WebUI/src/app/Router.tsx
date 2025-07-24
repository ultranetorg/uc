import { createBrowserRouter, createHashRouter, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, SiteLayout } from "ui/layouts"
import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  ErrorPage,
  ModerationPage,
  ModeratorDiscussionPage,
  ModeratorDiscussionsPage,
  ModeratorPublicationPage,
  ModeratorReviewPage,
  ProfilePage,
  PublicationPage,
  ReferendumPage,
  ReferendumsPage,
  SearchPage,
  SitePage,
  SitesPage,
} from "ui/pages"

import { SiteProvider } from "./SiteContext"
import { AppRouteObject } from "./types"

const { VITE_APP_SERVERLESS_BUILD: SERVERLESS_BUILD } = import.meta.env

// eslint-disable-next-line react-refresh/only-export-components
export const routes: AppRouteObject[] = [
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
        breadcrumb: "home",
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
            breadcrumb: "about",
            element: <AboutPage />,
          },

          {
            path: "/:siteId/g",
            breadcrumb: "governance",
            children: [
              {
                index: true,
                element: <ReferendumsPage />,
              },
              {
                path: ":referendumId",
                breadcrumb: (t, p) => t("referendum:title", { ...p }),
                element: <ReferendumPage />,
              },
            ],
          },

          {
            path: "/:siteId/m",
            breadcrumb: "moderation",
            element: <ModerationPage />,
          },
          {
            path: "/:siteId/m-d",
            element: <ModeratorDiscussionsPage />,
          },
          {
            path: "/:siteId/m-d/:discussionId",
            element: <ModeratorDiscussionPage />,
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
        path: "/:siteId/a/:authorId",
        element: <AuthorPage />,
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
