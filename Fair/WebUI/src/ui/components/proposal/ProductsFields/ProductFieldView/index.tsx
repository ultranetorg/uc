import { ProductFieldModel } from "types"
import { ProductFieldViewString } from "./ProductFieldViewString.tsx"
import { JSX } from "react"
import { ProductFieldViewUri } from "./ProductFieldViewUri.tsx"
import { ProductFieldViewBigInt } from "./ProductFieldViewBigInt.tsx"
import { ProductFieldViewDate } from "./ProductFieldViewDate.tsx"
import { ProductFieldViewFile } from "./ProductFieldViewFile.tsx"

export const ProductFieldView = ({ node: { type, value } }: { node: ProductFieldModel }) => {
  let component: JSX.Element;

  switch (type) {
    case 'uri': {
      component = <ProductFieldViewUri value={value} />
      break;
    }
    case 'money': {
      component = <ProductFieldViewBigInt value={value} />
      break;
    }
    case 'date': {
      component = <ProductFieldViewDate value={value} />
      break;
    }
    case 'file-id': {
      component = <ProductFieldViewFile value={value} />
      break;
    }

    default: {
      component = <ProductFieldViewString value={value} />
    }
  }

  return <div className="px-4 py-2 text-sm">{component}</div>;
}
