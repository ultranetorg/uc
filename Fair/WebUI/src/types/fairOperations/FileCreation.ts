import { BaseFairOperation } from "./BaseFairOperation"

export class FileCreation extends BaseFairOperation {
  public owner: string
  public data: string
  public mime: string

  constructor(owner: string, data: string, mime: string) {
    super("FileCreation")
    this.owner = owner
    this.data = data
    this.mime = mime
  }
}
