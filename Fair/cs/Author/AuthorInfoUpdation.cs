using System.Text;

namespace Uccs.Fair;

public enum AuthorField : byte
{
	None, Title, Description, Reference
}

public class AuthorInfoChange : IBinarySerializable
{
	public AuthorField	Field { get; set; }
	public string		X { get; set; }
	public string		Y { get; set; }

	public AuthorInfoChange()
	{
	}

	public AuthorInfoChange(AuthorField field, string value)
	{
		Field = field;
		X = value;
	}

	public AuthorInfoChange(AuthorField field, string x, string y) : this(field, x)
	{
		Y = y;
	}

	public void Read(Reader reader)
	{
		Field = reader.Read<AuthorField>();
		X = reader.ReadUtf8();
		Y = reader.ReadUtf8();
	}

	public void Write(Writer writer)
	{
		writer.Write(Field);
		writer.WriteUtf8(X);
		writer.WriteUtf8(Y);
	}
}


public class AuthorInfoUpdation : FairOperation
{
	public AutoId				Author { get; set; }
	public AuthorInfoChange[]	Changes { get; set; }

	public override string		Explanation => $"Author={Author}, Changes={Changes.Length}";
	
	const int					ChangesMaximum = 30;

	public override void Read(Reader reader)
	{
		Author		= reader.Read<AutoId>();
		Changes		= reader.ReadArray<AuthorInfoChange>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Author);
		writer.Write(Changes);
	}

	public override bool IsValid(McvNet net)
	{
		if(Changes.Length > ChangesMaximum)
			return false;

		if(Changes.Count(i => i.Field == AuthorField.Title) > 1)
			return false;

		if(Changes.Count(i => i.Field == AuthorField.Description) > 1)
			return false;

		foreach(var i in Changes)
		{
			switch(i.Field)
			{
				case AuthorField.Title:
					if(i.X.Length > Fair.TitleLengthMaximum)
						return false;

					break;

				case AuthorField.Description:
					if(i.X.Length > Fair.PostLengthMaximum)
						return false;

					break;

				case AuthorField.Reference:
					if(i.X.Length > Fair.TitleLengthMaximum)
						return false;

					if(i.Y?.Length > Uccs.Fair.Author.WeblinkLength)
						return false;
					break;
			}
		}

		return Changes.Any();
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		a = execution.Authors.Affect(Author);

		foreach(var i in Changes)
		{
			switch(i.Field)
			{
				case AuthorField.Title:
					execution.Free(a, a, Encoding.UTF8.GetByteCount(a.Title));

					a.Title = i.X;

					execution.Allocate(a, a, Encoding.UTF8.GetByteCount(a.Title));
					break;

				case AuthorField.Description:
					if(a.Description != null)
						execution.Free(a, a, Encoding.UTF8.GetByteCount(a.Description));

					a.Description = i.X;
					execution.Allocate(a, a, Encoding.UTF8.GetByteCount(i.X));
					break;
			}
		}


		if(Changes.Any(i => i.Field == AuthorField.Reference))
		{
			if(a.Referrences.Length > 0)
				execution.Free(a, a, a.Referrences.Sum(i => Encoding.UTF8.GetByteCount(i.Text) + Encoding.UTF8.GetByteCount(i.Uri)));

			a.Referrences = Changes.Where(i => i.Field == AuthorField.Reference).Select(i => new AuthorReference(i.X, i.Y)).ToArray();

			execution.Allocate(a, a, a.Referrences.Sum(i => Encoding.UTF8.GetByteCount(i.Text) + Encoding.UTF8.GetByteCount(i.Uri)));
		}

		execution.PayOperationEnergy(a);
	}
}