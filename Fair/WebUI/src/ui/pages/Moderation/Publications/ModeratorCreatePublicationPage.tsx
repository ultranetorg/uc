import { useCallback, useEffect, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { useModerationContext } from "app"
import { SvgEyeSm, SvgSearchMd, SvgX } from "assets"
import { SEARCH_DELAY } from "config"
import { useGetUnpublishedSiteProduct } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { BaseVotableOperation, PageType, ProposalCreation, ProposalOption, Role } from "types"
import { ButtonBar, ButtonOutline, ButtonPrimary, Input, MessageBox } from "ui/components"
import { ModerationHeader, ProductFieldsTree } from "ui/components/specific"
import { showToast } from "utils"

export const ModeratorCreatePublicationPage = () => {
  const { siteId } = useParams()
  const { getOperationVoterId, isModerator } = useModerationContext()
  const { mutate, isPending } = useTransactMutationWithStatus()
  const navigate = useNavigate()
  const { t } = useTranslation("createPublication")

  const voterId = getOperationVoterId("publication-creation")

  const [searchParams, setSearchParams] = useSearchParams()
  const [query, setQuery] = useState(searchParams.get("productId") ?? "")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)
  const { data: product } = useGetUnpublishedSiteProduct(siteId, debouncedQuery)

  useEffect(() => {
    setSearchParams(product?.id ? { productId: product.id } : {}, { replace: true })
  }, [product?.id, setSearchParams])

  const handleInputClear = useCallback(() => {
    setQuery("")
  }, [setQuery])

  const handleCreatePublication = useCallback(() => {
    const role = isModerator ? Role.Moderator : Role.Publisher
    const options = [
      {
        title: "",
        operation: { $type: "PublicationCreation", Product: product!.id } as unknown as BaseVotableOperation,
      },
    ] as ProposalOption[]

    const operation = new ProposalCreation(siteId!, voterId!, role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:publicationCreated"), "success")
        navigate(`/${siteId}/m/c`)
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [isModerator, mutate, navigate, product, siteId, t, voterId])

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={t("searchProduct")}
        parentBreadcrumbs={[
          { title: t("common:proposals"), path: `/${siteId}/m` },
          { title: t("common:publications"), path: `/${siteId}/m/c` },
        ]}
        components={
          <>
            {!!product && !!product.fields && !!voterId && (
              <ButtonBar className="items-center">
                <ButtonPrimary
                  className="h-11 w-43.75"
                  label={t("createPublication")}
                  onClick={handleCreatePublication}
                  disabled={isPending}
                  loading={isPending}
                />
                <Link
                  to={`/${siteId}/m/v`}
                  state={{ productId: product.id, source: "ModeratorCreatePublicationPage" as PageType }}
                >
                  <ButtonOutline
                    disabled={isPending}
                    className="h-11 w-52"
                    label="Preview publication"
                    iconBefore={<SvgEyeSm className="fill-gray-800" />}
                  />
                </Link>
              </ButtonBar>
            )}
          </>
        }
      />
      <div className="max-w-120">
        <Input
          value={query}
          onChange={setQuery}
          placeholder={t("placeholders:enterProductId")}
          className="max-w-120 placeholder:text-gray-500"
          iconAfter={
            <>
              {query && (
                <div onClick={handleInputClear} className="cursor-pointer">
                  <SvgX className="stroke-gray-400 hover:stroke-gray-950" />
                </div>
              )}
              <SvgSearchMd className="size-5 stroke-gray-500" />
            </>
          }
        />
      </div>

      {!!debouncedQuery && product && product.fields && product.fields.length ? (
        <ProductFieldsTree productFields={product.fields} />
      ) : debouncedQuery ? (
        <MessageBox className="p-6" message={!product ? t("productNotFound") : t("productHasNoField")} />
      ) : (
        <></>
      )}
    </div>
  )
}
