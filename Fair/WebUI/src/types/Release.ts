import { DownloadSource } from "./DownloadSource"

export type CPUArchitecture = "x64" | "x86" | "arm64" | "arm" | string

export interface Source {
  uri: string
  source: DownloadSource
}

export interface Distributive {
  type: string
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
  name: string
  version: string
  date: number
  distributives: Distributive[]
  requirements: Requirements
}
