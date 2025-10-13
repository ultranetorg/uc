import { ProductFieldModel } from "types"
import { ProductFieldViewString } from "./ProductFieldViewString.tsx"
import { JSX } from "react"
import { ProductFieldViewUri } from "./ProductFieldViewUri.tsx"

export const ProductFieldView = ({ node: { type, value } }: { node: ProductFieldModel }) => {
  let component: JSX.Element;

  switch (type) {
    case 'uri': {
      component = <ProductFieldViewUri value={value} />
      break;
    }
    default: {
      component = <ProductFieldViewString value={value} />
    }
  }

  return <div className="px-4 py-2 text-sm">{component}</div>;
}
