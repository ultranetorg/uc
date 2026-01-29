import { OperationType } from "types"

type GroupedOperations = {
  items: OperationType[]
}

export const groupOperations = (operations: OperationType[]): GroupedOperations[] => {
  const map = new Map<string, OperationType[]>()

  for (const op of operations) {
    const parts = op.split("-")

    const groupKey = parts[0]

    if (!map.has(groupKey)) {
      map.set(groupKey, [])
    }

    map.get(groupKey)!.push(op)
  }

  return Array.from(map.entries())
    .sort(([a], [b]) => {
      const aFirst = a.split("-")[0]
      const bFirst = b.split("-")[0]
      return aFirst.localeCompare(bFirst)
    })
    .map(([, items]) => ({
      items: items.sort(),
    }))
}
