﻿#if ETHEREUM
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Uccs.Rdn
{
	public partial class EmitFunction : EmitFunctionBase { }

    [Function("Emit")]
    public class EmitFunctionBase : FunctionMessage
    {
        [Parameter("bytes", "secret", 1)]
        public virtual byte[] Secret { get; set; }
    }

    public partial class RemoveNetFunction : RemoveNetFunctionBase { }

    [Function("RemoveNet")]
    public class RemoveNetFunctionBase : FunctionMessage
    {
        [Parameter("string", "name", 1)]
        public virtual string Name { get; set; }
    }

    public partial class SetNetFunction : SetNetFunctionBase { }

    [Function("SetNet")]
    public class SetNetFunctionBase : FunctionMessage
    {
        [Parameter("string", "name", 1)]
        public virtual string Name { get; set; }
        [Parameter("string", "nodes", 2)]
        public virtual string Nodes { get; set; }
    }

    public partial class CreatorFunction : CreatorFunctionBase { }

    [Function("creator", "address")]
    public class CreatorFunctionBase : FunctionMessage
    {

    }

    public partial class FindEmissionFunction : FindEmissionFunctionBase { }

    [Function("FindEmission", "uint256")]
    public class FindEmissionFunctionBase : FunctionMessage
    {
        [Parameter("bytes", "secret", 1)]
        public virtual byte[] Secret { get; set; }
    }

    public partial class GetNetFunction : GetNetFunctionBase { }

    [Function("GetNet", "string")]
    public class GetNetFunctionBase : FunctionMessage
    {
        [Parameter("string", "name", 1)]
        public virtual string Name { get; set; }
    }
    
    public partial class CreatorOutputDTO : CreatorOutputDTOBase { }

    [FunctionOutput]
    public class CreatorOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class FindEmissionOutputDTO : FindEmissionOutputDTOBase { }

    [FunctionOutput]
    public class FindEmissionOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetNetOutputDTO : GetNetOutputDTOBase { }

    [FunctionOutput]
    public class GetNetOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }
}
#endif
