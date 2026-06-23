import { createBrowserRouter, createHashRouter, RouteObject, RouterProvider } from "react-router-dom"

import { AppLayout, BaseLayout, SiteLayout } from "ui/layouts"
import { ErrorPage, SitesPage } from "ui/pages"

import { AuthenticationProvider } from "./AuthenticationProvider"
import { EntityRoute } from "./EntityRoute"
import { SignInProvider } from "./SignInProvider"
import { SitePoliciesProvider } from "./SitePoliciesProvider"
import { SiteProvider } from "./SiteProvider"
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
        path: ":appEntity/*",
        element: (
          <SiteRolesProvider>
            <SitePoliciesProvider>
              <SiteLayout>
                <EntityRoute />
              </SiteLayout>
            </SitePoliciesProvider>
          </SiteRolesProvider>
        ),
      },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes, { basename: "/" })
const router = SERVERLESS_BUILD === "1" ? hashRouter : browserRouter
export const Router = () => <RouterProvider router={router} />
