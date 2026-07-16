import { createBrowserRouter, createHashRouter, Outlet, RouteObject, RouterProvider } from "react-router-dom"

import { BaseLayout } from "ui/layouts"
import { IndexPage } from "ui/pages"

import { AuthenticationProvider } from "./AuthenticationProvider"
import { EntityRoute } from "./EntityRoute"
import { RouteErrorBoundary } from "./RouteErrorBoundary"
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
              <Outlet />
            </SiteProvider>
          </UserProvider>
        </SignInProvider>
      </AuthenticationProvider>
    ),
    errorElement: <RouteErrorBoundary />,
    children: [
      {
        index: true,
        element: (
          <BaseLayout>
            <IndexPage />
          </BaseLayout>
        ),
      },
      {
        path: ":appEntity/*",
        element: (
          <SiteRolesProvider>
            <SitePoliciesProvider>
              <EntityRoute />
            </SitePoliciesProvider>
          </SiteRolesProvider>
        ),
        errorElement: <RouteErrorBoundary />,
      },
    ],
  },
]

const browserRouter = createBrowserRouter(routes)
const hashRouter = createHashRouter(routes, { basename: "/" })
const router = SERVERLESS_BUILD === "1" ? hashRouter : browserRouter
export const Router = () => <RouterProvider router={router} />
