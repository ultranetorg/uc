import { TFunction } from "i18next"
import { isDate } from "lodash"

import { SvgCheckCircle } from "assets"
import { Currency, AccountAuthor, RoundDomainBid, RoundEmission, RoundMember, ResourceAddress } from "types"
import { ListRow, CopyableText, Tag, TextLink } from "ui/components"
import { formatDataLength, formatDateTime, formatCurrencyString, shortenWideString, toYesNo } from "utils"

export const getListItemRenderer = (
  currency?: Currency,
  rate?: number,
  emissionMultiplier?: number,
  t?: TFunction,
  tag?: string,
) => {
  return (row: ListRow) => {
    // Types.
    if (row.type === "author") {
      return (value: string) => <TextLink to={`/authors/${value}`}>{value}</TextLink>
    }

    if (row.type === "account") {
      return (value: string) => value && <TextLink to={`/accounts/${value}`}>{shortenWideString(value)}</TextLink>
    }

    if (row.type === "currency") {
      return (value: bigint) => formatCurrencyString(value, currency, rate, emissionMultiplier)
    }

    if (row.type === "enums") {
      return (value: string) => {
        const items = value.split(",")
        return (
          <div className="flex gap-2">
            {items.map(item => (
              <Tag key={item} className="select-none">
                {item}
              </Tag>
            ))}
          </div>
        )
      }
    }

    if (row.type === "string_array") {
      return (value: string[]) => {
        if (value.length === 0) {
          return undefined
        }

        return (
          <div className="flex gap-2">
            {value.map(item => (
              <div key={item}>{item}</div>
            ))}
          </div>
        )
      }
    }

    if (row.type === "text_copyable") {
      return (value: string) => {
        return (
          <CopyableText
            textCopiedMessage={
              <div className="flex items-center gap-3 text-sm leading-[0.875rem] text-black">
                <SvgCheckCircle /> Value copied to clipboard
              </div>
            }
            textTitle={value}
          >
            {shortenWideString(value)}
          </CopyableText>
        )
      }
    }

    // Tags

    if (tag === "operation" && row.accessor === "name") {
      return (author: string) => <TextLink to={`/authors/${author}`}>{author}</TextLink>
    }

    // New
    if (row.accessor === "consil") {
      return (value?: ResourceAddress) => {
        if (!value) {
          return undefined
        }

        return (
          <TextLink
            to={`/authors/${value.author}/resources/${value.resource}`}
          >{`${value.author}/${value.resource}`}</TextLink>
        )
      }
    }

    // Old

    if (row.accessor === "address" || row.accessor === "id" || row.accessor === "name" || row.accessor === "resource") {
      return (value: string) => {
        return (
          <CopyableText
            textCopiedMessage={
              <div className="flex items-center gap-3 text-sm leading-[0.875rem] text-black">
                <SvgCheckCircle /> Address copied to clipboard
              </div>
            }
            textTitle={value}
          >
            {shortenWideString(value)}
          </CopyableText>
        )
      }
    }

    if (row.accessor === "authors") {
      return (values: AccountAuthor[]) => {
        if (values.length === 0) {
          return undefined
        }

        return (
          <div className="flex gap-2">
            {values.map(({ name }) => {
              return (
                <TextLink key={name} to={`/authors/${name}`}>
                  {name}
                </TextLink>
              )
            })}
          </div>
        )
      }
    }

    if (
      row.accessor === "account" ||
      row.accessor === "highestBidBy" ||
      row.accessor === "lastWinner" ||
      row.accessor === "owner" ||
      row.accessor === "to" ||
      row.accessor === "signer"
    ) {
      return (value: string) => value && <TextLink to={`/accounts/${value}`}>{shortenWideString(value)}</TextLink>
    }

    if (row.accessor === "release") {
      return (value: string) => {
        return (
          <CopyableText
            textClassName="overflow-hidden text-ellipsis text-sm leading-[0.875rem]"
            textCopiedMessage={
              <div className="flex items-center gap-3 text-sm leading-[0.875rem] text-black">
                <SvgCheckCircle /> Release Hash copied to clipboard
              </div>
            }
            textTitle={value}
          >
            {shortenWideString(value)}
          </CopyableText>
        )
      }
    }

    // Resource author
    if (row.accessor === "author") {
      return (author: string) => <TextLink to={`/authors/${author}`}>{author}</TextLink>
    }

    if (row.accessor === "roundId" || row.accessor === "startedAt") {
      return (value: string) => {
        return <TextLink to={`/rounds/${value}`}>{value}</TextLink>
      }
    }

    if (row.accessor === "parentId") {
      return (value: number) => <TextLink to={`/rounds/${value}`}>{value.toString()}</TextLink>
    }

    if (row.accessor === "transactionId") {
      return (value: string) => {
        return <TextLink to={`/transactions/${value}`}>{value}</TextLink>
      }
    }

    if (row.accessor === "emissions") {
      return (value: RoundEmission[]) => {
        if (value.length === 0) {
          return undefined
        }

        return (
          <>
            {value.map(item => (
              <div className="flex gap-2">
                {formatCurrencyString(item.wei, currency, rate, emissionMultiplier)} ({item.eid})
              </div>
            ))}
          </>
        )
      }
    }

    if (
      row.accessor === "bailStatus" ||
      row.accessor === "changes" ||
      row.accessor === "flags" ||
      row.accessor === "initials" ||
      row.accessor === "result" ||
      row.accessor === "type"
    ) {
      return (value: string) => {
        const items = value.split(",")
        return (
          <div className="flex gap-2">
            {items.map(item => (
              <Tag key={item} className="select-none">
                {item}
              </Tag>
            ))}
          </div>
        )
      }
    }

    if (row.accessor === "consil" && !!t) {
      return (value: number) => {
        return t("consil", { count: value, value })
      }
    }

    if (row.accessor === "$type" && !!t) {
      return (value: string) => t(value)
    }

    if (row.accessor === "dataLength") {
      return (value: number) => (value !== 0 ? formatDataLength(value) : undefined)
    }

    if (row.accessor === "rankCheck") {
      return (value: boolean) => toYesNo(value)
    }

    if (row.accessor === "domainBids") {
      return (bids: RoundDomainBid[]) => {
        if (bids.length === 0) {
          return undefined
        }

        return (
          <div className="flex gap-2">
            {bids.map(bid => (
              <TextLink key={bid.author + bid.bid + bid.tld} to={`/authors/${bid.author}`}>
                {bid.author}
              </TextLink>
            ))}
          </div>
        )
      }
    }

    if (row.accessor === "members") {
      return (members: RoundMember[]) => {
        if (members.length === 0) {
          return undefined
        }

        return (
          <>
            {members.map(member => (
              <TextLink key={member.account} to={`/accounts/${member.account}`}>
                {member.account}
              </TextLink>
            ))}
          </>
        )
      }
    }

    return (value: any) => (!isDate(value) ? value : formatDateTime(value))
  }
}
