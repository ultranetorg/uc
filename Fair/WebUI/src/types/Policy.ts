import { ApprovalRequirement } from "./ApprovalRequirement"
import { OperationType } from "./OperationType"

export type Policy = {
  operationClass: OperationType
  approval: ApprovalRequirement
}
