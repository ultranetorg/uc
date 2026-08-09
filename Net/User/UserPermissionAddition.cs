namespace Uccs.Net;

//public class UserPermissionAddition : Operation
//{
//	public byte					Priority { get; set; }
//	public Permission			Permission { get; set; }
//
//	public override string		Explanation => $"{nameof(Priority)}={Priority}, {nameof(Permission)}={Permission}";
//	
//	public UserPermissionAddition()
//	{
//	}
//	
//	public override bool IsValid(McvNet net)
//	{ 
//		return Permission.Operations.All(i =>	i == (uint)OperationClass.CandidacyDeclaration ||
//												i == (uint)OperationClass.UtilityTransfer ||
//												i == (uint)OperationClass.UserBandwidthAllocation ||
//												i == (uint)OperationClass.UserNameChange||
//												i == (uint)OperationClass.UserOwnerChange ||
//												i == (uint)OperationClass.UserPermissionAddition ||
//												i == (uint)OperationClass.UserPermissionRemoval);
//	}
//
//	public override void Read(Reader reader)
//	{
//		Priority = reader.ReadByte();
//		Permission = reader.Read<Permission>();
//	}
//
//	public override void Write(Writer writer)
//	{
//		writer.Write(Priority);
//		writer.Write(Permission);
//	}
//
//	public override void Execute(Execution execution)
//	{
//		if(!User.IsPermitted(execution, execution.Net.Constructor.TypeToCode(GetType()), User.Id))
//		{
//			Error = Denied;
//			return;
//		}
//
//		User.Permissions = [..User.Permissions[..Priority], Permission, ..User.Permissions[Priority..]];
//	}
//}
