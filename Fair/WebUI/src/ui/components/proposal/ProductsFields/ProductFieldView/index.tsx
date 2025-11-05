import { ProductFieldCompareViewModel, ProductFieldViewModel } from "types"
import { ProductFieldViewString } from "./ProductFieldViewString.tsx"
import { JSX } from "react"
import { ProductFieldViewUri } from "./ProductFieldViewUri.tsx"
import { ProductFieldViewBigInt } from "./ProductFieldViewBigInt.tsx"
import { ProductFieldViewDate } from "./ProductFieldViewDate.tsx"
import { ProductFieldViewFile } from "./ProductFieldViewFile.tsx"
import { ProductFieldViewVideo } from "./ProductFieldViewVideo"
import { getCompareStatus } from "../selected-props.ts"

export const ProductFieldView = ({ node }: { node: ProductFieldViewModel }) => {
  let component: JSX.Element

  const compareStatus = getCompareStatus(node)
  const oldValue = compareStatus ? (node as ProductFieldCompareViewModel).oldValue ?? null : null
  console.log('node', node)

  switch (node.type) {
    case "uri": {
      if (parent?.name === "video") {
        component = <ProductFieldViewVideo value={node.value} />
      } else {
        component = <ProductFieldViewUri value={node.value} />
      }
      break
    }
    case "money": {
      component = <ProductFieldViewBigInt value={node.value} />
      break
    }
    case "date": {
      component = <ProductFieldViewDate value={node.value} />
      break
    }
    case "file-id": {
      component = <ProductFieldViewFile value={node.value} />
      break
    }

    default: {
      component = <ProductFieldViewString value={node.value} oldValue={oldValue} status={compareStatus} />
    }
  }

  return <div className="h-full px-4 py-2 text-sm">{component}</div>
}
