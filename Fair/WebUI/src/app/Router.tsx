import { createBrowserRouter, createHashRouter, RouterProvider } from "react-router-dom"

import { Layout, PageLayout, SearchLayout } from "ui/layouts"
import {
  AccountPage,
  AuctionPage,
  AuctionsPage,
  AuthorPage,
  DashboardPage,
  ErrorPage,
  OperationPage,
  ResourcePage,
  RoundPage,
  TermsPage,
  TransactionPage,
} from "ui/pages"

const { VITE_APP_ULTRANET_BUILD } = import.meta.env

const routes = [
  {
    path: "/",
    element: <Layout />,
    errorElement: (
      <Layout>
        <ErrorPage />
      </Layout>
    ),
    children: [
      {
        element: <SearchLayout />,
        children: [
          {
            index: true,
            element: <DashboardPage />,
            handle: {
              pageId: "dashboard",
            },
          },
          {
            element: <PageLayout />,
            children: [
              {
                path: "authors/:authorId",
                element: <AuthorPage />,
                handle: {
                  pageId: "author",
                },
              },
              {
                path: "accounts/:accountId",
                element: <AccountPage />,
                handle: {
                  pageId: "account",
                },
              },
              {
                path: "authors/:author/resources/*",
                element: <ResourcePage />,
                handle: {
                  pageId: "resource",
                },
              },
              {
                path: "rounds/:roundId",
                element: <RoundPage />,
                handle: {
                  pageId: "round",
                },
              },
              {
                path: "transactions/:transactionId",
                element: <TransactionPage />,
                handle: {
                  pageId: "transaction",
                },
              },
              {
                path: "operations/:operationId",
                element: <OperationPage />,
                handle: {
                  pageId: "operation",
                },
              },
            ],
          },
        ],
      },
      {
        path: "terms",
        element: <TermsPage />,
        handle: { pageId: "terms", hideSearch: true },
      },
      {
        path: "error-page",
        element: <ErrorPage />,
        handle: { pageId: "error-page" },
      },
    ],
  },
  {
    path: "auctions",
    element: (
      <Layout>
        <SearchLayout />
      </Layout>
    ),
    errorElement: (
      <Layout>
        <ErrorPage />
      </Layout>
    ),
    handle: { isAuctionsPath: true },
    children: [
      {
        index: true,
        element: <AuctionsPage />,
        handle: {
          pageId: "auctions",
        },
      },
      {
        element: <PageLayout />,
        children: [
          {
            path: ":auctionId",
            element: <AuctionPage />,
            handle: {
              pageId: "auction",
            },
          },
        ],
      },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes)
const router = !!VITE_APP_ULTRANET_BUILD ? hashRouter : browserRouter

export const Router = () => <RouterProvider router={router} />
