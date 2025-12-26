import { ProductFieldModel } from "types"

export const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

export const getValue = <TValue = string>(
  fields: ProductFieldModel[] | undefined,
  name: string,
): TValue | undefined => {
  return fields?.find(x => nameEq(x.name, name))?.value as TValue | undefined
}

export const getChildren = (fields: ProductFieldModel[] | undefined, name: string) => {
  return fields?.find(x => nameEq(x.name, name))?.children ?? []
}

export const getChildrenAny = (fields: ProductFieldModel[] | undefined, names: string[]) => {
  for (const n of names) {
    const c = getChildren(fields, n)
    if (c.length) return c
  }
  return [] as ProductFieldModel[]
}
