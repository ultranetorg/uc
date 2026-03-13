//using System.Diagnostics.CodeAnalysis;
//using Ardalis.GuardClauses;
//using Uccs.Web.Pagination;

//namespace Uccs.Fair;

//public class ProposalService2
//(
//	ILogger<ProposalService2> logger,
//	FairMcv mcv
//)
//{
//	class TypeUpdateFunc
//	{
//		public Type Type { get; set; }
//		public Action<FairMcv, Proposal, ProposalModel2> UpdateFunc { get; set; }
//	}

//	IDictionary<FairOperationClass, TypeUpdateFunc> _operationClassToUpdateMap = new Dictionary<FairOperationClass, TypeUpdateFunc>()
//	{
//		{FairOperationClass.SiteModeratorAddition, new TypeUpdateFunc() {Type = typeof(SiteModeratorAdditionProposalModel2), UpdateFunc = UpdateSiteModeratorAddition}},
//		{FairOperationClass.SiteModeratorRemoval, new TypeUpdateFunc() {Type = typeof(SiteModeratorRemovalProposalModel2), UpdateFunc = UpdateSiteModeratorRemoval}},
//	};

//	public TotalItemsResult<ProposalModel2> GetProposals([NotEmpty][NotNull] string siteId, FairOperationClass? optionClass, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
//	{
//		logger.LogDebug($"GET {nameof(ProposalService2)}.{nameof(GetProposals)} method called with {{SiteId}}, {{OptionClass}}, {{Page}}, {{PageSize}}", siteId, optionClass, page, pageSize);

//		Guard.Against.NullOrEmpty(siteId);
//		Guard.Against.Negative(page);
//		Guard.Against.NegativeOrZero(pageSize);

//		AutoId siteEntityId = AutoId.Parse(siteId);

//		lock(mcv.Lock)
//		{
//			Site site = mcv.Sites.Latest(siteEntityId);
//			if(site == null)
//			{
//				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
//			}

//			return LoadProposalsNotOptmized(site, optionClass, page, pageSize, cancellationToken);
//		}
//	}

//	TotalItemsResult<ProposalModel2> LoadProposalsNotOptmized(Site site, FairOperationClass? optionClass, int page, int pageSize, CancellationToken cancellationToken)
//	{
//		if(cancellationToken.IsCancellationRequested)
//			return TotalItemsResult<ProposalModel2>.Empty;

//		var totalItems = 0;
//		var result = new List<ProposalModel2>(pageSize);
//		foreach (AutoId proposalId in site.Proposals)
//		{
//			if(cancellationToken.IsCancellationRequested)
//				return new TotalItemsResult<ProposalModel2>() { Items = result, TotalItems = totalItems };

//			Proposal proposal = mcv.Proposals.Latest(proposalId);
//			if(optionClass.HasValue && proposal.OptionClass != optionClass)
//			{
//				continue;
//			}

//			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
//			{
//				var by = (FairUser) mcv.Users.Latest(proposal.By);

//				ProposalModel2 model = null;
//				if(optionClass.HasValue && _operationClassToUpdateMap.TryGetValue(optionClass.Value, out TypeUpdateFunc typeUpdateFunc))
//				{
//					model = Activator.CreateInstance(typeUpdateFunc.Type) as ProposalModel2;
//					typeUpdateFunc.UpdateFunc(mcv, proposal, model);
//				}
//				else
//				{
//					model = new ProposalModel2();
//					UpdateProposalModelProperties(model, proposal, by);
//				}

//				result.Add(model);
//			}

//			++totalItems;
//		}

//		return new TotalItemsResult<ProposalModel2>()
//		{
//			Items = result,
//			TotalItems = totalItems
//		};
//	}

//	ProposalModel2 UpdateProposalModelProperties(ProposalModel2 model, Proposal proposal, FairUser by)
//	{
//		model.Id = proposal.Id.ToString();
//		model.OptionClass = proposal.OptionClass;
//		model.OptionsYesCount = proposal.Options.Select(o => o.Yes.Length);
//		model.NeitherCount = proposal.Neither.Length;
//		model.AnyCount = proposal.Any.Length;
//		model.BanCount = proposal.Ban.Length;
//		model.BanishCount = proposal.Banish.Length;
//		model.CreationTime = proposal.CreationTime.Hours;
//		model.Title = proposal.Title;
//		model.Text = proposal.Text;
//		model.By = new AccountBaseAvatarModel(by);

//		return model;
//	}

//	static void UpdateSiteModeratorAddition(FairMcv mcv, Proposal proposal, ProposalModel2 model)
//	{
//	}

//	static void UpdateSiteModeratorRemoval(FairMcv mcv, Proposal proposal, ProposalModel2 model)
//	{
//	}
//}