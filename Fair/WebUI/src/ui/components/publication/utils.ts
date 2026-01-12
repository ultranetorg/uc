import { ProductField } from "types"

export const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

export const getValue = <TValue = string>(fields: ProductField[] | undefined, name: string): TValue | undefined => {
  return fields?.find(x => nameEq(x.name, name))?.value as TValue | undefined
}

export const getChildren = (fields: ProductField[] | undefined, name: string) => {
  return fields?.find(x => nameEq(x.name, name))?.children ?? []
}

export const getChildrenAny = (fields: ProductField[] | undefined, names: string[]) => {
  for (const n of names) {
    const c = getChildren(fields, n)
    if (c.length) return c
  }
  return [] as ProductField[]
}

export type RequirementPlatform = {
  key: string
  label: string
  node: ProductField
}

const normalizePlatformName = (raw: string): { key: string; label: string } => {
  const value = raw.trim()
  const lower = value.toLowerCase()

  if (lower.includes("windows")) {
    return { key: "windows", label: "Windows" }
  }
  if (lower.includes("mac")) {
    // Design expects macOS spelling with lowercase "m"
    return { key: "macos", label: "macOS" }
  }
  if (lower.includes("linux")) {
    return { key: "linux", label: "Linux" }
  }

  return { key: lower || value.toLowerCase(), label: value || raw }
}

export const getRequirementPlatforms = (fields: ProductField[] | undefined): RequirementPlatform[] => {
  const result: RequirementPlatform[] = []
  const seen = new Set<string>()
  const releases = (fields ?? []).filter(x => nameEq(x.name, "release"))

  for (const release of releases) {
    const children = release.children ?? []
    const requirements = children.find(c => nameEq(c.name, "requirements"))
    const platforms = requirements?.children ?? []

    for (const platform of platforms) {
      if (!nameEq(platform.name, "platform")) continue

      const raw = String(platform.value ?? "").trim()
      if (!raw) continue

      const normalized = normalizePlatformName(raw)
      if (seen.has(normalized.key)) continue

      seen.add(normalized.key)
      result.push({ key: normalized.key, label: normalized.label, node: platform })
    }
  }

  const order = ["windows", "macos", "linux"]

  return result.sort((a, b) => {
    const ia = order.indexOf(a.key)
    const ib = order.indexOf(b.key)

    if (ia === -1 && ib === -1) return a.label.localeCompare(b.label)
    if (ia === -1) return 1
    if (ib === -1) return -1
    return ia - ib
  })
}
