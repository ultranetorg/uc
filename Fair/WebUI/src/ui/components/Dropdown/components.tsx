import Select, { Props } from "react-select"

import { DropdownItem } from "./types"

type CustomSelectProps = Props<DropdownItem, false>

export const CustomSelect = (props: CustomSelectProps) => <Select {...props} />
