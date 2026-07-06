import { Distributive, DownloadSource, FieldValue, Hardware, Release, Requirements, Software, Source } from "types"
import { getValue, isIpfsUri, isMagnetUri, isRdnLink, isWebUri, nameEq } from "utils"

import { normalizePlatformName } from "./utils"

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

const getDownloadSourceByLink = (link: string): DownloadSource | undefined => {
  if (isMagnetUri(link)) return "torrent"
  if (isIpfsUri(link)) return "ipfs"
  if (isRdnLink(link)) return "rdn"
  if (isWebUri(link)) return "web"

  return undefined
}

const getDistributives = (nodes: FieldValue[]): Distributive[] => {
  const sourcesByType = new Map<string, Source[]>()

  for (const node of nodes) {
    const type = getValue(node.children, "type")
    if (!type) continue

    const sources = (node.children ?? [])
      .filter(x => nameEq(x.name, "source") && !!x.value)
      .flatMap<Source>(x => {
        const source = getDownloadSourceByLink(x.value as string)
        return source ? [{ uri: x.value as string, source }] : []
      })
    if (!sources.length) continue

    sourcesByType.set(type, [...(sourcesByType.get(type) ?? []), ...sources])
  }

  return [...sourcesByType.entries()].map(([type, sources]) => ({ type, sources }))
}

const getRequirements = (fields: FieldValue[], platform: string): Requirements => {
  const platformNode = fields.find(x => nameEq(x.name, "platform"))
  const platformChildren = platformNode?.children ?? []

  const minimalChildren = platformChildren.find(x => nameEq(x.name, "minimal"))?.children ?? []
  const minHardware = minimalChildren.find(x => nameEq(x.name, "hardware"))?.children ?? []
  const minSoftware = minimalChildren.find(x => nameEq(x.name, "software"))?.children ?? []

  const recommendedNode = platformChildren.find(x => nameEq(x.name, "recommended"))
  const recommendedChildren = recommendedNode?.children ?? []
  const recHardware = recommendedChildren.find(x => nameEq(x.name, "hardware"))?.children ?? []
  const recSoftware = recommendedChildren.find(x => nameEq(x.name, "software"))?.children ?? []

  const hardware = buildHardware(minHardware)
  const software = buildSoftware(minSoftware)

  const recommendedHardware = buildHardware(recHardware)
  const recommendedSoftware = buildSoftware(recSoftware)

  return {
    platform: {
      platform,
      minimal: { hardware, software },
      recommended:
        recommendedHardware || recommendedSoftware
          ? { hardware: recommendedHardware, software: recommendedSoftware }
          : undefined,
    },
  }
}

export const getReleases = (fields: FieldValue[]): Release[] | undefined =>
  fields
    .filter(x => nameEq(x.name, "release"))
    .flatMap(x => {
      const name = x.value as string
      if (!name) return []

      const children = x.children ?? []

      const version = getValue(children, "version")
      if (!version) return []

      const date = getValue<number>(children, "date")
      if (date === undefined) return []

      const distributiveNodes = children.filter(x => nameEq(x.name, "distributive"))
      const distributives = getDistributives(distributiveNodes)
      if (!distributives.length) return []

      const requirementsNode = children.find(x => nameEq(x.name, "requirements"))
      const requirements = getRequirements(requirementsNode?.children ?? [], normalizePlatformName(name))

      return [{ name, version, date, distributives, requirements }] satisfies Release[]
    })
