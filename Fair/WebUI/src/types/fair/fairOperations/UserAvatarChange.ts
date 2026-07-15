import { BaseFairOperation } from "./BaseFairOperation"

export class UserAvatarChange extends BaseFairOperation {
  public image: string | null

  constructor(image: string | null) {
    super("UserAvatarChange")
    this.image = image
  }
}
