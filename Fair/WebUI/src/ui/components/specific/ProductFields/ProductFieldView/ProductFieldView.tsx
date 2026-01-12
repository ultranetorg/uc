import { JSX } from "react"

import { ProductFieldViewModel, ProductFieldCompareViewModel } from "../types"
import { getCompareStatus } from "../utils"

import { ProductFieldViewString } from "./ProductFieldViewString"
import { ProductFieldViewUri } from "./ProductFieldViewUri"
import { ProductFieldViewBigInt } from "./ProductFieldViewBigInt"
import { ProductFieldViewDate } from "./ProductFieldViewDate"
import { ProductFieldViewFile } from "./ProductFieldViewFile"
import { ProductFieldViewVideo } from "./ProductFieldViewVideo"

export const ProductFieldView = ({ node }: { node: ProductFieldViewModel }) => {
  let component: JSX.Element

  const compareStatus = getCompareStatus(node)
  const oldValue = compareStatus ? (node as ProductFieldCompareViewModel).oldValue : undefined

  switch (node.type) {
    case "uri": {
      if (node.parent?.name === "video") {
        component = <ProductFieldViewVideo value={node.value} oldValue={oldValue} status={compareStatus} />
      } else {
        component = <ProductFieldViewUri value={node.value} oldValue={oldValue} status={compareStatus} />
      }
      break
    }
    case "money": {
      component = <ProductFieldViewBigInt value={node.value} oldValue={oldValue} status={compareStatus} />
      break
    }
    case "date": {
      component = <ProductFieldViewDate value={node.value} oldValue={oldValue} status={compareStatus} />
      break
    }
    case "file-id": {
      component = <ProductFieldViewFile value={node.value} oldValue={oldValue} status={compareStatus} />
      break
    }

    default: {
      component = <ProductFieldViewString value={node.value} oldValue={oldValue} status={compareStatus} />
    }
  }

  return <div className="h-full px-4 py-2 text-sm">{component}</div>
}
