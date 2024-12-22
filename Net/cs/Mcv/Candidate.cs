namespace Uccs.Net;

// 	public class Candidate
// 	{
// 		public EntityId			Account { get; set; }
// 		public Time				Registered { get; set; }
// 		public bool				Offline { get; set; }
// 		public IPAddress[]		BaseRdcIPs { get; set; } = [];
// 
// 		public override string ToString()
// 		{
// 			return $"Account={Account}, BaseRdcIPs={{{BaseRdcIPs.Length}}}";
// 		}
// 	
//   		public virtual void WriteBaseState(BinaryWriter writer)
//  		{
//  			writer.Write(Account);
//  			writer.Write(Registered);
//  			writer.Write(Offline);
// 			writer.Write(BaseRdcIPs, i => writer.Write(i));
//  		}
//  
//  		public virtual void ReadBaseState(BinaryReader reader)
//  		{
// 			Account			= reader.Read<EntityId>();
// 			Registered		= reader.Read<Time>();
// 			Offline			= reader.ReadBoolean();
// 			BaseRdcIPs		= reader.ReadArray(() => reader.ReadIPAddress());
// 		}
// 	}
