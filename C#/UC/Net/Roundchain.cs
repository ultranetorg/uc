using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.Util;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace UC.Net
{
	public delegate void BlockDelegate(Block b);

	public class Roundchain
	{
		class Genesis
		{
			public Zone Zone;
			public Cryptography Crypto;
			public string Rounds;
		}

		public const int									Pitch = 8;
		public const int									LastGenesisRound = Pitch*2;
		public const int									MembersMin = 7;
		public const int									MembersMax = 1024;
		public const int									NewMembersPerRoundMax = 1;
		public const int									MembersRotation = 32;
		const int											LoadedRoundsMax = 100_000;
		public static readonly Coin							BailMin = 1000;
		//public static readonly Coin							FundablesPercent = 10;
		public static readonly Coin							FeePerByte = new Coin(0.000001);

		public static Account								OrgAccount = Account.Parse("0x00fe929a68368c301a1906ed39016ee9be3d937b");
		public static Account								GenAccount = Account.Parse("0x00ffea61659c02c4a38d5736767bec23ab8d2875");

		public Settings										Settings;

		public List<Round>									Rounds	= new();
		public Dictionary<int, Round>						LoadedRounds = new();
		public List<Peer>									Members	= new();
		public List<Account>								Funds = new();

		public AccountTable									Accounts;
		public AuthorTable									Authors;
		public ProductTable									Products;
		public RocksDb										Database;
		
		public Log											Log;
		public BlockDelegate								BlockAdded;

 		public static readonly Account[]					Fathers =	{
																			Account.Parse("0x000038a7a3cb80ec769c632b7b3e43525547ecd1"),
																			Account.Parse("0x00015326bcf44c84a605afbdd5343de4aaf11387"),
																			Account.Parse("0x0002a311f7cf0aabfd3a248a89824bbd94a458a2"),
																			Account.Parse("0x00031174fcd4f971249e4112f925209a16813137"),
																			Account.Parse("0x0004761973068828923e7c811dd7f5b8eee0bae5"),
																			Account.Parse("0x0005a748d15de450cd488fbed9cc3b3213f042e5"),
																			Account.Parse("0x0006680ad7845cfb115cd56f834385817e93999a"),
																			//Account.Parse("0x0007f34bc43d41cf3ec2e6f684c7b9b131b04b41"),
																			//Account.Parse("0x00086c57b20ac627f1b04b7bf50bb27330438b6b"),
																			//Account.Parse("0x0009cc6575ddd868bc1476f7ea516af58125b0dd"),
																			//Account.Parse("0x000a52cad719404896a55f2dc2ea28dc7a6a9249"),
																			//Account.Parse("0x000b2dc121bd9114e7aab47754130f6148b38755"),
																			//Account.Parse("0x000c68d29c01f7e5873a9e353ee9af4d9505be93"),
																			//Account.Parse("0x000d381af6561f165a44680afa06e2360fd1e060"),
																			//Account.Parse("0x000ea12d3b17f96ef79ef4012db9df43a3d6da1b"),
																			//Account.Parse("0x000ff3c7e0b5c19447d5175cb4c6641cfa613152"),
 																		};

		readonly static List<Genesis>						Genesises =	new()
															{
																new Genesis {Zone = Zone.Localnet, Crypto = new NoCryptography(),		 Rounds = "000001020100000038a7a3cb80ec769c632b7b3e43525547ecd1000038a7a3cb80ec769c632b7b3e43525547ecd1000000000000000000000000620088857e2bd8107d10973587f7fc06938abaeec0531279c18a306f46b4bfaf00c0a80164030000ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000004c31ac6683dfcfca48e511cb8a94017f2b140cc77c9b4124033761e5b17205bb000000000000000000000000000100fe929a68368c301a1906ed39016ee9be3d937b00080006680ad7845cfb115cd56f834385817e93999a000000000000000000000000cc68da1ec4802a53161ee4505a0dd354d3370d87dc25ec7976df35a077efb37f0000020101090000a0dec5adc93536c0a8016a020009000040bd8b5b936b6c000005a748d15de450cd488fbed9cc3b3213f042e50000000000000000000000003db9baf6c23ce50886c89ad4a15454fe35f0b4a126e5cafb3c6d46278c585f720000020101090000a0dec5adc93536c0a80169020009000040bd8b5b936b6c000004761973068828923e7c811dd7f5b8eee0bae500000000000000000000000025c7ef6c5fbba25040b6456ed8fccc84e8e63802eefcdc91994ee36223e6a5940000020101090000a0dec5adc93536c0a80168020009000040bd8b5b936b6c0000031174fcd4f971249e4112f925209a16813137000000000000000000000000ce774e8cb5870c2812ed3f7297df7a2b62beff77c0f3131d7c7546c1e837730c0000020101090000a0dec5adc93536c0a80167020009000040bd8b5b936b6c000002a311f7cf0aabfd3a248a89824bbd94a458a2000000000000000000000000c4f57261b1d1dfe64afd6405c2b7580d81df26d32808f074a3d3e0c8537a2c480000020101090000a0dec5adc93536c0a80166020009000040bd8b5b936b6c0000015326bcf44c84a605afbdd5343de4aaf1138700000000000000000000000095e69c2bdec171d0667f2ae391e5355c077672f9514e5b5aeda868cf64cb04550000020101090000a0dec5adc93536c0a80165020009000040bd8b5b936b6c00000038a7a3cb80ec769c632b7b3e43525547ecd1000000000000000000000000c2272b737a269f04420b9215316413927e6f2aec968eb45b3ac374362a36f0620000020101090000a0dec5adc93536c0a80164020009000040bd8b5b936b6c0000fe929a68368c301a1906ed39016ee9be3d937b00000000000000000000000034194c3da9e99cf9effd5c4e692165f6762ab4f1926e7ab0390d4bc2b2682b09000002040102756f0100020008000064a7b3b6e00d0001000101030100ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000006a1c44b93aa2124e2b1726b2089321796dfaac11a1f8e3d020db3821f9b84577000081d8c4bd750000000000000000000000000100fe929a68368c301a1906ed39016ee9be3d937b00000000000000000000000095d426f93628e237ec35d9d796236f4a7e037112c8379d955028e0a9bbc3115c000101050202756f02554fff02000101030200ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000c22dec0bed12433eac3cb4546789aaa354ae71400ba0caff850e66abbddbc8800000010000000000000000000000000003000101030300ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000e0cc31da68c92ccb5ef8573707a9180667388535d3d678a55cbd708b6a686ed00000010000000000000000000000000004000101030400ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000008ec6ca7116de10344a0884d6775e2ec4bc95916d2a9d5c8be151a41a576e793e0000010000000000000000000000000005000101030500ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000004489ea46d813145a7a775ff6de76c06f6b2574dab17683e330a796aeae1e504c0000010000000000000000000000000006000101030600ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000b7a01009a7882b6e08b4c9ce5d0ce038d9bafc5f91196318e66e1d697f1dff6c0000010000000000000000000000000007000101030700ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000699a6173df7a91e783cd82f2438061e63ecab16997c0cc67f951132273873d590000010000000000000000000000000008000101030800ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000004ae0003fe2bb254eed893e21e4c7d89c67968c77d0f4a024383b74294ffaad56000001000000000000000001000038a7a3cb80ec769c632b7b3e43525547ecd10000000009000101030900ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000b339fc2b683f724b8fcf7bf4a35083ac08016fc0744fc5f5f16bdc18038a5d55000001000000000000000000000000000a000101030a00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000a4c3381a668e84e62454e557bb502eb6d457f6c71318683a301e34aa13b9fd6c000001000000000000000000000000000b000101030b00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000b1b9e103a18bc68cf1d0f7cf9b3fbb09ad741bebaa026ee6fba4478fbc45f7c8000001000000000000000000000000000c000101030c00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d287500000000000000000000000012327e75e615617a17731dcef32c64e522f1e49060acb4c710b778da595c8ffa000001000000000000000000000000000d000101030d00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d28750000000000000000000000002111a18e0df19b7da745ab65ece13bded7e042312a1afd8f071ed46f15eca890000001000000000000000000000000000e000101030e00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000d3b52c09406d6009811c9f8d1a36251d3a10b446af4cf9b67b672b00e4db8976000001000000000000000000000000000f000101030f00ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000ad21203663b9cba2d88b9794e8ba52e4016135517b236d38abef691bda3838350000010000000000000000000000000010000101031000ffea61659c02c4a38d5736767bec23ab8d287500ffea61659c02c4a38d5736767bec23ab8d2875000000000000000000000000f45ec5a75b09949188ec2740cf5b7607f227bc13efef655bccac79b2e7400dca000001014ae0003f0001000038a700000001000000000000"},
																//new Genesis {Zone = Zone.Local, Crypto = new EthereumCryptography(), Rounds = "0001000002030000ffea61659c02c4a38d5736767bec23ab8d28752118dcc5aa9c4ea5ac7a6baeb2c00fe795131249fff38aa123804415e92fc46067430454532a3445367b2f6fa24e6f0f1f71eeebf77d3f5179dbeec93eda59ea1b00000000000000000000000000000832b3d6ba18c945bbfc9baf44b52f65a449880b348520ccb42cd5fb38fc5016fe73c908ba73699e77c8c0d350ff14bc3d5f46e6d109f29cd21e4312679095a5f81b00000201090000a0dec5adc93536c0a8016a02080000c84e676dc11b000f31436863408437bf50c02865cab034aafe625d71fe4c9af7ccbbb72f7bd5b37feb8aba785eac0d60327ee7ab779525c12ee1957468336bc916c34a35273def1c00000201090000a0dec5adc93536c0a8016902080000c84e676dc11b00ae7ba257f8ae7dc6ae1c6c9a18f0947def9550a079abc02380020bea56162c37673c77412077a28c7e8e8d3803fbbdc11ff354dcf5f582d7aae42c5142a2b62e1c00000201090000a0dec5adc93536c0a8016802080000c84e676dc11b001a06703e7774c581c5f22c57e5d4ae01c7d3eb81e305ec5d32ae7521e6d53f836d9694151789a36e33dac68339f47bfe70b16c6b78fa6e8ca3ebcd44607b8ca11c00000201090000a0dec5adc93536c0a8016702080000c84e676dc11b00c97c2585746d1aed2eed862a21abeac3df1a4ceb2871429e3f40548660742159686039fdc966acc4679a8334b366c90db87333d849c5d92ed313625083f9981e1b00000201090000a0dec5adc93536c0a8016602080000c84e676dc11b006ffec6696eaf311a4e6d0b4ed03a3b8e4c7a37f8c4bd6f8e1a6568d3a58360916066f20a448846f1f5ce2fe0d494895dfc490278e38f70e94876e540f57231171b00000201090000a0dec5adc93536c0a8016502080000c84e676dc11b00e15bbcb49faa9daa8586b24aee9a1a7058b270e5325698eefb8507c7441616cf16fb8372bf549782c930b4e926affa5f30e01855a673a0142a95b730689996bb1c00000201090000a0dec5adc93536c0a8016402080000c84e676dc11b0089531e12cf910e62c26d5b8d5388527fa3d90b841955d385cfa9985482c706d054cef1778afbefa0ce61aeae488adb549b49dc29461e4348a32f6cf62d9a781a1b0000020402756f01000208000064a7b3b6e00d000100000038a7a3cb80ec769c632b7b3e43525547ecd1262509a8167f6693d39f21763f27134736a2b7ac3748f63f8bc602f7d6411ec357f4482ec576c8fca0c1e81f572ca0049461ed1c01a7cd5a1fb2fb64cdf3acf51cc0a8016400000000000101000001030100ffea61659c02c4a38d5736767bec23ab8d287515bb970258e792336b5b5a25a2776bb5545d86727877b66484e2cd938336721a6a79c88085bcefcab120b0983db1b64b6c0d3836a7a075f7672d8f96b03458b81b0081d8c4bd75000000000000000000000000011add463d846716812fb3d8992eef576e4e8aea1061a47ff5e4412fdadc32befc04e5db6b6074a017ee57b1cfce40f80f74aa7ae1b35a43212cc086bf914750c71b0101010502756f02554fff00000000000201000001030200ffea61659c02c4a38d5736767bec23ab8d2875726fa00c278d0194b0ed9b201949f2b2b579598afeeaf69a841b44149c5f9e1448ae784e390eb45adc8049beff11929d5eabfd388a29ee6713b7974541c8191a1c00010000000000000000000000000000000000000301000001030300ffea61659c02c4a38d5736767bec23ab8d28753eef6e8f4c15ddcfd8a6d7f18ed8d9f99a873523595a92a1fb4a2bc2e771bab84ddc51fcd004f55fa8a49597102d54195c18a4be6423b962bf58fa6fbe19dac61b00010000000000000000000000000000000000000401000001030400ffea61659c02c4a38d5736767bec23ab8d2875e4d286f6fd65b81e96a3612c32edcb047ee164c163fdb2443a6dbd95fde57493559f726e39eb0944f8e670005e1effe59e124e19fe9d4da5d6dc6f749c2eb7781b00010000000000000000000000000000000000000501000001030500ffea61659c02c4a38d5736767bec23ab8d28752b8041c52bc22ae8fab2e95c2a03223e84bad203db2f1bd34957e41d18a774f06a6a3e1ec12aacf994a3c8bc61d587d0984b4d223369cd38be0425f9ba9ede291b00010000000000000000000000000000000000000601000001030600ffea61659c02c4a38d5736767bec23ab8d2875511857c0b4df66d58af0c1237f1e55e3508d4232c1c9c7935b8576861ab9869c330d429aaab5587e9c19d61d4c68f0c7e5cd3223ab62ed3c09ff0f85a2d7c2be1b00010000000000000000000000000000000000000701000001030700ffea61659c02c4a38d5736767bec23ab8d2875b890b2162ca8b00993baf33d84568b67343212bf7fca18eb04e1677dd55fe7bd32e2dd35822c66d2612222a3100713eee5772719ccc244ad98480fee2292de391c00010000000000000000000000000000000000000801000001030800ffea61659c02c4a38d5736767bec23ab8d2875ee02ea3c839beb5929dc5bd7015d9a508419dffcff6d82d64fc35bb0d7c29ca66c7eaa76f023d11a5e277241b217f51458b4bbb26b695de51443b646f239131c1c0001000000000000000001000038a7a3cb80ec769c632b7b3e43525547ecd10000000000000000000901000001030900ffea61659c02c4a38d5736767bec23ab8d28752d86e829f4f3467a2a7d35411992baed3f646b25bb94da7b858320e8d92e0f153b410caa15ff00f0daef27915834118842c4ee23e918f14bd384be2b14e388841c0001000000000000000000000100fe929a68368c301a1906ed39016ee9be3d937b000000000000000a01000001030a00ffea61659c02c4a38d5736767bec23ab8d2875a1ab94398ff9ed9488233381df7aec80cd239e9f60cf5d3473a07f0c87774c7932ef58a0246dde93a989f4581c8a6a98fbb550eddafdad9b76270dbf7ae86a4e1c00010000000000000000000000000000000000000b01000001030b00ffea61659c02c4a38d5736767bec23ab8d287582bd2bf95a787747a6ded0e177430b660b59e1cd8f15eedd7473197e2bfeae2f01fe94ebd4a25c5c07b203993b873df3635eea3f558c4b04b45c824f0c7f9e551b00010000000000000000000000000000000000000c01000001030c00ffea61659c02c4a38d5736767bec23ab8d28758bdd4fe8f8a51e305c1047c3cbb60f71e49e225b62a2a9d0ad24288f24d599193c624422c21ad0a689d07222eaa574ca3ab5d74d1f4ee0e7bee7ec9713964caf1b00010000000000000000000000000000000000000d01000001030d00ffea61659c02c4a38d5736767bec23ab8d28751f7cad65a8132dd003cc78f5cd2072b968eab3fbc81723412744f687570b3d381c2a4a04902125201583da4914d9a0a3f0e8ff7a8abbd458626209269a05b2091c00010000000000000000000000000000000000000e01000001030e00ffea61659c02c4a38d5736767bec23ab8d2875756b64452b2da3d326f9f457eba3ff70c2fa91dc726b0adecb867a340ae2be256b4059e38f16a4f1bc28fb7bbf341b83de39d52bf53fff3da53a52d7981a735c1c00010000000000000000000000000000000000000f01000001030f00ffea61659c02c4a38d5736767bec23ab8d287508994bf6f6ade6ea4327286abc5e5a91d7ea8877817d1f10125ea5e5d5f347f955b7c714ff44119171552674a1b3323263f201bad656719708fd30ed3ece58fc1b00010000000000000000000000000000000000001001000001031000ffea61659c02c4a38d5736767bec23ab8d28753311bef7c11f68a9fd87a6f624f77dbcaea32116972ae7662dc30f9b562d5da77d9defa7040c6b2bac8644902e795c9e7b83e2f0e64662d89c1b6c54d62d20b71b000101763880ea0001000038a7000000010000000000000000000000"},

																//new Genesis {Zone = Zone.Testnet0,	Crypto = new EthereumCryptography(), Rounds = "000001020100000038a7a3cb80ec769c632b7b3e43525547ecd1f5134209d039b6b90e792450dbe8d0aa771a450b479c161b13cc0d056b46558d78b427946ed87451efca7890c0e4a2787ae4bb7f5b5c1f15372718a180f922d41b4e2fcc64030000ffea61659c02c4a38d5736767bec23ab8d2875662789e409a26ca49c583a349b69618cd1ce323fad7dafe114fb28abbaa171cd569d0615fad5d88381aacae25bbe5ce4278aa292c519319cb7e38b37dbd190961c0000000000000000000000000100fe929a68368c301a1906ed39016ee9be3d937b0008a285a2283f5c4559b49d81c80e7d32d2c3ae8e4fe1ab5ce94d87a3601befbdd276d00ce8d629b5290c7d4fdaecd16fd453338acf3645b0a2609eebbd43806ecf1c00020101090000a0dec5adc935364e2fcde5020009000040bd8b5b936b6c00840e5c222074ed3bf7e26f2f040e51d0d009f6c90bc1e61c6bec3589606bf8673e4ed0f6250f21e97e564dba4e98257670785232defbdba4e6e4cb5f49e6d04c1c00020101090000a0dec5adc935364e2fc6da020009000040bd8b5b936b6c00ec3f2cc97a33c369a3b785d524d38c4f58e22c5854a2980944f7a1521774a7fc0f4a2d6aff555487c13b5f4a8b27d844a48afc35fb0c616c4f675f78e1d3b7651b00020101090000a0dec5adc935364e2fd6ab020009000040bd8b5b936b6c00e628d45a93b75430b1ba924955295a2d9fc53393fa9aeacf7dc28977d9fc5478079268ba8f354ac45d870ecf9ca4aa3463f4a7c1f855513de2161816827667581c00020101090000a0dec5adc935364e2fd6aa020009000040bd8b5b936b6c00e140d1b584d68d63b9b050662a8bd4c8b870ac861cb53a54f7d7fb9cef4c4815105efaf9325a3836c7dadd1d493c25402361d66ba75530a7863f8bdc77a15b6b1b00020101090000a0dec5adc935364e2fd6a6020009000040bd8b5b936b6c009f3da5d161284e26e4da6d41ae8448efe12d37d13f28c2b3a22775647d6d132852053a0008390f21ca29d3e0a1cd0f669b943d1b30fe0751c1fa5ab038dfe1d51c00020101090000a0dec5adc935364e2fd6a1020009000040bd8b5b936b6c0078da1bca585a857b3d1be3e70437f215cc3ada5d8c4d9d4a80c81c05005f43b55ae193b6aec0ff4605d10d7d2dd36320a092c33fa17cedaa16ad1627d253aac91c00020101090000a0dec5adc935364e2fcc64020009000040bd8b5b936b6c00b7479d7c4c542d0893e065c3914cbaefe20993b670a96295c74c2bbb4730427e7fe95ee75d3d4d0b93589412df376eb707f55ee4ac2c0374a6428e026300ae251c0002040102756f0100020008000064a7b3b6e00d0001000101030100ffea61659c02c4a38d5736767bec23ab8d287574ec8f76c2a6aad74b33cc9c090e4ffb2d083115aee93f2ff74f8e9903fe1ae86c626956f91d1473a245e82e240b0502f4d00248b83299fe828ec52ffd6d27b41c0081d8c4bd75000000000000000000000000018a1bedb16356f1b9499dc20579bf1b3c99e12ca2b6163879eb3e83314a90047b2769a581b1d8dd7cc798f14ff0b95f730a0d087e4b4d389aa80241a648ef57361c0101050202756f02554fff02000101030200ffea61659c02c4a38d5736767bec23ab8d2875a0105130a2609cb4e8b3398fffcf86117a79a41b21eb33840398c0a99d4a41af5e529b480058a8583ebb1e8718c21b1fd831f3f9f8935b0468a404eea768717b1c00010000000000000000000000000003000101030300ffea61659c02c4a38d5736767bec23ab8d287582e47b8f8191079e282c13f9d3ae1f61270cbe5ab7dd023bb5229a3e2e0899106fda5752cd3ba9e26c6d0a20cbd7ea1366a2427f90defcd664ee5f8cd4dd8a091b00010000000000000000000000000004000101030400ffea61659c02c4a38d5736767bec23ab8d28754d4ee934e5b1fee07e55852144816ecb275b0f6c216371c243ddf33538c5c0d65c6c4e36fa87b96a8c92b25c518968d6a4672ca81c580609d46e3913b47af2b61c00010000000000000000000000000005000101030500ffea61659c02c4a38d5736767bec23ab8d2875087395248ba2561d81efa782962d2d67c4b8c8dde4028209b0b9b9aa04464daf6fe2d5ca842389014cc93468b62302d4ad09ed8f4c93f3bb8807673d4e9959e41b00010000000000000000000000000006000101030600ffea61659c02c4a38d5736767bec23ab8d28755c5936a64a3f3ac4cc982a5c485e05889126b57a835cd083ac23790ce8afb9a22686bfc02dc198cb4b8bd282ebeb4fbd002e1cb4ac2d09ba0e68bec64b774ca21b00010000000000000000000000000007000101030700ffea61659c02c4a38d5736767bec23ab8d2875c58f7bf74b5952c5a989494223b84ad06ec405114fc145355dc70839e2cab81a3e9147950d05d9fccf4e231474fdf782de2007c19a46dd51f55ddf70be312cf11c00010000000000000000000000000008000101030800ffea61659c02c4a38d5736767bec23ab8d2875c11d4c39faddaedf3091911250ef0c1479f60818ce35fb8393b4d123788121c411339bbd44f3091ab14f45fc75198bd4fd28874b37a61925d0a4decefa3334271b0001000000000000000001000038a7a3cb80ec769c632b7b3e43525547ecd10000000009000101030900ffea61659c02c4a38d5736767bec23ab8d28753a9032657cc1433204da2db080d6efdf035885855811716767314226287f60937d354fec92f428a433dc69a2015395f17adeaa3101ea9eae235a803208fd0be51c0001000000000000000000000000000a000101030a00ffea61659c02c4a38d5736767bec23ab8d28754464b274f931ecc8e9c6e403e1736132c8edd28db4d195762a35c720eb3efd5928a7c762be13451ab73235acff316b6861cd1c1067ad464f24b8d7b4e089d86f1c0001000000000000000000000000000b000101030b00ffea61659c02c4a38d5736767bec23ab8d28751a1aff5d76520b5b7e35bb0cc0b6b02506535035acb0e9fedaf8b9f86cd902cf027550efdcb98ac0c84beb95a11d9d293431f930ddc71bd2096ef1569f21a12b1c0001000000000000000000000000000c000101030c00ffea61659c02c4a38d5736767bec23ab8d2875c438c8bf112e03288672771526478e44dc4de48d6f8f0a29742ab288d5f585484e16aa0f8cd907d7ebd37a16da7892d7e2120886f2b2dec17d993d8f23263dba1c0001000000000000000000000000000d000101030d00ffea61659c02c4a38d5736767bec23ab8d2875e36068edd2d27d0faa5183e98def0e080aed92cbdfb0b6bcdf343171fae5258138fe3f7c89316aa6a893386d7224cc51060e6f91ae4ad3acc6566abef1bb5ccd1b0001000000000000000000000000000e000101030e00ffea61659c02c4a38d5736767bec23ab8d2875058ee8f1d6873facda8451e53d8d82d878455967af3a651f345ddfc3b832310678c012fe3298be2f6d489952476e06e9a7d27e75c375f21f02d913ec0c8903991b0001000000000000000000000000000f000101030f00ffea61659c02c4a38d5736767bec23ab8d2875811d61eb26e70339cda975f8005e1bc36463106fc7de9f69dab085e8de3a11984f8a74cb99fa7c9f4c814a377a6b9f373622ef0dfb2b6245024b5432288e157a1b00010000000000000000000000000010000101031000ffea61659c02c4a38d5736767bec23ab8d2875bd2c9f15cfa4fb686a2c9b341c9e781f7c1026ae63622f873a2045a8e9ae25a932fd071d6607d3e0ed4d3717698d08a2454a3ff1740257521674bd1af92209ab1c00010180790a560001000038a700000001000000000000"},
																new Genesis {Zone = Zone.Testnet1,	Crypto = new EthereumCryptography(), Rounds = "000001020100000038a7a3cb80ec769c632b7b3e43525547ecd1f5134209d039b6b90e792450dbe8d0aa771a450b479c161b13cc0d056b46558d78b427946ed87451efca7890c0e4a2787ae4bb7f5b5c1f15372718a180f922d41b4e2fcc64030000ffea61659c02c4a38d5736767bec23ab8d2875662789e409a26ca49c583a349b69618cd1ce323fad7dafe114fb28abbaa171cd569d0615fad5d88381aacae25bbe5ce4278aa292c519319cb7e38b37dbd190961c0000000000000000000000000100fe929a68368c301a1906ed39016ee9be3d937b0008a285a2283f5c4559b49d81c80e7d32d2c3ae8e4fe1ab5ce94d87a3601befbdd276d00ce8d629b5290c7d4fdaecd16fd453338acf3645b0a2609eebbd43806ecf1c00020101090000a0dec5adc935364e2fcde5020009000040bd8b5b936b6c00840e5c222074ed3bf7e26f2f040e51d0d009f6c90bc1e61c6bec3589606bf8673e4ed0f6250f21e97e564dba4e98257670785232defbdba4e6e4cb5f49e6d04c1c00020101090000a0dec5adc935364e2fc6da020009000040bd8b5b936b6c00ec3f2cc97a33c369a3b785d524d38c4f58e22c5854a2980944f7a1521774a7fc0f4a2d6aff555487c13b5f4a8b27d844a48afc35fb0c616c4f675f78e1d3b7651b00020101090000a0dec5adc935364e2fd6ab020009000040bd8b5b936b6c00e628d45a93b75430b1ba924955295a2d9fc53393fa9aeacf7dc28977d9fc5478079268ba8f354ac45d870ecf9ca4aa3463f4a7c1f855513de2161816827667581c00020101090000a0dec5adc935364e2fd6aa020009000040bd8b5b936b6c00e140d1b584d68d63b9b050662a8bd4c8b870ac861cb53a54f7d7fb9cef4c4815105efaf9325a3836c7dadd1d493c25402361d66ba75530a7863f8bdc77a15b6b1b00020101090000a0dec5adc935364e2fd6a6020009000040bd8b5b936b6c009f3da5d161284e26e4da6d41ae8448efe12d37d13f28c2b3a22775647d6d132852053a0008390f21ca29d3e0a1cd0f669b943d1b30fe0751c1fa5ab038dfe1d51c00020101090000a0dec5adc935364e2fd6a1020009000040bd8b5b936b6c0078da1bca585a857b3d1be3e70437f215cc3ada5d8c4d9d4a80c81c05005f43b55ae193b6aec0ff4605d10d7d2dd36320a092c33fa17cedaa16ad1627d253aac91c00020101090000a0dec5adc935364e2fcc64020009000040bd8b5b936b6c00b7479d7c4c542d0893e065c3914cbaefe20993b670a96295c74c2bbb4730427e7fe95ee75d3d4d0b93589412df376eb707f55ee4ac2c0374a6428e026300ae251c0002040102756f0100020008000064a7b3b6e00d0001000101030100ffea61659c02c4a38d5736767bec23ab8d287574ec8f76c2a6aad74b33cc9c090e4ffb2d083115aee93f2ff74f8e9903fe1ae86c626956f91d1473a245e82e240b0502f4d00248b83299fe828ec52ffd6d27b41c0081d8c4bd75000000000000000000000000018a1bedb16356f1b9499dc20579bf1b3c99e12ca2b6163879eb3e83314a90047b2769a581b1d8dd7cc798f14ff0b95f730a0d087e4b4d389aa80241a648ef57361c0101050202756f02554fff02000101030200ffea61659c02c4a38d5736767bec23ab8d2875a0105130a2609cb4e8b3398fffcf86117a79a41b21eb33840398c0a99d4a41af5e529b480058a8583ebb1e8718c21b1fd831f3f9f8935b0468a404eea768717b1c00010000000000000000000000000003000101030300ffea61659c02c4a38d5736767bec23ab8d287582e47b8f8191079e282c13f9d3ae1f61270cbe5ab7dd023bb5229a3e2e0899106fda5752cd3ba9e26c6d0a20cbd7ea1366a2427f90defcd664ee5f8cd4dd8a091b00010000000000000000000000000004000101030400ffea61659c02c4a38d5736767bec23ab8d28754d4ee934e5b1fee07e55852144816ecb275b0f6c216371c243ddf33538c5c0d65c6c4e36fa87b96a8c92b25c518968d6a4672ca81c580609d46e3913b47af2b61c00010000000000000000000000000005000101030500ffea61659c02c4a38d5736767bec23ab8d2875087395248ba2561d81efa782962d2d67c4b8c8dde4028209b0b9b9aa04464daf6fe2d5ca842389014cc93468b62302d4ad09ed8f4c93f3bb8807673d4e9959e41b00010000000000000000000000000006000101030600ffea61659c02c4a38d5736767bec23ab8d28755c5936a64a3f3ac4cc982a5c485e05889126b57a835cd083ac23790ce8afb9a22686bfc02dc198cb4b8bd282ebeb4fbd002e1cb4ac2d09ba0e68bec64b774ca21b00010000000000000000000000000007000101030700ffea61659c02c4a38d5736767bec23ab8d2875c58f7bf74b5952c5a989494223b84ad06ec405114fc145355dc70839e2cab81a3e9147950d05d9fccf4e231474fdf782de2007c19a46dd51f55ddf70be312cf11c00010000000000000000000000000008000101030800ffea61659c02c4a38d5736767bec23ab8d2875c11d4c39faddaedf3091911250ef0c1479f60818ce35fb8393b4d123788121c411339bbd44f3091ab14f45fc75198bd4fd28874b37a61925d0a4decefa3334271b0001000000000000000001000038a7a3cb80ec769c632b7b3e43525547ecd10000000009000101030900ffea61659c02c4a38d5736767bec23ab8d28753a9032657cc1433204da2db080d6efdf035885855811716767314226287f60937d354fec92f428a433dc69a2015395f17adeaa3101ea9eae235a803208fd0be51c0001000000000000000000000000000a000101030a00ffea61659c02c4a38d5736767bec23ab8d28754464b274f931ecc8e9c6e403e1736132c8edd28db4d195762a35c720eb3efd5928a7c762be13451ab73235acff316b6861cd1c1067ad464f24b8d7b4e089d86f1c0001000000000000000000000000000b000101030b00ffea61659c02c4a38d5736767bec23ab8d28751a1aff5d76520b5b7e35bb0cc0b6b02506535035acb0e9fedaf8b9f86cd902cf027550efdcb98ac0c84beb95a11d9d293431f930ddc71bd2096ef1569f21a12b1c0001000000000000000000000000000c000101030c00ffea61659c02c4a38d5736767bec23ab8d2875c438c8bf112e03288672771526478e44dc4de48d6f8f0a29742ab288d5f585484e16aa0f8cd907d7ebd37a16da7892d7e2120886f2b2dec17d993d8f23263dba1c0001000000000000000000000000000d000101030d00ffea61659c02c4a38d5736767bec23ab8d2875e36068edd2d27d0faa5183e98def0e080aed92cbdfb0b6bcdf343171fae5258138fe3f7c89316aa6a893386d7224cc51060e6f91ae4ad3acc6566abef1bb5ccd1b0001000000000000000000000000000e000101030e00ffea61659c02c4a38d5736767bec23ab8d2875058ee8f1d6873facda8451e53d8d82d878455967af3a651f345ddfc3b832310678c012fe3298be2f6d489952476e06e9a7d27e75c375f21f02d913ec0c8903991b0001000000000000000000000000000f000101030f00ffea61659c02c4a38d5736767bec23ab8d2875811d61eb26e70339cda975f8005e1bc36463106fc7de9f69dab085e8de3a11984f8a74cb99fa7c9f4c814a377a6b9f373622ef0dfb2b6245024b5432288e157a1b00010000000000000000000000000010000101031000ffea61659c02c4a38d5736767bec23ab8d2875bd2c9f15cfa4fb686a2c9b341c9e781f7c1026ae63622f873a2045a8e9ae25a932fd071d6607d3e0ed4d3717698d08a2454a3ff1740257521674bd1af92209ab1c00010180790a560001000038a700000001000000000000"},
															};

		readonly byte[]										LastRoundKey	= new byte[] {1};
		readonly byte[]										WeiSpentKey		= new byte[] {2};
		readonly byte[]										FactorKey		= new byte[] {3};
		readonly byte[]										EmissionKey		= new byte[] {4};
		readonly byte[]										MembersKey		= new byte[] {5};
		readonly byte[]										FundsKey		= new byte[] {6};

		public Round										LastConfirmedRound	=> Rounds.FirstOrDefault(i => i.Confirmed) ?? LastSavedRound;
		public Round										LastVotedRound		=> Rounds.FirstOrDefault(i => i.Voted) ?? LastConfirmedRound;
		public Round										LastNonEmptyRound	=> Rounds.FirstOrDefault(i => i.Blocks.Any()) ?? LastConfirmedRound;
		public Round										LastPayloadRound	=> Rounds.FirstOrDefault(i => i.Blocks.Any(i => i is Payload)) ?? LastConfirmedRound;
		Round												LastSavedRound	{
																				get
																				{
																					var d = Database.Get(LastRoundKey);

																					if(d == null)
																						return null;
																					else
																						return FindRound(BitConverter.ToInt32(d));
																				}
																			}

		public BigInteger									LastSavedWeiSpent	=> new BigInteger(Database.Get(WeiSpentKey));
		public Coin											LastSavedFactor		=> new Coin(Database.Get(FactorKey));
		public Coin											LastSavedEmission	=> new Coin(Database.Get(EmissionKey));

		public ColumnFamilyHandle							AccountsFamily	=> Database.GetColumnFamily(nameof(Accounts));
		public ColumnFamilyHandle							AuthorsFamily	=> Database.GetColumnFamily(nameof(Authors));
		public ColumnFamilyHandle							ProductsFamily	=> Database.GetColumnFamily(nameof(Products));
		public ColumnFamilyHandle							RoundsFamily	=> Database.GetColumnFamily(nameof(Rounds));

		public static int									GetValidityPeriod(int rid) => rid + Pitch;

		public Roundchain(Settings settings, Log log, INas nas, Vault vault, RocksDb database)
		{
			Settings = settings;
			Log = log;
			Database = database;

			///GenerateFathers(256);

			Accounts = new AccountTable(this, AccountsFamily);
			Authors = new AuthorTable(this, AuthorsFamily);
			Products = new ProductTable(this, ProductsFamily);

			if(LastSavedRound == null)
			{
				var ips = nas.GetInitials(settings.Zone).ToArray();

				if(Settings.Dev.GenerateGenesis)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					var gen = vault.GetFather(GenAccount);
					var org = vault.GetFather(OrgAccount);

					//long datebase = 1900;

					void write(int rid)
					{
						var r = FindRound(rid);
						r.Voted = true;
						//r.Confirmed = true;
						r.Write(w);
					}

					var b0 = new Payload(this)
							{
								RoundId		= 0,
								TimeDelta	= 0,
								Reference	= RoundReference.Empty,
							};

					var jr = new GeneratorJoinRequest(this)
								{
									RoundId	= 0,
									IP		= ips[0]
								};
					jr.Sign(vault.GetFather(Fathers[0]));
					Add(jr, false);

					var t = new Transaction(Settings, org);
					t.AddOperation(new Emission(org, Web3.Convert.ToWei(1, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
					t.AddOperation(new AuthorBid(org, "uo", 0){ Id = 1 });
					t.Sign(gen, 0);
					b0.AddNext(t);
						
					for(int i=0; i<Fathers.Length; i++)
					{
						var f = vault.GetFather(Fathers[i]);
					
						t = new Transaction(Settings, f);
						t.AddOperation(new Emission(f, Web3.Convert.ToWei(settings.Zone == Zone.Mainnet ? 2 : 2_000, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
						t.AddOperation(new CandidacyDeclaration(f, new Coin(1000), ips[i]){ Id = 1 });
						t.Sign(gen, 0);

						b0.AddNext(t);
					}

					b0.FundJoiners.Add(OrgAccount);

					b0.Sign(gen);
					Add(b0, false);

					write(0);
					
					for(int i = 1; i < Pitch; i++)
					{
						var b = new Payload(this)
								{
									RoundId		= i,
									TimeDelta	= i == 1 ? ((long)TimeSpan.FromDays(365).TotalMilliseconds + 1) : 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
									Reference	= RoundReference.Empty,
								};

						if(i == 1)
						{
							t = new Transaction(Settings, org);
							t.AddOperation(new AuthorRegistration(org, "uo", "UO", 255){ Id = 2 });
							t.Sign(gen, i);
							b.AddNext(t);
						}
							
						b.Sign(gen);
						Add(b, false);

						write(i);
					}

					for(int i = Pitch; i <= LastGenesisRound; i++)
					{
						var p = GetRound(i - Pitch);

						var b = new Payload(this)
								{
									RoundId		= i,
									TimeDelta	= 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
									Reference	= Refer(p)
								};
	
						if(i == Pitch)
							b.Joiners.Add(Fathers[0]);

						b.Sign(gen);
						Add(b, false);

						write(i);
					}
					
					var g = s.ToArray().ToHex();
					
					if(g != Genesises.Find(i => i.Zone == Settings.Zone && i.Crypto.GetType() == Cryptography.Current.GetType()).Rounds)
						throw new IntegrityException("Genesis update needed");

					Rounds.Clear();
				}

				var rd = new BinaryReader(new MemoryStream(Genesises.Find(i => i.Zone == Settings.Zone && i.Crypto.GetType() == Cryptography.Current.GetType()).Rounds.HexToByteArray()));
	
				for(int i = 0; i < Pitch*2 + 1; i++)
				{
					var r = new Round(this);
					r.Read(rd);
	
					Rounds.Insert(0, r);
			
					if(i == Pitch)
					{
						r.ConfirmedJoiners = new ();
						r.ConfirmedJoiners.Add(Fathers[0]);

						r.ConfirmedFundJoiners = new ();
						r.ConfirmedFundJoiners.Add(OrgAccount);
					}

					r.ConfirmedViolators	= r.ConfirmedViolators		?? new();
					r.ConfirmedJoiners		= r.ConfirmedJoiners		?? new();
					r.ConfirmedLeavers		= r.ConfirmedLeavers		?? new();
					r.ConfirmedFundJoiners	= r.ConfirmedFundJoiners	?? new();
					r.ConfirmedFundLeavers	= r.ConfirmedFundLeavers	?? new();

					foreach(var p in r.Payloads)
						p.Confirmed = true;

					if(r.Id > 0)
						r.Time = CalculateTime(r, r.Unique.OfType<Payload>());

					r.Confirmed = true;

					Seal(r);
				}

				if(!Rounds.All(i => i.Payloads.All(i => i.Transactions.All(i => i.Operations.All(i => i.Successful)))))
				{
					throw new IntegrityException("Genesis construction failed");
				}
			}
			else
			{
				var r = new BinaryReader(new MemoryStream(Database.Get(MembersKey)));
				Members = r.ReadList<Peer>(() => { var p = new Peer(); p.ReadMember(r); return p; });

				r = new BinaryReader(new MemoryStream(Database.Get(FundsKey)));
				Funds = r.ReadList<Account>();
			}
		}

		public string GenerateFathers(int n)
		{
 			var accs = new EthECKey[n];
 
 			var tasks = new Task[n];
 
 			for(int i=0; i<n; i++)
 			{
 				tasks[i] =	Task.Run(() =>
 							{
 								while(accs.Contains(null))
 								{
 									var k = EthECKey.GenerateKey();
 	
 									var pub = Sha3Keccack.Current.CalculateHash(k.GetPubKeyNoPrefix());
 					
 									if(pub[12] == 0x00)
 									{
 										var i = pub[13];
 		
 										if(i < n && accs[i] == null)
 										{
 											accs[i] = k;

											byte[] array2 = new byte[pub.Length - 12];
											Array.Copy(pub, 12, array2, 0, pub.Length - 12);

											new PrivateAccount(k).Save(Path.Join(Settings.Secret.Fathers, "0x" + array2.ToHex()), Settings.Secret.Password);
 										}
 									}
 								}
 							});
 			}
 
 			Task.WaitAll(tasks);
 
 			return string.Join(Environment.NewLine, accs.Select(i => i.GetPrivateKey()));
		}

		public void Add(Block b, bool execute = true)
		{
			var r = GetRound(b.RoundId);

			b.Round = r;
			r.Blocks.Add(b);

			if(execute)
			{
				if(b is GeneratorJoinRequest jr)
				{
					jr.Declaration = Accounts.FindLastOperation<CandidacyDeclaration>(jr.Generator);
				}
				else if(b is Payload)
				{
 					for(int i = r.Id; i <= LastPayloadRound.Id; i++)
 					{
 						var ir = GetRound(i);
 						
 						if(ir.Payloads.Any())
						{
							ir.Time = CalculateTime(ir, ir.Payloads);
							Execute(ir, ir.Payloads, null);
						}
 						else
 							break;
 					}
				}

				if(b is Vote v && r.FirstArrivalTime == DateTime.MaxValue)
				{
					r.FirstArrivalTime = DateTime.UtcNow;
				} 
			}

			BlockAdded?.Invoke(b);
		}

		public void Add(IEnumerable<Block> bb)
		{
			foreach(var i in bb)
			{
				Add(i);
			}
		}

		public bool Verify(Block b)
		{
			if(b.RoundId <= LastConfirmedRound.Id)
				return false;

			var r = FindRound(b.RoundId);
	
			if(r != null && r.Blocks.Any(i => i.Signature.SequenceEqual(b.Signature)))
				return false;
		
			return b.Valid;
		}

		public Round GetRound(int rid)
		{
			var r = FindRound(rid);

			if(r == null)
			{	
				r = new Round(this) {Id = rid};
				r.LastAccessed = DateTime.UtcNow;
				Rounds.Add(r);
				Rounds = Rounds.OrderByDescending(i => i.Id).ToList();
			}

			return r;
		}

		public Round FindRound(int rid)
		{
			foreach(var i in Rounds)
				if(i.Id == rid)
				{
					i.LastAccessed = DateTime.UtcNow;
					return i;
				}

			if(LoadedRounds.ContainsKey(rid))
			{
				var r = LoadedRounds[rid];
				r.LastAccessed = DateTime.UtcNow;
				return r;
			}

			var d = Database.Get(BitConverter.GetBytes(rid), RoundsFamily);

			if(d != null)
			{
				var r = new Round(this);
				r.Id			= rid; 
				r.Voted			= true; 
				r.Confirmed		= true;
				r.LastAccessed	= DateTime.UtcNow;

				r.Load(new BinaryReader(new MemoryStream(d)));
	
				LoadedRounds[r.Id] = r;
				Recycle();
				return r;
			}
			else
				return null;
		}

		void Recycle()
		{
			if(LoadedRounds.Count > LoadedRoundsMax)
			{
				foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.LastAccessed).Skip(LoadedRoundsMax))
				{
					LoadedRounds.Remove(i.Key);
				}
			}
		}

		public IEnumerable<Peer> VotersFor(Round r)
		{
			return Members.Where(i => i.JoinedGeneratorsAt < r.Id);
		}

		public IEnumerable<GeneratorJoinRequest> JoinersFor(Round round)
		{
			return FindRound(round.ParentId).JoinRequests.Select(jr =>	{
																			var d = Accounts.FindLastOperation<CandidacyDeclaration>(jr.Generator, rp: r => r.Confirmed && r.Id <= round.ParentId - Pitch);
																			return new{jr = jr, d = d};
																		})	/// round.ParentId - Pitch means to not join earlier than [Pitch] after declaration, and not redeclare after a join is requested
															.Where(i => i.d != null && i.d.Bail >= (Settings.Dev != null && Settings.Dev.DisableBailMin ? 0 : BailMin))
															.OrderByDescending(i => i.d.Bail)
															.Select(i => i.jr);
		}

		public IEnumerable<Account> ProposeJoiners(Round round)
		{
			var joiners = JoinersFor(round);

			var n = Members.Count < MembersMax ? MembersMax - Members.Count : MembersRotation;

			//.Where(i =>	{ 
			//				var d = Accounts.FindLastOperation<CandidacyDeclaration>(i, null, null, null, r => r.Id <= round.Id);
			//				return d != null && d.Transaction.Payload.RoundId <= round.Id - Pitch*2 && d.Bail >= (Settings.Dev != null && Settings.Dev.DisableBailMin ? 0 : BailMin); 
			//			})

			return joiners.Take(n).Select(i => i.Generator);
		}

		public IEnumerable<Account> ProposeLeavers(Round round, Account generator)
		{
			var joiners = JoinersFor(round);

			var o = VotersFor(round).Where(i =>	i.JoinedGeneratorsAt < round.ParentId &&
												Rounds.Count(r =>	round.ParentId <= r.Id && r.Id < round.Id &&					/// in previous Pitch number of rounds
																	r.Blocks.Any(b => b.Generator == i.Generator)) < Pitch * 2/3 &&	/// sent less than 2/3 of required blocks
												!Enumerable.Range(round.Id - Pitch + 1, Pitch - 1).Select(i => FindRound(i)).Any(r => r.Votes.Any(v => v.Generator == generator && v.Leavers.Contains(i.Generator))) /// not yet reported in prev [Pitch-1] rounds
											)	
												
									.Select(i => i.Generator);

			if(!o.Any() && Members.Count == MembersMax && joiners.Any())
				return Members.OrderByDescending(i => i.JoinedGeneratorsAt).Take(joiners.Take(MembersRotation).Count()).Select(i => i.Generator);
			else
				return o;
		}

		public bool QuorumReached(Round r)
		{
			var members = VotersFor(r).Select(i => i.Generator);

			return r.Unique.Count(i => members.Contains(i.Generator)) >= Math.Max(1, members.Count() * 2/3);
		}

		public bool QuorumFailed(Round r)
		{
			var max = VotersFor(r).Select(i => i.Generator);

			return r.Unique.Count() >= Math.Max(1, max.Count() * 2/3) && r.Majority.Count() + (max.Count() - r.Unique.Count()) < Math.Max(1, max.Count() * 2/3);
		}

		public ChainTime CalculateTime(Round round, IEnumerable<Payload> votes)
		{
 			if(round.Id == 0)
 			{
 				return round.Time;
 			}

 			if(!votes.Any())
 			{
				return round.Previous.Time + new ChainTime(1);
			}

			///var t = 0L;
			///var n = 0;
			///
			///for(int i = Math.Max(0, round.Id - Pitch + 1); i <= round.Id; i++)
			///{
			///	var r = FindRound(i);
			///	t += r.Payloads.Sum(i => i.Time.Ticks);
			///	n += r.Payloads.Count();
			///}
			///
			///t /= n;

			return round.Previous.Time + new ChainTime(votes.Sum(i => i.TimeDelta)/votes.Count());
		}

		public IEnumerable<Transaction> CollectValidTransactions(IEnumerable<Transaction> txs, Round round)
		{
			txs = txs.Where(i => round.Id <= i.RoundMax /*&& IsSequential(i, round.Id)*/);

			if(txs.Any())
			{
// 				var p = new Payload(this);
// 				p.Member	= Account.Zero;
// 				p.Time		= DateTime.UtcNow;
// 				p.Round		= round;
// 				p.TimeDelta	= 1;
// 					
// 				foreach(var i in txs)
// 				{
// 					p.AddNext(i);
// 				}
// 				
//  				Execute(round, new Payload[] {p}, null);
 	
 //				txs = txs.Where(t => t.SuccessfulOperations.Any());
			}

			return txs;
		}

		//public bool IsSequential(Transaction transaction, int ridmax)
		//{
		//	var prev = Accounts.FindLastTransaction(transaction.Signer, t => t.Successful, null, r => r.Id < ridmax);
		//
		//	if(transaction.Id == 0 && prev == null)
		//		return true;
		//
		//	if(transaction.Id == 0 && prev != null || transaction.Id != 0 && prev != null && prev.Id != transaction.Id - 1)
		//		return false;
		//
		//	/// STRICT: return prev != null && (prev.Payload.Confirmed || prev.Payload.Transactions.All(i => IsSequential(i, i.Payload.RoundId))); /// All transactions in a block containing 'prev' one must also be sequential
		//	return prev.Payload.Confirmed || IsSequential(prev, prev.Payload.RoundId);
		//}

		///public bool IsSequential(Operation transaction, int ridmax)
		///{
		///	var prev = Accounts.FindLastOperation(transaction.Signer, o => o.Successful, t => t.Successful, null, r => r.Id < ridmax);
		///
		///	if(transaction.Id == 0 && prev == null)
		///		return true;
		///
		///	if(transaction.Id == 0 && prev != null || transaction.Id != 0 && prev != null && prev.Id != transaction.Id - 1)
		///		return false;
		///
		///	/// STRICT: return prev != null && (prev.Payload.Confirmed || prev.Payload.Transactions.All(i => IsSequential(i, i.Payload.RoundId))); /// All transactions in a block containing 'prev' one must also be sequential
		///	return prev.Transaction.Payload.Confirmed || IsSequential(prev, prev.Transaction.Payload.RoundId);
		///}

		public RoundReference Refer(Round round)
		{
			if(round.Id < Pitch)
				return RoundReference.Empty;

			if(!round.Parent.Confirmed && round.Id > LastGenesisRound)
				return null;

			var pp = round.Majority.OfType<Payload>().OrderBy(i => i.OrderingKey, new BytesComparer());
			
			var rr = new RoundReference();

			rr.Payloads		= pp.						Select(i => i.Prefix).ToList();
			rr.Violators	= round.ElectedViolators.	Select(i => i.Prefix).ToList();
			rr.Leavers		= round.ElectedLeavers.		Select(i => i.Prefix).ToList();
			rr.Joiners		= round.ElectedJoiners.		Select(i => i.Prefix).OrderBy(i => i, new BytesComparer()).Take(rr.Leavers.Count + NewMembersPerRoundMax).ToList();
			rr.FundLeavers	= round.ElectedFundLeavers.	Select(i => i.Prefix).ToList();
			rr.FundJoiners	= round.ElectedFundJoiners.	Select(i => i.Prefix).ToList();
			rr.Time			= CalculateTime(round, pp);

			return rr;
		}

		public void Execute(Round round, IEnumerable<Payload> payloads, IEnumerable<Account> blockforkers)
		{
			var prev = round.Previous;
				
			if(round.Id != 0 && prev == null)
				return;

			round.Members			= Members.ToList();
			round.Funds				= Funds.ToList();
			round.ExecutingPayloads = payloads;
			//round.Time				= time;//CalculateTime(round, payloads);

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();
			round.AffectedProducts.Clear();
			round.AffectedRounds.Clear();

			round.Emission	= round.Id == 0 ? 0						: (prev == LastSavedRound ?	LastSavedEmission	: prev.Emission);
			round.WeiSpent	= round.Id == 0 ? 0						: (prev == LastSavedRound ?	LastSavedWeiSpent	: prev.WeiSpent);
			round.Factor	= round.Id == 0 ? Emission.FactorStart	: (prev == LastSavedRound ?	LastSavedFactor		: prev.Factor);

			foreach(var b in round.ExecutingPayloads.Reverse())
				foreach(var t in b.Transactions.AsEnumerable().Reverse())
					foreach(var o in t.Operations)
					{
						o.Executed = false;
						o.Error = null;
					}

			foreach(var b in round.ExecutingPayloads.Reverse())
			{
				foreach(var t in b.Transactions.AsEnumerable().Reverse())
				{
					Coin fee = 0;

					foreach(var o in t.Operations.AsEnumerable().Reverse())
					{
						var l = round.ChangeAccount(t.Signer).FindOperation<Operation>(round);
					
						if(/*l == null && o.Id != 0 ||*/ l != null && o.Id <= l.Id)
						{
							o.Error = "Non sequential";
							break;
						}
						
						o.Execute(this, round);
						o.Executed = true;

// 						if(o.Error != null)
// 							break;

						if(o.Error == null)
						{
							var f = o.CalculateFee(round.Factor);
	
							if(round.ChangeAccount(t.Signer).Balance - f >= 0)
							{
								fee += f;
								round.ChangeAccount(t.Signer).Balance -= f;
							}
							else
							{
								o.Error = Operation.NotEnoughUNT;
	//							break;
							}
						}
					}
						
					if(t.SuccessfulOperations.Any())
					{
						round.ChangeAccount(t.Signer).Transactions.Add(round.Id);
						round.Distribute(fee, new [] {b.Generator}, 9, round.Funds, 1); /// this way we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding 
					}
				}
			}


			if(round.Id > LastGenesisRound)
			{
				var penalty = Coin.Zero;

				if(blockforkers != null && blockforkers.Any())
				{
					foreach(var f in blockforkers)
					{
						penalty += Accounts.FindLastOperation<CandidacyDeclaration>(f, o => o.Successful, null, null, i => i.Id < round.Id).Bail;
						round.ChangeAccount(f).BailStatus = BailStatus.Siezed;
					}

					round.Distribute(penalty, round.Members.Where(i => !blockforkers.Contains(i.Generator)).Select(i => i.Generator), 1, round.Funds, 1);
				}
			}
			//ExecuteWithoutErrors(round, payloads, blockforkers);
		}

		public void Confirm(Round round)
		{
			if(round.Id > 0 && LastConfirmedRound.Id + 1 != round.Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == round.Id");

			List<T>	confirm<T>(IEnumerable<byte[]> prefixes, Func<Vote, IEnumerable<T>> get, Func<T, byte[]> getprefix)
			{
				var o = prefixes.Select(v => round.Unique.SelectMany(i => get(i)).FirstOrDefault(i => getprefix(i).SequenceEqual(v)));

				if(o.Contains(default(T)))
					throw new ConfirmationException("Can't confirm, some references not found", round);
				else 
					return o.ToList();
			}

			/// check we have all payload blocks 

			foreach(var i in round.Payloads)
				i.Confirmed = false;

			var child = FindRound(round.Id + Pitch);
			var rf = child.Majority.First().Reference;
 	
			foreach(var pf in rf.Payloads)
			{
				var b = round.Unique.FirstOrDefault(i => pf.SequenceEqual(i.Prefix));

				if(b != null)
					b.Confirmed = true;
				else
					throw new ConfirmationException("Can't confirm, missing blocks", round);
			}

			round.ConfirmedViolators	= confirm(rf.Violators,		i => i.Violators,	i => i.Prefix);
			round.ConfirmedJoiners		= confirm(rf.Joiners,		i => i.Joiners,		i => i.Prefix);
			round.ConfirmedLeavers		= confirm(rf.Leavers,		i => i.Leavers,		i => i.Prefix);
			round.ConfirmedFundJoiners	= confirm(rf.FundJoiners,	i => i.FundJoiners, i => i.Prefix);
			round.ConfirmedFundLeavers	= confirm(rf.FundLeavers,	i => i.FundLeavers, i => i.Prefix);
			round.Time					= rf.Time;

			round.Confirmed = true;

			Seal(round);
		}

		public void Seal(Round round)
		{
			Execute(round, round.ConfirmedPayloads, round.ConfirmedViolators);

			foreach(var b in round.Payloads)
				foreach(var t in b.Transactions)
					foreach(var o in t.Operations)
						o.Placing = b.Confirmed && o.Successful ? PlacingStage.Confirmed : PlacingStage.FailedOrNotFound;

			round.Seal();

			Members.AddRange(round.ConfirmedJoiners.Select(i =>	{
																	var d = Accounts.FindLastOperation<CandidacyDeclaration>(i, o => o.Successful, null, null, r => r.Id < round.Id);
																	return new Peer {Generator = i, IP = d.IP, JoinedGeneratorsAt = round.Id + Pitch};
																}));

			Members.RemoveAll(i => round.AnyOperation(o => o is CandidacyDeclaration d && d.Signer == i.Generator && o.Placing == PlacingStage.Confirmed));  /// CandidacyDeclaration cancels membership
			Members.RemoveAll(i => round.AffectedAccounts.ContainsKey(i.Generator) && round.AffectedAccounts[i.Generator].Bail < (Settings.Dev.DisableBailMin ? 0 : BailMin));  /// if Bail has exhausted due to penalties (CURRENTY NOT APPLICABLE, penalties are disabled)
			Members.RemoveAll(i => round.ConfirmedLeavers.Contains(i.Generator));
			Members.RemoveAll(i => round.ConfirmedViolators.Contains(i.Generator));

			if(round.Id <= LastGenesisRound || round.Factor == Emission.FactorEnd) /// reorganization only after emission is over
			{
				Funds.AddRange(round.ConfirmedFundJoiners);
				Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));
			}

			//if(Hubs.Count > HubsMax)
			//{
			//	Hubs.OrderByDescending(i => i.JoinedHubsAt).ThenBy(i => i.IP.GetAddressBytes(), new BytesComparer())
			//} 

 			round.Members	= Members.ToList();
 			round.Funds		= Funds.ToList();

			if(round.Id - Rounds.Last().Id > Pitch + Pitch + 1) /// keep last [Pitch] sealed rounds cause [LastSealed - Pitch] round may contain JoinRequests that are needed if a node is joining
			{
				var r = Rounds.Last();

				using(var b = new WriteBatch())
				{
					b.Put(LastRoundKey, BitConverter.GetBytes(r.Id));
					b.Put(WeiSpentKey,	r.WeiSpent.ToByteArray());
					b.Put(FactorKey,	r.Factor.Attos.ToByteArray());
					b.Put(EmissionKey,	r.Emission.Attos.ToByteArray());

					Accounts.Save(b, r.AffectedAccounts.Values);
					Authors.Save(b, r.AffectedAuthors.Values);
					Products.Save(b, r.AffectedProducts.Values);

					var s = new MemoryStream();
					var w = new BinaryWriter(s);
					w.Write(r.Members, i => i.WriteMember(w));
					b.Put(MembersKey, s.ToArray());

					s.SetLength(0);
					w.Write(r.Funds);
					b.Put(FundsKey, s.ToArray());

					foreach(var i in r.AffectedRounds)
					{
						s.SetLength(0);						
						i.Save(w);
						b.Put(BitConverter.GetBytes(i.Id), s.ToArray(), RoundsFamily);
					}

					s.SetLength(0);
					r.Save(w); /// may duplicate above if affected, not big deal
					b.Put(BitConverter.GetBytes(r.Id), s.ToArray(), RoundsFamily);

					Database.Write(b);
				}
				
				Rounds.Remove(r);
				
				/// to save RAM
				r.Blocks.RemoveAll(i => !i.Confirmed);
				r.Members = null;
				r.Funds = null;
				r.Hubs = null;
				r.AffectedAccounts = null;
				r.AffectedAuthors = null;
				r.AffectedProducts = null;

				r.LastAccessed	= DateTime.UtcNow;
				LoadedRounds[r.Id] = r;

				Recycle();
			}
		}


		public ProductEntry FindProduct(ProductAddress authorproduct, int ridmax)
		{
			foreach(var r in Rounds.Where(i => i.Id <= ridmax))
				if(r.AffectedProducts.ContainsKey(authorproduct))
					return r.AffectedProducts[authorproduct];

			var e = Products.FindEntry(authorproduct);

			if(e != null && (e.LastRegistration > ridmax))
				throw new IntegrityException("maxrid works inside pool only");

			return e;
		}

		public Transaction FindLastPoolTransaction(Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Rounds : Rounds.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in b.Transactions)
						if(transaction_predicate == null || transaction_predicate(t))
							return t;

			return null;
		}

		public IEnumerable<Transaction> FindLastPoolTransactions(Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Rounds : Rounds.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in transaction_predicate == null ? b.Transactions : b.Transactions.Where(transaction_predicate))
						yield return t;
		}

		public O FindLastPoolOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastPoolTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
		}

		IEnumerable<O> FindLastPoolOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastPoolTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops : ops.Where(op);
		}

		public Block FindLastBlock(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			foreach(var r in Rounds.Where(i => i.Id <= maxrid))
				foreach(var b in r.Blocks)
					if(f(b))
						return b;

			return null;
		}

		public IEnumerable<Block> FindLastBlocks(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			foreach(var r in Rounds.Where(i => i.Id <= maxrid))
				foreach(var b in r.Blocks)
					if(f(b))
						yield return b;
		}

		public IEnumerable<ReleaseManifest> FindReleases(string author, string product, Func<ReleaseManifest, bool> f, int maxrid = int.MaxValue)
		{
			throw new NotImplementedException();

///			return FindLastPoolOperations<Release>(i => i.Author == author && i.Product == product && f(i), rp: r => r.Id <= maxrid).Union(Products.FindReleases(author, product, f));
		}
		
		public AccountInfo GetAccountInfo(Account account, bool confirmed)
		{
			var roundmax = confirmed ? LastConfirmedRound : LastNonEmptyRound;

			var a = Accounts.Find(account, roundmax.Id);

			if(a != null)
			{
				var i = new AccountInfo();

				var t = Accounts.FindLastOperation(account, i => i.Successful);

				i.Balance			= a.Balance;
				i.LastOperationId	= t == null ? -1 : t.Id;
				i.Authors			= a.Authors;
				i.Operations		= Accounts.FindLastOperations<Operation>(account).Take(10).Reverse().Select(i => new AccountOperationInfo(i)).ToList();

				return i;
			}

			return null;
		}

		public XonDocument GetAuthorInfo(string author, bool confirmed, IXonValueSerializator serializator)
		{
			var roundmax = confirmed ? LastConfirmedRound : LastNonEmptyRound;

			var a = Authors.Find(author, roundmax.Id);

			if(a != null)
			{
				return a.ToXon(serializator);
			}

			return null;
		}
				
		public XonDocument QueryRelease(ReleaseQuery query, bool confirmed, IXonValueSerializator serializator)
		{
			if(query.VersionQuery == VersionQuery.Latest)
			{
				var roundmax = confirmed ? LastConfirmedRound : LastPayloadRound;
	
				var p = Products.Find(query, roundmax.Id);
	
				if(p != null)
				{
					var r = p.Releases.Find(i => i.Channel == query.Channel && i.Platform == query.Platform);
					
					if(r != null)
					{
						var prev = FindRound(r.Rid).FindOperation<ReleaseManifest>(m =>	m.Address.Author == query.Author && 
																						m.Address.Product == query.Product && 
																						m.Address.Platform == query.Platform && 
																						m.Channel == query.Channel);
	
						return prev.ToXon(serializator);
					}
				}

				return null;
			}

			if(query.VersionQuery == VersionQuery.Exact)
			{
				var roundmax = confirmed ? LastConfirmedRound : LastPayloadRound;
	
				var p = Products.Find(query, roundmax.Id);
	
				if(p != null)
				{
					var r = p.Releases.Find(i => i.Platform == query.Platform && i.Version == query.Version);
					
					if(r != null)
					{
						var prev = FindRound(r.Rid).FindOperation<ReleaseManifest>(m =>	m.Address == query);
	
						return prev.ToXon(serializator);
					}
				}

				return null;
			}

			throw new ArgumentException("Unknown VersionQuery");
		}
	}
}
