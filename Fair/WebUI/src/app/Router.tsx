import { createBrowserRouter, createHashRouter, Outlet, RouteObject, RouterProvider } from "react-router-dom"

import { BaseLayout } from "ui/layouts"
import { IndexPage } from "ui/pages"

import { AuthenticationProvider } from "./AuthenticationProvider"
import { EntityRoute } from "./EntityRoute"
import { RouteErrorBoundary } from "./RouteErrorBoundary"
import { SignInProvider } from "./SignInProvider"
import { StorePoliciesProvider } from "./StorePoliciesProvider"
import { StoreProvider } from "./StoreProvider"
import { StoreRolesProvider } from "./StoreRolesProvider"
import { UserProvider } from "./UserProvider"

const { VITE_APP_SERVERLESS_BUILD: SERVERLESS_BUILD } = import.meta.env

const routes: RouteObject[] = [
  {
    path: "/",
    element: (
      <AuthenticationProvider>
        <SignInProvider>
          <UserProvider>
            <StoreProvider>
              <Outlet />
            </StoreProvider>
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
          <StoreRolesProvider>
            <StorePoliciesProvider>
              <EntityRoute />
            </StorePoliciesProvider>
          </StoreRolesProvider>
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
