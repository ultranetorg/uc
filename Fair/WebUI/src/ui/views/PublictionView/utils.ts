import { ProductFieldViewModel, TokenType } from "types"
import { formatSecDate } from "utils"
import { IPublication, IPublicationDescription } from "./types"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getData = <TData = string>(fields: ProductFieldViewModel[], token: TokenType): TData | undefined => {
  return fields.find(x => x.name === token)?.value as TData | undefined
}

const getFileUri = (fields: ProductFieldViewModel[], token: TokenType) => `${BASE_URL}/files/${getData(fields, token)}`

const parseDescription = (fields: ProductFieldViewModel[], token: TokenType): IPublicationDescription[] =>
  fields
    .filter(field => field.name === token)
    .map(field => {
      const language = getData(field.children!, "language")!
      const text = getData(field.children!, "value") ?? getData(field.children!, "description")!

      return { language, text } satisfies IPublicationDescription
    })

const parseArts = (fields: ProductFieldViewModel[], token: TokenType): IPublication["arts"] =>
  fields
    .filter(field => field.name === token)
    .map(field => {
      const children = field.children ?? []

      const screenshotChildren = children.find(c => c.name === "screenshot")?.children ?? []
      const videoChildren = children.find(c => c.name === "video")?.children ?? []

      return {
        screenshot: {
          uri: getFileUri(screenshotChildren, "id")!,
          description: parseDescription(screenshotChildren, "description"),
        },
        video: {
          uri: getData(videoChildren, "uri")!,
          description: parseDescription(videoChildren, "description"),
        },
      }
    })

const parseReleases = (fields: ProductFieldViewModel[], token: TokenType): IPublication["releases"] =>
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

      return {
        version: getData(children, "version")!,
        distributive: {
          platform: getData(distributiveChildren, "platform")!,
          version: getData(distributiveChildren, "version")!,
          date: formatSecDate(getData<number>(distributiveChildren, "date")!),
          deployment: getData(distributiveChildren, "deployment")!,
          download: {
            uri: getData(distributiveDownloadChildren, "uri")!,
            hash: {
              type: getData(distributiveDownloadHashChildren, "type")!,
              value: getData(distributiveDownloadHashChildren, "value")!,
            },
          },
        },
        requirements: {
          hardware: {
            cpu: getData(requirementsHardwareChildren, "cpu")!,
            gpu: getData(requirementsHardwareChildren, "gpu")!,
            npu: getData(requirementsHardwareChildren, "npu")!,
            ram: getData(requirementsHardwareChildren, "ram")!,
            hdd: getData(requirementsHardwareChildren, "hdd")!,
          },
          software: {
            os: getData(requirementsSoftwareChildren, "os")!,
            architecture: getData(requirementsSoftwareChildren, "architecture")!,
            version: getData(requirementsSoftwareChildren, "version")!,
          },
        },
      }
    })

export function toPublicationData(fields: ProductFieldViewModel[]): IPublication {
  return {
    logo: getFileUri(fields, "logo"),
    title: getData<string>(fields, "title")!,
    slogan: getData<string>(fields, "slogan"),
    uri: getData<string>(fields, "uri")!,
    tags: getData<string>(fields, "tags"),
    license: getData<string>(fields, "license")!,
    metadata: {
      version: getData<string>(fields.find(field => field.name === "metadata")?.children ?? [], "version") ?? "",
    },
    price: getData<number>(fields, "price") ?? 0,
    descriptionMin: parseDescription(fields, "description-minimal") ?? [],
    descriptionMax: parseDescription(fields, "description-maximal") ?? [],
    arts: parseArts(fields, "art") ?? [],
    releases: parseReleases(fields, "release") ?? [],
  }
}
