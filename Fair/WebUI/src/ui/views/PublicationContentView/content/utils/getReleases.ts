import { Distributive, FieldValue, Hardware, Hash, Release, Requirements, Software } from "types"
import { getValue, nameEq } from "utils"

const normalizePlatformName = (raw: string): string | undefined => {
  const value = raw.trim().toLocaleLowerCase()

  if (value.includes("windows")) return "windows"
  if (value.includes("mac")) return "macos"
  if (value.includes("linux")) return "linux"

  return undefined
}

const buildHardware = (fields: FieldValue[]): Hardware | undefined => {
  const cpu = getValue(fields, "cpu")
  const gpu = getValue(fields, "gpu")
  const npu = getValue(fields, "npu")
  const ram = getValue(fields, "ram")
  const hdd = getValue(fields, "hdd")
  if (!cpu && !gpu && !npu && !ram && !hdd) return undefined
  return { cpu, gpu, npu, ram, hdd }
}

const buildSoftware = (fields: FieldValue[]): Software | undefined => {
  const os = getValue(fields, "os")
  if (!os) return undefined
  return { os, architecture: getValue(fields, "architecture"), version: getValue(fields, "version") }
}

const getDistributive = (fields: FieldValue[]): Distributive | undefined => {
  const platform = getValue(fields, "platform")
  const date = getValue<number>(fields, "date")
  const distribution = getValue(fields, "distribution")
  const downloadNode = fields.find(x => nameEq(x.name, "download"))

  if (!platform || date === undefined || !distribution || !downloadNode) return undefined

  const downloadChildren = downloadNode.children ?? []
  const uri = getValue(downloadChildren, "uri")
  if (!uri) return undefined

  const hashNode = downloadChildren.find(x => nameEq(x.name, "hash"))
  const hash: Hash | undefined = hashNode
    ? { type: getValue(hashNode.children, "type") ?? "", value: getValue(hashNode.children, "value") ?? "" }
    : undefined

  return { platform, date, distribution, download: { uri, hash } }
}

const getRequirements = (fields: FieldValue[]): Requirements | undefined => {
  const firstPlatform = fields.find(x => nameEq(x.name, "platform"))
  if (!firstPlatform) return undefined

  const platform = firstPlatform.value as string
  if (!platform) return undefined
  const normalized = normalizePlatformName(platform)
  if (!normalized) return undefined

  const platformChildren = firstPlatform.children ?? []

  const minimalChildren = platformChildren.find(x => nameEq(x.name, "minimal"))?.children ?? []
  const minHardware = minimalChildren.find(x => nameEq(x.name, "hardware"))?.children ?? []
  const minSoftware = minimalChildren.find(x => nameEq(x.name, "software"))?.children ?? []

  const recommendedNode = platformChildren.find(x => nameEq(x.name, "recommended"))
  const recommendedChildren = recommendedNode?.children ?? []
  const recHardware = recommendedChildren.find(x => nameEq(x.name, "hardware"))?.children ?? []
  const recSoftware = recommendedChildren.find(x => nameEq(x.name, "software"))?.children ?? []

  const hardware = buildHardware(minHardware)
  const software = buildSoftware(minSoftware)
  if (!hardware && !software) return undefined

  const recommendedHardware = buildHardware(recHardware)
  const recommendedSoftware = buildSoftware(recSoftware)

  return {
    platform: {
      platform: normalized,
      minimal: { hardware: hardware, software: software },
      recommended:
        recommendedNode && (recommendedHardware || recommendedSoftware)
          ? { hardware: recommendedHardware, software: recommendedSoftware }
          : undefined,
    },
  }
}

export const getReleases = (fields: FieldValue[]): Release[] | undefined => {
  const releases = fields
    .filter(x => nameEq(x.name, "release"))
    .flatMap(x => {
      const children = x.children ?? []

      const version = getValue(children, "version")
      if (!version) return []

      let node = children.find(x => nameEq(x.name, "distributive"))
      if (!node) return []
      const distributive = getDistributive(node.children ?? [])
      if (!distributive) return []

      node = children.find(x => nameEq(x.name, "requirements"))
      if (!node) return []
      const requirements = getRequirements(node.children ?? [])
      if (!requirements) return []

      return [{ version, distributive, requirements }] satisfies Release[]
    })

  const uniqReleases = [...new Map(releases.map(x => [x.requirements.platform.platform, x])).values()]
  return uniqReleases.length > 0 ? (uniqReleases as Release[]) : undefined
}
