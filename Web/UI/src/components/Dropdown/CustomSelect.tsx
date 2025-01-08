import Select, { Props } from "react-select"

import { DropdownItem } from "./types"

export const CustomSelect = (props: Props<DropdownItem, false>) => (
  <Select {...props} theme={theme => ({ ...theme, borderRadius: 0 })} />
)
