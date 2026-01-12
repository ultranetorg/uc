import { ProductField, TokenType } from "types"
import { buildFileUrl, formatSecDate } from "utils"
import { IPublication, IPublicationDescription } from "./types"

const getData = <TData = string>(fields: ProductField[], token: TokenType): TData | undefined => {
  return fields.find(x => x.name === token)?.value as TData | undefined
}

const getFileUri = (fields: ProductField[], token: TokenType): string | undefined => {
  const id = getData<string>(fields, token)
  return id ? buildFileUrl(id) : undefined
}

const parseDescription = (fields: ProductField[], token: TokenType): IPublicationDescription[] =>
  fields
    .filter(field => field.name === token)
    .map(field => {
      const children = field.children ?? []
      const language = getData(children, "language")
      const text = getData(children, "value") ?? getData(children, "description")

      return language || text ? { language: language ?? "", text: text ?? "" } : undefined
    })
    .filter((x): x is IPublicationDescription => !!x)

const parseArts = (fields: ProductField[], token: TokenType): IPublication["arts"] =>
  fields
    .filter(field => field.name === token)
    .map(field => {
      const children = field.children ?? []

      const screenshotChildren = children.find(c => c.name === "screenshot")?.children ?? []
      const videoChildren = children.find(c => c.name === "video")?.children ?? []

      const screenshotUri = getFileUri(screenshotChildren, "id")
      const videoUri = getData<string>(videoChildren, "uri")

      return {
        screenshot: {
          uri: screenshotUri ?? "",
          description: parseDescription(screenshotChildren, "description"),
        },
        video: {
          uri: videoUri ?? "",
          description: parseDescription(videoChildren, "description"),
        },
      }
    })

const parseReleases = (fields: ProductField[], token: TokenType): IPublication["releases"] =>
  fields
    .filter(field => field.name === token)
    .map(field => {
      const children = field.children ?? []

      const distributiveChildren = children.find(c => c.name === "distributive")?.children ?? []
      const distributiveDownloadChildren = distributiveChildren.find(c => c.name === "download")?.children ?? []
      const distributiveDownloadHashChildren = distributiveDownloadChildren.find(c => c.name === "hash")?.children ?? []

      const requirementsChildren = children.find(c => c.name === "requirements")?.children ?? []
      const requirementsHardwareChildren = requirementsChildren.find(c => c.name === "hardware")?.children ?? []
      const requirementsSoftwareChildren = requirementsChildren.find(c => c.name === "software")?.children ?? []

      const dateVal = getData<number>(distributiveChildren, "date")

      return {
        version: getData(children, "version") ?? "",
        distributive: {
          platform: getData(distributiveChildren, "platform") ?? "",
          version: getData(distributiveChildren, "version") ?? "",
          date: dateVal ? formatSecDate(dateVal) : "",
          deployment: getData(distributiveChildren, "deployment") ?? "",
          download: {
            uri: getData(distributiveDownloadChildren, "uri") ?? "",
            hash: {
              type: getData(distributiveDownloadHashChildren, "type") ?? "",
              value: getData(distributiveDownloadHashChildren, "value") ?? "",
            },
          },
        },
        requirements: {
          hardware: {
            cpu: getData(requirementsHardwareChildren, "cpu") ?? "",
            gpu: getData(requirementsHardwareChildren, "gpu") ?? "",
            npu: getData(requirementsHardwareChildren, "npu") ?? "",
            ram: getData(requirementsHardwareChildren, "ram") ?? "",
            hdd: getData(requirementsHardwareChildren, "hdd") ?? "",
          },
          software: {
            os: getData(requirementsSoftwareChildren, "os") ?? "",
            architecture: getData(requirementsSoftwareChildren, "architecture") ?? "",
            version: getData(requirementsSoftwareChildren, "version") ?? "",
          },
        },
      }
    })

export function toPublicationData(fields: ProductField[]): IPublication {
  return {
    logo: getFileUri(fields, "logo") ?? "",
    title: getData<string>(fields, "title") ?? "",
    slogan: getData<string>(fields, "slogan") ?? "",
    uri: getData<string>(fields, "uri") ?? "",
    tags: getData<string>(fields, "tags") ?? "",
    license: getData<string>(fields, "license") ?? "",
    metadata: {
      version: getData<string>(fields.find(field => field.name === "metadata")?.children ?? [], "version") ?? "",
    },
    price: getData<number>(fields, "price"),
    descriptionMin: parseDescription(fields, "description-minimal"),
    descriptionMax: parseDescription(fields, "description-maximal"),
    arts: parseArts(fields, "art"),
    releases: parseReleases(fields, "release"),
  }
}
