import { ProductFieldViewModel } from "types"
import { ProductFieldViewString } from "./ProductFieldViewString.tsx"
import { JSX } from "react"
import { ProductFieldViewUri } from "./ProductFieldViewUri.tsx"
import { ProductFieldViewBigInt } from "./ProductFieldViewBigInt.tsx"
import { ProductFieldViewDate } from "./ProductFieldViewDate.tsx"
import { ProductFieldViewFile } from "./ProductFieldViewFile.tsx"
import { ProductFieldViewVideo } from "./ProductFieldViewVideo.tsx"

export const ProductFieldView = ({ node: { type, value, parent } }: { node: ProductFieldViewModel }) => {
  let component: JSX.Element

  switch (type) {
    case "uri": {
      if (parent?.name === "video") {
        component = <ProductFieldViewVideo value={value} />
      } else {
        component = <ProductFieldViewUri value={value} />
      }
      break
    }
    case "money": {
      component = <ProductFieldViewBigInt value={value} />
      break
    }
    case "date": {
      component = <ProductFieldViewDate value={value} />
      break
    }
    case "file-id": {
      component = <ProductFieldViewFile value={value} />
      break
    }

    default: {
      component = <ProductFieldViewString value={value} />
    }
  }

  return <div className="px-4 py-2 text-sm">{component}</div>
}
