export interface IPublicationDescription {
  language: string
  text: string
}

export interface IPublicationRelease {
  version: string
  distributive: {
    platform: string
    version: string
    date: string
    deployment: string
    download: {
      uri: string
      hash: {
        type: string
        value: string
      }
    }
  }
  requirements: {
    hardware: {
      cpu: string
      gpu: string
      npu: string
      ram: string
      hdd: string
    }
    software: {
      os: string
      architecture: string
      version: string
    }
  }
}

export interface IPublication {
  metadata: {
    version: string
  }
  title: string
  slogan?: string
  uri: string
  tags?: string
  logo: string
  license: string
  price?: number
  descriptionMin: IPublicationDescription[]
  descriptionMax: IPublicationDescription[]
  arts: {
    screenshot: {
      uri: string
      description: IPublicationDescription[]
    }
    video: {
      uri: string
      description: IPublicationDescription[]
    }
  }[]
  releases: IPublicationRelease[]
}
