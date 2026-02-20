import { BaseFairOperation } from "./BaseFairOperation"

export class FileDeletion extends BaseFairOperation {
  public file: string

  constructor(file: string) {
    super("FileDeletion")
    this.file = file
  }
}
