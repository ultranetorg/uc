import { UseQueryResult } from "@tanstack/react-query"
import {
  ProductFieldCompareModel,
  ProductFieldCompareViewModel,
  ProductFieldModel,
  ProductFieldViewModel,
  TotalItemsResult,
} from "types"

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

function getGenerator(parent: ProductFieldViewModel | undefined = undefined) {
  let index = 0
  return (field: ProductFieldModel, ext: Partial<ProductFieldCompareViewModel>): ProductFieldCompareViewModel => ({
    ...field,
    id: `${field.name}_${++index}`,
    children: [],
    parent,
    ...ext,
  })
}

function mergeArrays(fromList?: ProductFieldViewModel[], toList?: ProductFieldViewModel[], parent: ProductFieldViewModel | undefined = undefined): ProductFieldCompareViewModel[] {
  const fromGroups = groupByName(fromList)
  const toGroups = groupByName(toList)

  const keys = new Set<string>([...fromGroups.keys(), ...toGroups.keys()])

  const result: ProductFieldCompareViewModel[] = []
  const generate = getGenerator(parent)

  keys.forEach(key => {
    const fromArr = fromGroups.get(key) ?? []
    const toArr = toGroups.get(key) ?? []

    const maxLen = Math.max(fromArr.length, toArr.length)

    for (let i = 0; i < maxLen; i++) {
      const from = fromArr[i]
      const to = toArr[i]

      if (from && !to) {
        // removed
        result.push(
          generate(from, {
            children: mergeArrays(from.children, [], from),
            oldValue: from.value,
            isRemoved: true,
          }),
        )
        continue
      }

      if (!from && to) {
        // added
        result.push(
          generate(to, {
            children: mergeArrays([], to.children, to),
            isAdded: true,
          }),
        )
        continue
      }

      if (from && to) {
        // exists in both - compare
        const children = mergeArrays(from.children, to.children, to)

        const isChanged =
          from.value !== to.value ||
          from.type !== to.type ||
          children.some(c => c.isAdded || c.isRemoved || c.isChanged)

        // prefer 'to' values for current representation
        result.push(generate(to, {
          children,
          oldValue: from.value,
          isChanged: isChanged || undefined,
        }))
      }
    }
  })

  return result
}

export function mergeFields(
  response: UseQueryResult<ProductFieldCompareModel, Error>,
): UseQueryResult<TotalItemsResult<ProductFieldCompareViewModel>, Error> {
  const { data } = response

  if (!data) {
    return response as unknown as UseQueryResult<TotalItemsResult<ProductFieldCompareViewModel>, Error>
  }

  const merged = mergeArrays(mapItems(data.from), mapItems(data.to))

  const transformed: TotalItemsResult<ProductFieldCompareViewModel> = {
    totalItems: merged.length,
    items: merged,
    page: 1,
    pageSize: merged.length,
  }

  return { ...response, data: transformed } as unknown as UseQueryResult<
    TotalItemsResult<ProductFieldCompareViewModel>,
    Error
  >
}

// index по значению.
let _idCounter = 0
function nextId(name: string) {
  return `${name}_${++_idCounter}`
}

function mapItems(
  items: ProductFieldModel[],
  parent: ProductFieldViewModel | undefined = undefined,
): ProductFieldViewModel[] {
  return items.map(item => {
    const newItem: ProductFieldViewModel = { ...item, parent, children: undefined, id: nextId(item.name) }
    if (item.children && item.children.length > 0) {
      newItem.children = mapItems(item.children, newItem)
    }
    return newItem
  })
}

export function mapFields(
  response: UseQueryResult<TotalItemsResult<ProductFieldModel>, Error>,
): UseQueryResult<TotalItemsResult<ProductFieldViewModel>, Error> {
  const { data } = response

  if (!data) {
    return response as unknown as UseQueryResult<TotalItemsResult<ProductFieldViewModel>, Error>
  }

  // сбросим счётчик id перед маппингом, чтобы id были детерминированы для
  // одного вызова mapFields
  _idCounter = 0

  const transformed: TotalItemsResult<ProductFieldViewModel> = {
    ...data,
    items: mapItems(data.items),
  }

  return { ...response, data: transformed } as unknown as UseQueryResult<TotalItemsResult<ProductFieldViewModel>, Error>
}
