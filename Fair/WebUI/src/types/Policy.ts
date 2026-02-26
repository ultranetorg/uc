import { ApprovalRequirement } from "./ApprovalRequirement"
import { FairOperationClass } from "./FairOperationClass"

export type Policy = {
  operationClass: FairOperationClass
  approval: ApprovalRequirement
}
