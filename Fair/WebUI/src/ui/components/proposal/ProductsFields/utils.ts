import { UseQueryResult } from "@tanstack/react-query"

import { ProductFieldCompare, ProductFieldModel, ProductFieldViewModel } from "types"

import { CompareStatus, ProductFieldCompareViewModel } from "./types"

function groupByName(list?: ProductFieldViewModel[]) {
  const map = new Map<string, ProductFieldViewModel[]>()
  ;(list ?? []).forEach(item => {
    const key = item.name
    const arr = map.get(key) ?? []
    arr.push(item)
    map.set(key, arr)
  })
  return map
}

function getGenerator() {
  let index = 0
  return (
    field: ProductFieldModel,
    ext: Partial<ProductFieldCompareViewModel>,
    parent?: ProductFieldCompareViewModel,
  ): ProductFieldCompareViewModel => ({
    ...field,
    id: `${field.name}_${++index}`,
    children: [],
    parent,
    ...ext,
  })
}

function mergeArrays(
  fromList?: ProductFieldViewModel[],
  toList?: ProductFieldViewModel[],
  parent?: ProductFieldCompareViewModel,
  generate = getGenerator(),
): ProductFieldCompareViewModel[] {
  const fromGroups = groupByName(fromList)
  const toGroups = groupByName(toList)

  const keys = new Set<string>([...fromGroups.keys(), ...toGroups.keys()])

  const result: ProductFieldCompareViewModel[] = []

  keys.forEach(key => {
    const fromArr = fromGroups.get(key) ?? []
    const toArr = toGroups.get(key) ?? []

    const maxLen = Math.max(fromArr.length, toArr.length)

    for (let i = 0; i < maxLen; i++) {
      const from = fromArr[i]
      const to = toArr[i]

      if (from && !to) {
        // removed
        const item = generate(
          from,
          {
            oldValue: from.value,
            isRemoved: true,
          },
          parent,
        )
        item.children = mergeArrays(from.children, [], item, generate)
        result.push(item)
        continue
      }

      if (!from && to) {
        // added
        const item = generate(
          to,
          {
            isAdded: true,
          },
          parent,
        )
        item.children = mergeArrays([], to.children, item, generate)
        result.push(item)
        continue
      }

      if (from && to) {
        // exists in both - compare
        const item = generate(
          to,
          {
            oldValue: from.value,
          },
          parent,
        )

        item.children = mergeArrays(from.children, to.children, item, generate)

        item.isChanged =
          from.value !== to.value ||
          from.type !== to.type ||
          item.children.some(c => c.isAdded || c.isRemoved || c.isChanged) ||
          undefined

        // prefer 'to' values for current representation
        result.push(item)
      }
    }
  })

  return result
}

export function mergeFields(
  response: UseQueryResult<ProductFieldCompare, Error>,
): UseQueryResult<ProductFieldCompareViewModel[], Error> {
  const { data } = response

  if (!data) {
    return response as unknown as UseQueryResult<ProductFieldCompareViewModel[], Error>
  }

  const merged = mergeArrays(mapItems(data.from), mapItems(data.to))
  return { ...response, data: merged } as unknown as UseQueryResult<ProductFieldCompareViewModel[], Error>
}

function mapItems(
  items: ProductFieldModel[],
  parent: ProductFieldViewModel | undefined = undefined,
  idCounter: number = 0,
): ProductFieldViewModel[] {
  return items.map(item => {
    const newItem: ProductFieldViewModel = { ...item, parent, children: undefined, id: `${item.name}_${++idCounter}` }
    if (item.children && item.children.length > 0) {
      newItem.children = mapItems(item.children, newItem, idCounter)
    }
    return newItem
  })
}

export function mapFields(
  response: UseQueryResult<ProductFieldModel[], Error>,
): UseQueryResult<ProductFieldViewModel[], Error> {
  const { data } = response

  if (!data) {
    return response as unknown as UseQueryResult<ProductFieldViewModel[], Error>
  }

  const transformed: ProductFieldViewModel[] = mapItems(data)

  return { ...response, data: transformed } as unknown as UseQueryResult<ProductFieldViewModel[], Error>
}

export const isCompareNode = (
  n?: ProductFieldViewModel | ProductFieldCompareViewModel | null,
): n is ProductFieldCompareViewModel => {
  return !!n && ("isRemoved" in n || "isAdded" in n || "isChanged" in n)
}

export const getCompareStatus = (node?: ProductFieldViewModel | ProductFieldCompareViewModel | null): CompareStatus => {
  if (!node) return null
  if (!isCompareNode(node)) return null
  // precedence: removed > added > changed
  if (node.isRemoved) return "removed"
  if (node.isAdded) return "added"
  if (node.isChanged) return "changed"
  return null
}
