﻿using System.Diagnostics;

namespace Uccs.Net;

public class McvNodeSettings : SavableSettings
{
	public McvSettings				Mcv { get; set; }
	public PeeringSettings			Peering { get; set; } = new();
	public PeeringSettings			NtnPeering { get; set; }
	public ApiSettings				Api { get; set; }
	public bool						Log { get; set; }
	public int						RdcQueryTimeout { get; set; } = 5000;
	public int						RdcTransactingTimeout { get; set; } = 5*60*1000;

	public virtual long				Roles => (Mcv?.Roles ?? 0);


	public McvNodeSettings() : base(NetXonTextValueSerializator.Default)
	{
	}

	public McvNodeSettings(string profile) : base(profile, NetXonTextValueSerializator.Default)
	{
		if(Debugger.IsAttached)
		{
			RdcQueryTimeout = int.MaxValue;
			RdcTransactingTimeout = int.MaxValue;
		}
	}
}