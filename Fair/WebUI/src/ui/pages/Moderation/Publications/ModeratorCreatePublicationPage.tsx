import { useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useSearchParams } from "react-router-dom"
import { useDebounceValue } from "usehooks-ts"

import { useOperationPolicy, useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { SvgEyeSm, SvgSearchMd, SvgX } from "assets"
import { SEARCH_DELAY } from "config"
import { useGetUnpublishedStoreProduct } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { BaseVotableOperation, ProposalCreation, ProposalOption, Role } from "types"
import { ButtonBar, ButtonOutline, ButtonPrimary, Input, MessageBox } from "ui/components"
import { ModerationPublicationHeader, ModerationHeader, ProductFieldsTree } from "ui/components/specific"
import { isVotingRequired, routes, showToast } from "utils"

export const ModeratorCreatePublicationPage = () => {
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const { t } = useTranslation("createPublication")

  const { voterId } = useOperationPolicy("publication-creation")
  const { store: site } = useStoreContext()
  const { policies } = useStorePoliciesContext()
  const { isModerator } = useStoreRolesContext()
  const { mutate, isPending } = useTransactMutationWithStatus()

  const isRequiredVoting = isVotingRequired("publication-creation", site, policies)

  const [searchParams, setSearchParams] = useSearchParams()
  const [query, setQuery] = useState(searchParams.get("productId") ?? "")
  const [debouncedQuery] = useDebounceValue(query, SEARCH_DELAY)
  const { data: product, isError } = useGetUnpublishedStoreProduct(storeId, debouncedQuery)

  useStoreTitle(site?.title, query ? `Search Product - ${query}` : "Search Product")

  const parentBreadcrumbs = useMemo(
    () => [{ title: t("common:publications"), path: routes.moderation.publications(storeId!) }],
    [storeId, t],
  )

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

    const operation = new ProposalCreation(storeId!, voterId!, role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:publicationCreated"), "success")

        if (isRequiredVoting) {
          navigate(routes.moderation.publications(storeId!))
        } else {
          navigate(routes.moderation.publications(storeId!, "unpublished"))
        }
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [isModerator, isRequiredVoting, mutate, navigate, product, storeId, t, voterId])

  const isProductValid = !isError && !!product && !!product.fields && product.fields.length > 0

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={t("searchProduct")} parentBreadcrumbs={parentBreadcrumbs} />
      <div className="flex flex-col gap-12">
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
                <SvgSearchMd className="size-5 shrink-0 stroke-gray-500" />
              </>
            }
          />
        </div>

        {!!debouncedQuery && isProductValid ? (
          <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
            <ModerationPublicationHeader
              siteId={storeId!}
              title={product.title}
              logoId={product.logoId}
              authorId={product.authorId}
              authorTitle={product.authorTitle}
              components={
                <>
                  {isProductValid && !!voterId && (
                    <ButtonBar className="items-center">
                      <ButtonPrimary
                        className="h-11 w-40 capitalize"
                        label={t("common:create")}
                        onClick={handleCreatePublication}
                        disabled={isPending}
                        loading={isPending}
                      />
                      <Link
                        to={routes.moderation.preview(storeId!)}
                        state={{
                          productId: product.id,
                          previousPath: `${routes.moderation.createPublication(storeId!)}?productId=${product.id}`,
                          parentBreadcrumbs: [
                            ...parentBreadcrumbs,
                            {
                              title: t("searchProduct"),
                              path: `${routes.moderation.createPublication(storeId!)}?productId=${product.id}`,
                            },
                          ],
                        }}
                      >
                        <ButtonOutline
                          disabled={isPending}
                          className="h-11 w-40 capitalize"
                          label={t("common:preview")}
                          iconBefore={<SvgEyeSm className="fill-gray-800" />}
                        />
                      </Link>
                    </ButtonBar>
                  )}
                </>
              }
            />
            <ProductFieldsTree productFields={product.fields} />
          </div>
        ) : debouncedQuery ? (
          <MessageBox className="p-6" message={isError || !product ? t("productNotFound") : t("productHasNoField")} />
        ) : null}
      </div>
    </div>
  )
}
