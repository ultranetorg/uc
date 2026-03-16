import { createBrowserRouter, createHashRouter, RouteObject, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, ModerationLayout, SiteLayout, UsersLayout } from "ui/layouts"
import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  CreateDiscussionPage,
  CreateReferendumPage,
  ProposalPage,
  ProposalsPage,
  ErrorPage,
  ModeratorCreatePublicationPage,
  ModeratorsPage,
  PerpetualSurveyPage,
  ProfilePage,
  PublicationPage,
  PublicationsPage,
  PublishersPage,
  ReferendumPage,
  ReferendumsPage,
  ReviewsPage,
  SearchPage,
  SitePage,
  SitesPage,
  UserPage,
  UsersPage,
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

          // Governance
          {
            path: "g/new",
            element: (
              <CreateProposalProvider>
                <CreateReferendumPage />
              </CreateProposalProvider>
            ),
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
            path: "g/:tabKey?",
            element: <ReferendumsPage />,
          },

          // Moderation
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
            path: "m",
            element: <ModerationLayout />,
            children: [
              {
                index: true,
                element: <ProposalsPage />,
              },
              {
                path: "p/:discussionId",
                element: <ProposalPage />,
              },
              {
                path: "m/:tabKey?",
                element: <ModeratorsPage />,
              },
              {
                path: "c/:tabKey?",
                element: <PublicationsPage />,
              },
              {
                path: "a/:tabKey?",
                element: <PublishersPage />,
              },
              {
                path: "r",
                element: <ReviewsPage />,
              },
              {
                path: "u",
                element: <UsersLayout />,
                children: [
                  {
                    index: true,
                    element: <UsersPage />,
                  },
                  {
                    path: ":name",
                    element: <UserPage />,
                  },
                ],
              },
            ],
          },
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
