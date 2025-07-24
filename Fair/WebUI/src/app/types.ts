import { TFunction } from "i18next"
import { Params, RouteObject } from "react-router-dom"

export type AppRouteObject = {
  breadcrumb?: string | ((t: TFunction, params?: Params<string>) => string)
  children?: AppRouteObject[]
} & RouteObject
