import { FairOperationType } from "./FairOperationType"

export class BaseFairOperation {
  $type: FairOperationType

  constructor(type: FairOperationType) {
    this.$type = type
  }
}
