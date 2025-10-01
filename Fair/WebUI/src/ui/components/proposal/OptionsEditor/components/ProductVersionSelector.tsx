import { useCallback, useEffect, useState } from "react"
import { useSearchParams } from "react-router-dom"
import { clamp } from "lodash"

import { useGetPublicationVersions } from "entities"

export type ProductVersionSelectorProps = {
  onChange: (value: number) => void
}

export const ProductVersionSelector = ({ onChange }: ProductVersionSelectorProps) => {
  const [searchParams] = useSearchParams()

  const [version, setVersion] = useState<number | undefined>()

  const { data } = useGetPublicationVersions(searchParams.get("publicationId") || undefined)

  const handleChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const value = +e.target.value
      const version = clamp(value, 0, data?.latestVersion || 0)
      setVersion(version)
      onChange(version)
    },
    [data?.latestVersion, onChange],
  )

  useEffect(() => setVersion(data?.version), [data?.version])

  return (
    <div className="flex flex-col gap-2">
      <span>Latest: {data?.latestVersion}</span>
      Current: <input type="number" value={version ?? ""} onChange={handleChange} max={data?.latestVersion} min={0} />
    </div>
  )
}
