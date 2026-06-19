import { createBrowserRouter, createHashRouter, RouteObject, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, ModerationLayout, SiteLayout, UsersLayout } from "ui/layouts"
import {
  AboutPage,
  AuthorPage,
  CategoryPage,
  ErrorPage,
  ProfilePage,
  PublicationPage,
  SearchPage,
  SitePage,
  SitesPage,
  UserPage,
} from "ui/pages"
import {
  CreateReferendumPage,
  PerpetualSurveyPage,
  PerpetualSurveysPage,
  ReferendumPage,
  ReferendumsPage,
} from "ui/pages/governance"
import {
  CreateDiscussionPage,
  ModeratorChangedPublicationPage,
  ModeratorCreatePublicationPage,
  ModeratorProposalPage,
  ModeratorPublicationPage,
  ModeratorsPage,
  PreviewPage,
  ProposalPage,
  ProposalsPage,
  PublicationsPage,
  PublisherProposalPage,
  PublisherPage,
  PublishersPage,
  ReviewsPage,
  UnpublishedPublicationPage,
  UsersPage as ModerationUsersPage,
} from "ui/pages/moderation"
import { CreateProposalProvider } from "ui/views"

import { AuthenticationProvider } from "./AuthenticationProvider"
import { SignInProvider } from "./SignInProvider"
import { SiteProvider } from "./SiteProvider"
import { SitePoliciesProvider } from "./SitePoliciesProvider"
import { SiteRolesProvider } from "./SiteRolesProvider"
import { UserProvider } from "./UserProvider"

const { VITE_APP_SERVERLESS_BUILD: SERVERLESS_BUILD } = import.meta.env

const routes: RouteObject[] = [
  {
    path: "/",
    element: (
      <AuthenticationProvider>
        <SignInProvider>
          <UserProvider>
            <SiteProvider>
              <BaseLayout />
            </SiteProvider>
          </UserProvider>
        </SignInProvider>
      </AuthenticationProvider>
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
          <SiteRolesProvider>
            <SitePoliciesProvider>
              <SiteLayout />
            </SitePoliciesProvider>
          </SiteRolesProvider>
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
            path: "u/:userId",
            element: <UserPage isFromModeration={false} />,
          },
          {
            path: "e/:publisherId",
            element: <PublisherPage isFromModeration={false} />,
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
            path: "g/p",
            element: <PerpetualSurveysPage />,
          },
          {
            path: "g/r",
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
                path: "m/p/:proposalId",
                element: <ModeratorProposalPage />,
              },
              {
                path: "m/:tabKey?",
                element: <ModeratorsPage />,
              },
              {
                path: "c/p/:proposalId",
                element: <ModeratorPublicationPage />,
              },
              {
                path: "c/c/:publicationId",
                element: <ModeratorChangedPublicationPage />,
              },
              {
                path: "c/u/:publicationId",
                element: <UnpublishedPublicationPage />,
              },
              {
                path: "c/:tabKey?",
                element: <PublicationsPage />,
              },
              {
                path: "a/r/:proposalId",
                element: <PublisherProposalPage />,
              },
              {
                path: "a/:tabKey?",
                element: <PublishersPage />,
              },
              {
                path: "a/p/:publisherId",
                element: <PublisherPage />,
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
                    path: ":tabKey?",
                    element: <ModerationUsersPage />,
                  },
                  {
                    path: "u/:userId",
                    element: <UserPage />,
                  },
                ],
              },
              {
                path: "v",
                element: <PreviewPage />,
              },
            ],
          },
        ],
      },

      {
        path: ":siteId/a/:authorId",
        element: <AuthorPage />,
      },
      // {
      //   path: "p/:address",
      //   element: <ProfilePage />,
      // },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes, { basename: "/" })
const router = SERVERLESS_BUILD === "1" ? hashRouter : browserRouter
export const Router = () => <RouterProvider router={router} />
