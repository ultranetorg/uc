import { Params, useParams as useRouterParams } from "react-router-dom"

import { ENTITY_PREFIXES, EntityParam, stripPrefix } from "utils"

export function useParams<ParamsOrKey extends string | Record<string, string | undefined> = string>(): Readonly<
  [ParamsOrKey] extends [string] ? Params<ParamsOrKey> : Partial<ParamsOrKey>
> {
  const params = useRouterParams<ParamsOrKey>()
  const result: Record<string, string | undefined> = { ...(params as Record<string, string | undefined>) }

  for (const key of Object.keys(ENTITY_PREFIXES) as EntityParam[]) {
    if (result[key] !== undefined) {
      result[key] = stripPrefix(key, result[key])
    }
  }

  for (const key of ["appEntity", "subEntity"]) {
    const entity = result[key]
    if (entity !== undefined) {
      const matched = (Object.keys(ENTITY_PREFIXES) as EntityParam[]).find(key =>
        entity.startsWith(ENTITY_PREFIXES[key]),
      )
      if (matched) {
        result[matched] = stripPrefix(matched, entity)
      }
    }
  }

  return result as Readonly<[ParamsOrKey] extends [string] ? Params<ParamsOrKey> : Partial<ParamsOrKey>>
}
