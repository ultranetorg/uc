import { Params, RouteObject } from "react-router-dom"

export type AppRouteObject = {
  breadcrumb?: string | ((t: (key: string) => string, params?: Params<string>) => string)
  children?: AppRouteObject[]
} & RouteObject
