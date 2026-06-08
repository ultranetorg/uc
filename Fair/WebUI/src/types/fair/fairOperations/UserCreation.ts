import { BaseFairOperation } from "./BaseFairOperation"

export class UserCreation extends BaseFairOperation {
  public owner: string

  constructor(owner: string) {
    super("UserCreation")
    this.owner = owner
  }
}
