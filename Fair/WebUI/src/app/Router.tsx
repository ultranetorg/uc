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
  ModeratorChangedPublicationPage,
  ModeratorCreatePublicationPage,
  ModeratorDiscussionPage,
  ModeratorPublicationPage,
  ModeratorUnpublishedProductPage,
  ModeratorUserRegistrationPage,
  PerpetualSurveyPage,
  ProfilePage,
  PublicationPage,
  ReferendumPage,
  ReferendumsPage,
  SearchPage,
  SitePage,
  SitesPage,
} from "ui/pages"

import { ManageUsersProvider } from "./ManageUsersProvider"
import { ModerationProvider } from "./ModerationProvider"
import { NodeCheckerProvider } from "./NodeCheckerProvider"
import { SiteProvider } from "./SiteProvider"
import { UserProvider } from "./UserProvider"

const { VITE_APP_SERVERLESS_BUILD: SERVERLESS_BUILD } = import.meta.env

const routes: RouteObject[] = [
  {
    path: "/",
    element: (
      <NodeCheckerProvider>
        <ManageUsersProvider>
          <SiteProvider>
            <UserProvider>
              <BaseLayout />
            </UserProvider>
          </SiteProvider>
        </ManageUsersProvider>
      </NodeCheckerProvider>
    ),
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
        element: <SiteLayout />,
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
              <ModerationProvider>
                <CreateReferendumPage />
              </ModerationProvider>
            ),
          },
          {
            path: "/:siteId/g/:tabKey?",
            element: <ReferendumsPage />,
          },
          {
            path: "/:siteId/g/p/:perpetualSurveyId",
            element: <PerpetualSurveyPage />,
          },
          {
            path: "/:siteId/g/r/:referendumId",
            element: <ReferendumPage />,
          },

          {
            path: "/:siteId/m/new",
            element: (
              <ModerationProvider>
                <CreateDiscussionPage />
              </ModerationProvider>
            ),
          },
          {
            path: "/:siteId/m/new-publication",
            element: <ModeratorCreatePublicationPage />,
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
            path: "/:siteId/m/c/:publicationId",
            element: <ModeratorChangedPublicationPage />,
          },
          {
            path: "/:siteId/m/n/:productId",
            element: <ModeratorUnpublishedProductPage />,
          },
          {
            path: "/:siteId/m/p/:discussionId",
            element: <ModeratorPublicationPage />,
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
