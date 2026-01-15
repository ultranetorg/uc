import { ProductField } from "types"

export const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

export const getValue = <TValue = string>(fields: ProductField[] | undefined, name: string): TValue | undefined => {
  return fields?.find(x => nameEq(x.name, name))?.value as TValue | undefined
}
