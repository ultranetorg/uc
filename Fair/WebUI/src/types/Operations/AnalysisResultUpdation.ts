import { AnalysisResult, ResourceAddress } from "types"

import { Operation } from "./Operation"

export type AnalysisResultUpdation = {
  resource: ResourceAddress
  release: string
  consil: ResourceAddress
  result: AnalysisResult
} & Operation
