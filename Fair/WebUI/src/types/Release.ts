import { DownloadSource } from "./DownloadSource"

export type CPUArchitecture = "x64" | "x86" | "arm64" | "arm" | string

export interface Hash {
  type: string
  value: string
}

export interface Source {
  uri: string
  source: DownloadSource
  hash?: Hash
}

export interface Distributive {
  platform: string
  date: number
  distribution: string
  sources: Source[]
}

export interface Hardware {
  cpu?: string
  gpu?: string
  npu?: string
  ram?: string
  hdd?: string
}

export interface Software {
  os: string
  architecture?: CPUArchitecture
  version?: string
}

export interface PlatformRequirements {
  platform: string
  minimal: {
    hardware?: Hardware
    software?: Software
  }
  recommended?: {
    hardware?: Hardware
    software?: Software
  }
}

export interface Requirements {
  platform: PlatformRequirements
}

export interface Release {
  version: string
  distributive: Distributive
  requirements: Requirements
}
