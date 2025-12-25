import { useEffect, useMemo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { SvgStarXxs } from "assets"
import { PublicationDetails, ProductFieldModel } from "types"
import { ButtonPrimary, DropdownSecondary, LinkFullscreen } from "ui/components"
import { formatAverageRating, formatDateDots } from "utils"

import { AuthorImageTitle } from "./components"

const LABEL_CLASSNAME = "leading-4 text-gray-500 text-2xs"
const VALUE_CLASSNAME = "overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800"

const nameEq = (name: unknown, expected: string) => String(name).toLowerCase() === expected

const getValue = (fields: ProductFieldModel[] | undefined, name: string) => {
  return fields?.find(x => nameEq(x.name, name))?.value as unknown
}

const getChildren = (fields: ProductFieldModel[] | undefined, name: string) => {
  return fields?.find(x => nameEq(x.name, name))?.children ?? []
}

const getChildrenAny = (fields: ProductFieldModel[] | undefined, names: string[]) => {
  for (const n of names) {
    const c = getChildren(fields, n)
    if (c.length) return c
  }
  return [] as ProductFieldModel[]
}

export type SoftwareInfoProps = {
  siteId: string
  publication: PublicationDetails
  publisherLabel: string
  versionLabel: string
  activationLabel: string
  osLabel: string
  ratingLabel: string
  lastUpdatedLabel: string
}

export const SoftwareInfo = ({
  siteId,
  publication,
  publisherLabel,
  versionLabel,
  activationLabel,
  osLabel,
  ratingLabel,
  lastUpdatedLabel,
}: SoftwareInfoProps) => {
  const fields = publication.productFields

  const versions = useMemo(() => {
    const releases = (fields ?? []).filter(x => nameEq(x.name, "release"))
    const collected: string[] = []
    for (const r of releases) {
      const v = getValue(r.children, "version")
      if (v) collected.push(String(v))
    }
    const unique = Array.from(new Set(collected)).filter(Boolean)
    return unique.length ? unique : ["2.16.4.1870"]
  }, [fields])

  const [selectedVersion, setSelectedVersion] = useState<string>(versions[0])

  useEffect(() => {
    if (!versions.length) return
    setSelectedVersion(prev => (versions.includes(prev) ? prev : versions[0]))
  }, [versions])

  const versionItems = useMemo(() => versions.map(v => ({ value: v, label: v })), [versions])

  const licenseType = useMemo(() => {
    const v = getValue(fields, "license")
    return v ? String(v) : "Freeware"
  }, [fields])

  const price = useMemo(() => {
    const v = getValue(fields, "price")
    return v ? String(v) : "$5"
  }, [fields])

  const languages = useMemo(() => {
    const uiLanguages = getChildrenAny(fields, ["uilanguages", "ui-languages", "ui_languages"])
    const codes = uiLanguages
      .filter(x => nameEq(x.name, "language"))
      .map(x => String(x.value).toUpperCase())
      .filter(Boolean)
    const unique = Array.from(new Set(codes))
    return unique.length ? unique.join(", ") : "EN"
  }, [fields])

  return (
    <div className="flex flex-col gap-6 rounded-lg border border-[#D7DDEB] bg-[#F3F5F8] p-6">
    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{publisherLabel}</span>
      <LinkFullscreen to={`/${siteId}/a/${publication.authorId}`}>
        <AuthorImageTitle title={publication.authorTitle} authorAvatar={publication.authorAvatar} />
      </LinkFullscreen>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{versionLabel}</span>
      <DropdownSecondary
        className="w-full"
        controlled={true}
        size="medium"
        items={versionItems}
        value={selectedVersion}
        onChange={x => setSelectedVersion(x.value)}
      />
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{activationLabel}</span>
      <span className={VALUE_CLASSNAME}>Free</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{osLabel}</span>
      <span className={VALUE_CLASSNAME}>Windows / MacOS / Linux</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>License Type</span>
      <span className={VALUE_CLASSNAME}>{licenseType}</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{ratingLabel}</span>
      <div className={twMerge(VALUE_CLASSNAME, "flex items-center gap-1")}>
        {formatAverageRating(publication.rating)} <SvgStarXxs className="fill-favorite" />
      </div>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>Price</span>
      <span className={VALUE_CLASSNAME}>{price}</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{lastUpdatedLabel}</span>
      <span className={VALUE_CLASSNAME}>{formatDateDots(publication.productUpdated)}</span>
    </div>

    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>Languages</span>
      <span className={VALUE_CLASSNAME}>{languages}</span>
    </div>

    <div className="flex flex-col gap-4">
      <ButtonPrimary className="w-full" label="Download from RDN" />
      <ButtonPrimary className="w-full" label="Download from torrent" />
      <ButtonPrimary className="w-full" label="Download from torrent" />
      <ButtonPrimary className="w-full" label="Download from web" />
    </div>
    </div>
  )
}
