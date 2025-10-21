import { memo } from "react"
import { AutoId, useGetFile } from "entities"
import { buildSrc } from "utils"
import { SpinnerRowSvg } from "assets"

export const ProductFieldViewFile = memo(({ value }: { value: string }) => {
  const id = AutoId.fromBase64(value).toString()

  const { error, data, isPending } = useGetFile(id)
  if (isPending) {
    return <SpinnerRowSvg />
  }
  if (error) {
    return <>Error occurred</>
  }

  return <img alt="image" className="h-full w-full object-cover" src={buildSrc(data)} />
})
