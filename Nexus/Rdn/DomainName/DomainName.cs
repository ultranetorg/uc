namespace Uccs.Rdn;

using System.Text.RegularExpressions;

public class DomainName : IBinarySerializable, ITableEntry<StringId>, IExpirable
{
	public const int				LengthMinimum = 1;
	public const int				LengthMaximum = 256;
	public const char				National = '~';
	public const char				Subdomain = '.';
	static readonly Regex			NameRegex = new ($@"^[a-z0-9\{Subdomain}]+[a-z0-9{Subdomain}{National}]*$", RegexOptions.Compiled);
	public static readonly string[] PriorityTlds = ["com", "org", "net", "info", "biz"];

	public StringId					Id { get; set; }
	public AutoId					Owner { get; set; }
	public AutoId					Domain { get; set; }

	public short					Expiration { get; set; }

	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public static bool				IsRoot(string name) => !name.Contains(Subdomain); 
	public static bool				IsSubdomain(string name) => name.Contains(Subdomain); 
	
	public static string			GetParent(string name) => name.Substring(name.IndexOf(Subdomain) + 1); 
	public static string			GetFirstName(string name) => name.Substring(0, name.IndexOf(Subdomain));

	public DomainName()
	{
	}

	public DomainName(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {nameof(Owner)}={Owner}, {nameof(Domain)}={Domain}, {nameof(Expiration)}={Expiration}";
	}

	public static bool IsValid(string name)
	{
		return  !string.IsNullOrWhiteSpace(name) &&
				name.Length is >= LengthMinimum and <= LengthMaximum &&
				NameRegex.Match(name).Success;
	}

	public object Clone()
	{
		var a = new DomainName(Mcv)
				{	
					Id				= Id,
					Owner			= Owner,
					Domain			= Domain,
				};

		(this as IExpirable).Copy(a);

		return a;
	}

	public void WriteMain(Writer writer)
	{
		writer.Write(Owner);
		writer.WriteNullable(Domain);

		(this as IExpirable).WriteExpirable(writer);
	}

	public void ReadMain(Reader reader)
	{
		Owner	= reader.Read<AutoId>();
		Domain	= reader.ReadNullable<AutoId>();

		(this as IExpirable).ReadExpirable(reader);
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		WriteMain(writer);
	}

	public void Read(Reader reader)
	{
		Id	= reader.Read<StringId>();
		ReadMain(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
			
	public static string GetRoot(string name)
	{
		var i = name.LastIndexOf(Subdomain);

		return i == -1 ? name : name.Substring(i + 1);
	}
}
