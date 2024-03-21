// Ethreum-based Ultranet Network Activation System
// SPDX-License-Identifier: 0BSD
pragma solidity >=0.7.4;

contract Nas
{
    struct Transfer
    {
        address sender;
        uint    amount;
    }

    mapping (bytes => Transfer) transfers;
    mapping (string => string) zones;
    
    address nextVersion;
    address public creator;
    uint16 protocolVersion = 0;

    constructor()
    {
        creator = msg.sender;
        
        SetZone("Testnet1", "168.119.54.200\n"
							"78.47.204.100\n"	
                            "78.47.214.161\n"
                            "78.47.214.166\n"
                            "78.47.214.170\n"
                            "78.47.214.171\n"
                            "78.47.198.218\n"
                            "78.47.205.229");

        SetZone("Mainnet",  "168.119.54.200\n"
							"78.47.204.100\n"	
                            "78.47.214.161\n"
                            "78.47.214.166\n"
                            "78.47.214.170\n"
                            "78.47.214.171\n"
                            "78.47.198.218\n"
                            "78.47.205.229");
    }
/*
    function IsAdmin() public view returns (bool)
    {
        return msg.sender == creator;
    }
*/
    function RequestTransfer(bytes memory secret) payable public
    {
        require(msg.value > 0, "Value (ETH) must be greater than zero");
        require(transfers[secret].sender == 0x0000000000000000000000000000000000000000, "Already exists");

       // payable(address(this)).transfer(msg.value);
        payable(0).transfer(msg.value);

        Transfer memory t = Transfer(msg.sender, msg.value);

        transfers[secret] = t;
    }
    
    function FindTransfer(bytes memory secret) public view returns(uint)
    {
        return transfers[secret].amount;
    }

    function SetZone(string memory name, string memory nodes) public
    {
        require(msg.sender == creator, "Access denied");
        require(bytes(name).length > 0, "Name can not be empty");
        require(bytes(nodes).length > 0, "Nodes can not be empty");

        zones[name] = nodes;
    }
    
    function RemoveZone(string memory name) public
    {
        require(msg.sender == creator, "Access denied");

        delete zones[name];
    }

    function GetZone(string memory name) public view returns (string memory)
    {
        return zones[name];
    }
}
