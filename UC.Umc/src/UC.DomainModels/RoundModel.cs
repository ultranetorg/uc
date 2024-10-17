namespace UC.DomainModels;

public class RoundModel
{
	public int Id { get; set; }

	public int? ParentId { get; set; }

	public string Hash { get; set; } = null!;

	public int ConsensusTime { get; set; }

	public long ConsensusExecutionFee { get; set; }

	public int CandidatesCount { get; set; }
	public int MembersCount { get; set; }
	public int FundsCount { get; set; }
	public int MigrationsCount { get; set; }
	public int ConsensusMigrationsCount { get; set; }

	public int ConsensusMemberLeaversCount { get; set; }

	public int ConsensusFundJoinersCount { get; set; }
	public int ConsensusFundLeaversCount { get; set; }

	public int ConsensusViolatorsCount { get; set; }

	public IEnumerable<RoundGenerator> Candidates { get; set; } = null!;
	public IEnumerable<RoundGenerator> Members { get; set; } = null!;

	public IEnumerable<string> Funds { get; set; } = null!;

	public IEnumerable<string> Migrations { get; set; } = null!;

	public IEnumerable<ForeignResult> ConsensusMigrations { get; set; } = null!;

	public IEnumerable<TransactionModel> Transactions { get; set; } = null!;
}

