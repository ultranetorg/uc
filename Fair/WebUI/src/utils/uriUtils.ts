const parseMagnetLink = (value: string) => {
  if (!value.startsWith("magnet:?")) return undefined

  const params = new URLSearchParams(value.slice("magnet:?".length))
  const xt = params.get("xt")

  const HEX_HASH = /^urn:btih:[0-9a-fA-F]{40}$/i
  const BASE32_HASH = /^urn:btih:[A-Z2-7]{32}$/i

  if (!xt || (!HEX_HASH.test(xt) && !BASE32_HASH.test(xt))) return undefined

  return {
    hash: xt.split(":")[2],
    name: params.get("dn") ?? undefined,
    trackers: params.getAll("tr"),
  }
}

export const isMagnetUri = (value: string) => !!parseMagnetLink(value)

const CID_V0 = /^Qm[1-9A-HJ-NP-Za-km-z]{44}$/
const CID_V1 = /^(b[a-z2-7]{58,}|B[A-Z2-7]{58,}|z[1-9A-HJ-NP-Za-km-z]{48,}|f[0-9a-f]+)$/i

const isCID = (value: string) => CID_V0.test(value) || CID_V1.test(value)

export const isIpfsUri = (value: string) => {
  if (value.startsWith("ipfs://")) {
    const cid = value.slice(7).split("/")[0]
    return isCID(cid)
  }

  try {
    const url = new URL(value)
    const gatewayPath = url.pathname.match(/^\/ipfs\/([^/]+)/)
    if (gatewayPath) return isCID(gatewayPath[1])

    const subdomainCid = url.hostname.match(/^([^.]+)\.ipfs\./)
    if (subdomainCid) return isCID(subdomainCid[1])
  } catch {
    return false
  }

  return false
}

export const isRdnLink = (value: string) => value.startsWith("rdn://")
