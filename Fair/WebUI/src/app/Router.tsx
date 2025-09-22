import { createBrowserRouter, createHashRouter, RouteObject, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, SiteLayout } from "ui/layouts"
import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  CreateDiscussionPage,
  CreateReferendumPage,
  DevelopPage,
  ErrorPage,
  ModerationPage,
  ModeratorDiscussionPage,
  ModeratorPublicationPage,
  ModeratorReviewPage,
  ModeratorUserRegistrationPage,
  ProfilePage,
  PublicationPage,
  ReferendumPage,
  ReferendumsPage,
  SearchPage,
  SitePage,
  SitesPage,
} from "ui/pages"

import { ModeratorProvider } from "./ModeratorContext"
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
            path: "/:siteId/g/new",
            element: (
              <ModeratorProvider>
                <CreateReferendumPage />
              </ModeratorProvider>
            ),
          },
          {
            path: "/:siteId/g",
            element: <ReferendumsPage />,
          },
          {
            path: "/:siteId/g/:referendumId",
            element: <ReferendumPage />,
          },

          {
            path: "/:siteId/m/new",
            element: (
              <ModeratorProvider>
                <CreateDiscussionPage />
              </ModeratorProvider>
            ),
          },
          {
            path: "/:siteId/m/:tabKey?",
            element: <ModerationPage />,
          },
          {
            path: "/:siteId/m/d/:discussionId",
            element: <ModeratorDiscussionPage />,
          },
          {
            path: "/:siteId/m/p/:discussionId",
            element: <ModeratorPublicationPage />,
          },
          {
            path: "/:siteId/m/r/:discussionId",
            element: <ModeratorReviewPage />,
          },
          {
            path: "/:siteId/m/u/:discussionId",
            element: <ModeratorUserRegistrationPage />,
          },

          ...(import.meta.env.DEV
            ? [
                {
                  path: "/:siteId/dev",
                  element: <DevelopPage />,
                },
              ]
            : []),
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
