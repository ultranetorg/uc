using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class AccountOperationInfo
	{
		public string			Name {get; set;}
		public string			Decsription {get; set;}
		public string			Error {get; set;}
		public int				TransactionId {get; set;}
		public int				RoundId {get; set;}

		public AccountOperationInfo(){}

		public AccountOperationInfo(Operation o)
		{
			Name = o.GetType().Name;
			Decsription = o.Description;
			Error = o.Error;
			TransactionId = o.Id;
			RoundId = o.Transaction.Payload.RoundId;
		}
	}

	public class AccountInfo
	{
		public Coin									Balance {get; set;}
		//public int									LastOperationId {get; set;}
		public IEnumerable<string>					Authors {get; set;}
		public IEnumerable<AccountOperationInfo>	Operations {get; set;}

		class Item
		{
			public string Name;
			public object Value;
			public List<Item> Items = new();

			public Item()
			{
			}

			public Item(string name, object value)
			{
				Name = name;
				Value = value;
			}

			public Item Add(string name, object value)
			{
				Items.Add(new Item(name, value));
				return Items.Last();
			}

			public int MaxNameLength(int d, int inmax)
			{
				var l = (d * 4) + (Name != null ? Name.Length : 0);
				int max = l > inmax ? l : inmax;

				foreach(var i in Items)
				{
					max = i.MaxNameLength(d + 1, max);
				}

				return max;
			}
		}

		public void Dump(Action<int, int, string, object> top)
		{
			var root = new Item();

			root.Add("Balance",				Balance);
			//root.Add("Last Transaction Id",	LastOperationId);
			
			if(Authors != null)
			{
				root.Add("Authors", string.Join(',', Authors));
			}

			var ops = root.Add("Last Operations", null);

			foreach(var i in Operations)
			{
				ops.Add(i.Name, $"{i.Decsription} Tx={i.TransactionId} R={i.RoundId} Result={(i.Error ?? "OK")}");
			}

			var max = root.MaxNameLength(-1, 0);

			void dump(Item item, int d)
			{
				top(d, max, item.Name, item.Value);

				foreach(var i in item.Items)
				{
					dump(i, d + 1);
				}
			}

			foreach(var i in root.Items)
			{
				dump(i, 0);
			}
		}
	}
}
