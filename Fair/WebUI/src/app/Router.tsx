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

import { AuthenticationProvider } from "./AuthenticationProvider"
import { CreateProposalProvider } from "./CreateProposalProvider"
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
        <AuthenticationProvider>
          <UserProvider>
            <BaseLayout />
          </UserProvider>
        </AuthenticationProvider>
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
        path: ":siteId",
        element: (
          <SiteProvider>
            <ModerationProvider>
              <SiteLayout />
            </ModerationProvider>
          </SiteProvider>
        ),
        children: [
          {
            index: true,
            element: <SitePage />,
          },
          {
            path: "c/:categoryId",
            element: <CategoryPage />,
          },
          {
            path: "p/:publicationId",
            element: <PublicationPage />,
          },
          {
            path: "s",
            element: <SearchPage />,
          },
          {
            path: "i",
            element: <AboutPage />,
          },

          {
            path: "g/new",
            element: (
              <CreateProposalProvider>
                <CreateReferendumPage />
              </CreateProposalProvider>
            ),
          },
          {
            path: "g/:tabKey?",
            element: <ReferendumsPage />,
          },
          {
            path: "g/p/:perpetualSurveyId",
            element: <PerpetualSurveyPage />,
          },
          {
            path: "g/r/:referendumId",
            element: <ReferendumPage />,
          },

          {
            path: "m/new",
            element: (
              <CreateProposalProvider>
                <CreateDiscussionPage />
              </CreateProposalProvider>
            ),
          },
          {
            path: "m/new-publication",
            element: <ModeratorCreatePublicationPage />,
          },
          {
            path: "m/:tabKey?",
            element: <ModerationPage />,
          },
          {
            path: "m/d/:discussionId",
            element: <ModeratorDiscussionPage />,
          },
          {
            path: "m/c/:publicationId",
            element: <ModeratorChangedPublicationPage />,
          },
          {
            path: "m/n/:productId",
            element: <ModeratorUnpublishedProductPage />,
          },
          {
            path: "m/p/:discussionId",
            element: <ModeratorPublicationPage />,
          },
          {
            path: "m/u/:discussionId",
            element: <ModeratorUserRegistrationPage />,
          },

          ...(import.meta.env.DEV
            ? [
                {
                  path: "dev",
                  element: <DevelopPage />,
                },
              ]
            : []),
        ],
      },

      {
        path: ":siteId/a/:authorId",
        element: <AuthorPage />,
      },
      {
        path: "p/:address",
        element: <ProfilePage />,
      },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes, { basename: "/" })
const router = SERVERLESS_BUILD === "1" ? hashRouter : browserRouter
export const Router = () => <RouterProvider router={router} />
