import { ApprovalRequirement } from "./ApprovalRequirement"
import { OperationType } from "./OperationType"
import { RoleName } from "./RoleName"

export type Policy = {
  operationClass: OperationType
  creators: RoleName[]
  approval: ApprovalRequirement
}
