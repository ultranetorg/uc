using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace UC.Net
{
    public partial class FindTransferFunction : FindTransferFunctionBase { }

    [Function("FindTransfer", "uint256")]
    public class FindTransferFunctionBase : FunctionMessage
    {
        [Parameter("bytes", "secret", 1)]
        public virtual byte[] Secret { get; set; }
    }

    public partial class GetZoneFunction : GetZoneFunctionBase { }

    [Function("GetZone", "string")]
    public class GetZoneFunctionBase : FunctionMessage
    {
        [Parameter("string", "name", 1)]
        public virtual string Name { get; set; }
    }

    public partial class RemoveZoneFunction : RemoveZoneFunctionBase { }

    [Function("RemoveZone")]
    public class RemoveZoneFunctionBase : FunctionMessage
    {
        [Parameter("string", "name", 1)]
        public virtual string Name { get; set; }
    }

    public partial class RequestTransferFunction : RequestTransferFunctionBase { }

    [Function("RequestTransfer")]
    public class RequestTransferFunctionBase : FunctionMessage
    {
        [Parameter("bytes", "secret", 1)]
        public virtual byte[] Secret { get; set; }
    }

    public partial class SetZoneFunction : SetZoneFunctionBase { }

    [Function("SetZone")]
    public class SetZoneFunctionBase : FunctionMessage
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

    public partial class FindTransferOutputDTO : FindTransferOutputDTOBase { }

    [FunctionOutput]
    public class FindTransferOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetZoneOutputDTO : GetZoneOutputDTOBase { }

    [FunctionOutput]
    public class GetZoneOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("string", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }







    public partial class CreatorOutputDTO : CreatorOutputDTOBase { }

    [FunctionOutput]
    public class CreatorOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }
}
