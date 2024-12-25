import { networks } from "constants"

import { StylizedSelect } from "./StylizedSelect"

const options = networks.map(x => ({ value: x.name.toLowerCase(), label: x.name }))
const defaultValue = networks[0].name.toLocaleLowerCase()

const styles = {
  controlStyle: {
    width: "157px",
    height: "40px",
  },
  singleValueStyle: {
    marginLeft: "0px",
  },
  valueContainerStyle: {
    marginLeft: "13px",
    paddingLeft: "0px",
  },
}

export const NetworkDropdown = () => (
  <StylizedSelect isSearchable={false} value={defaultValue} options={options} {...styles} />
)
