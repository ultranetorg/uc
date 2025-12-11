import { BaseFairOperation } from "./BaseFairOperation"

export class AccountNicknameChange extends BaseFairOperation {
  public nickname: string

  constructor(nickname: string) {
    super("AccountNicknameChange")
    this.nickname = nickname
  }
}
