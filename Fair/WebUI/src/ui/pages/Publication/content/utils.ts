import { ProductFieldModel } from "types"
import { buildFileUrl, ensureHttp } from "utils"

export const getValue = <TValue = string>(
  fields: ProductFieldModel[] | undefined,
  name: ProductFieldModel["name"],
) => {
  return fields?.find(x => x.name === name)?.value as TValue | undefined
}

export function buildDescriptions(fields: ProductFieldModel[] | undefined) {
  const items = (fields ?? [])
    .filter(x => x.name === "description-maximal")
    .map(x => {
      const children = x.children ?? []
      const language = getValue(children, "language")
      const text = getValue(children, "value")
      return language || text ? { language: (language as string) ?? "", text: (text as string) ?? "" } : undefined
    })
    .filter((x): x is { language: string; text: string } => !!x)

  return items
}

export function buildMediaItems(fields: ProductFieldModel[] | undefined) {
  const arts = (fields ?? []).filter(x => x.name === "art")
  const items: { src: string; poster?: string; alt?: string }[] = []

  for (const art of arts) {
    const children = art.children ?? []
    const screenshotChildren = children.find(c => c.name === "screenshot")?.children ?? []
    const videoChildren = children.find(c => c.name === "video")?.children ?? []

    const screenshotId = getValue<string>(screenshotChildren, "id")
    const screenshotUri = screenshotId ? buildFileUrl(screenshotId) : undefined
    const videoUri = getValue<string>(videoChildren, "uri")

    if (screenshotUri) items.push({ src: screenshotUri, alt: "screenshot" })
    if (videoUri) items.push({ src: ensureHttp(videoUri), poster: screenshotUri, alt: "video" })
  }

  return items
}

export function buildRequirementsTabs(fields: ProductFieldModel[] | undefined) {
  const releases = (fields ?? []).filter(x => x.name === "release")
  const tabMap = new Map<string, { key: string; label: string; sections: { key: string; name: string; values: Record<string, string> }[] }>()

  for (const rel of releases) {
    const relChildren = rel.children ?? []
    const reqChildren = relChildren.find(c => c.name === "requirements")?.children ?? []

    for (const platformNode of reqChildren.filter(x => x.name === "platform")) {
      const platformChildren = platformNode.children ?? []
      const platform = (((platformNode.value as string) ?? "unknown") as string).toLowerCase()

      const minimalChildren = platformChildren.find(c => c.name === "minimal")?.children ?? []
      const recommendedChildren = platformChildren.find(c => c.name === "recommended")?.children ?? []

      const buildSection = (key: string, name: string, root: ProductFieldModel[]) => {
        const hardware = root.find(c => c.name === "hardware")?.children ?? []
        const software = root.find(c => c.name === "software")?.children ?? []

        const values: Record<string, string> = {}
        const cpu = getValue<string>(hardware, "cpu")
        const gpu = getValue<string>(hardware, "gpu")
        const npu = getValue<string>(hardware, "npu")
        const ram = getValue<string>(hardware, "ram")
        const hdd = getValue<string>(hardware, "hdd")
        const os = getValue<string>(software, "os")
        const arch = getValue<string>(software, "architecture")
        const ver = getValue<string>(software, "version")

        if (cpu) values.CPU = cpu
        if (gpu) values.GPU = gpu
        if (npu) values.NPU = npu
        if (ram) values.RAM = ram
        if (hdd) values.HDD = hdd
        if (os) values.OS = os
        if (arch) values.Architecture = arch
        if (ver) values.Version = ver

        return Object.keys(values).length > 0 ? { key, name, values } : undefined
      }

      const minimal = buildSection("minimal", "Minimum", minimalChildren)
      const recommended = buildSection("recommended", "Recommended", recommendedChildren)

      if (!minimal && !recommended) continue

      const existing = tabMap.get(platform) ?? { key: platform, label: platform.toUpperCase(), sections: [] }
      if (minimal && !existing.sections.some(s => s.key === "minimal")) existing.sections.push(minimal)
      if (recommended && !existing.sections.some(s => s.key === "recommended")) existing.sections.push(recommended)
      tabMap.set(platform, existing)
    }
  }

  return [...tabMap.values()]
}
