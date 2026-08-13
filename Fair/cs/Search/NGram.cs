namespace Uccs.Fair;

using System;
using System.Buffers;
using System.Collections.Generic;
using Roaring.Net.CRoaring;

public class Ngram<ID> : IBinarySerializable, ITableEntry<ID> where ID : EntityId, new()
{
	public ID				Id { get; set; }
	public Roaring64Bitmap	Entities { get; set; }
	public bool				Deleted { get; set; }

	FairMcv					Mcv;

	public Ngram()
	{
	}

	~Ngram()
	{
		Entities.Dispose();
	}

	public Ngram(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {nameof(Entities)}={{{Entities.Count}}}";
	}

	public object Clone()
	{
		var a = new Ngram<ID>(Mcv)
				{
					Id = Id,
					Entities = Entities
				};

		return a;
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		
		Entities.Optimize(); 
		writer.WriteBytes(Entities.Serialize(SerializationFormat.Frozen));
	}

	public void Read(Reader reader)
	{
		Id = reader.Read<ID>();
		Entities = Roaring64Bitmap.Deserialize(reader.ReadBytes(), SerializationFormat.Frozen);
	}
}

public abstract class NgramTable<ID> : Table<ID, Ngram<ID>>, IDisposable where ID : EntityId, new()
{
    public readonly int            Q;
    public readonly bool		  Sorted;

    public abstract ID            CreateId(ulong ngramSpan, object more);

    public NgramTable(Mcv chain, string name, bool index, int q = 3, bool sorted = true) : base(chain, name, index)
    {
        if(q is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(q), "Q must be between 1 and 4 for ulong-packed key implementation.");

        Q = q;
        Sorted = sorted;
    }

    public override Ngram<ID> Create()
    {
        return new Ngram<ID>();
    }

    protected void CollectBitmapIfExists(ulong ngramSpan, object more, List<Roaring64Bitmap> list, Func<ID, Ngram<ID>> retrieve)
    {
        var e = retrieve(CreateId(ngramSpan, more));

        if(e != null)
        {
            list.Add(e.Entities);
        }
    }

	public Span<char> PadLessQ(ReadOnlySpan<char> span, Span<char> padded)
	{
		padded[0] = '^';
		span.CopyTo(padded[1..]);

		if(span.Length + 1 < Q)
		{
			padded[(span.Length + 1)..].Fill('_');
		}

		return padded;
	}

    public ulong[] Search(string query, object more, Func<ID, Ngram<ID>> retrieve, int skip, int take, int minMatchPercent = 30)
    {
        if(minMatchPercent is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(minMatchPercent), "Percent must be between 1 and 100");

        var span = query.AsSpan();
        var matchedBitmaps = new List<Roaring64Bitmap>(span.Length >= Q ? span.Length - Q + 1 : 1);

        // 1. Сбор совпавших битовых карт
        if(span.Length < Q)
		{
			Span<char> padded = stackalloc char[Q];
			PadLessQ(span, padded);
			CollectBitmapIfExists(PackNGram(padded), more, matchedBitmaps, retrieve);
		}
		else
        {
            for(int i = 0; i <= span.Length - Q; i++)
            {
                CollectBitmapIfExists(PackNGram(span.Slice(i, Q)), more, matchedBitmaps, retrieve);
            }
        }

        int totalNGrams = matchedBitmaps.Count;
        if(totalNGrams == 0)
            return Array.Empty<ulong>();

        // 2. Целочисленный расчёт порога (округление вверх)
        int minMatches = (totalNGrams * minMatchPercent + 99) / 100;
        if(minMatches < 1)
            minMatches = 1;

        // 3. Логика OR (нужно хотя бы 1 совпадение)
        if(minMatches == 1)
        {
            if(totalNGrams == 1)
                return matchedBitmaps[0].ToArray();

            int othersCount = totalNGrams - 1;
            var rented = ArrayPool<Roaring64Bitmap>.Shared.Rent(othersCount);

            try
            {
                for(int i = 0; i < othersCount; i++)
                {
                    rented[i] = matchedBitmaps[i + 1];
                }

                var exactArray = new Roaring64Bitmap[othersCount];
                Array.Copy(rented, exactArray, othersCount);

                using var unionResult = matchedBitmaps[0].OrMany(exactArray);
                
                return unionResult.ToArray();
            }
            finally
            {
                ArrayPool<Roaring64Bitmap>.Shared.Return(rented);
            }
        }

        // 4. Логика AND (сортировка in-place по .Count)
        matchedBitmaps.Sort(static (a, b) => a.Count.CompareTo(b.Count));

        var result = matchedBitmaps[0].Clone();

        for(int i = 1; i < minMatches; i++)
        {
            var nextResult = result.And(matchedBitmaps[i]);
            result.Dispose();
            result = nextResult;

            if(result.Count == 0)
            {
                result.Dispose();
                return Array.Empty<ulong>();
            }
        }


//    var candidateArray = result.ToArray();
//    var matchCounts = new Dictionary<ulong, int>(candidateArray.Length);
//
//    // Инициализируем словарь кандидатами
//    foreach (var id in candidateArray)
//    {
//        matchCounts[id] = 0;
//    }
//
//    // Итерируемся по исходным битмапам и инкрементируем счетчики только для кандидатов
//    foreach (var bitmap in matchedBitmaps)
//    {
//        // Пересекаем исходный битмап с кандидатами, чтобы итерироваться только по релевантным ID
//        using var intersection = result.And(bitmap);
//        
//        foreach (ulong id in intersection.Values)
//        {
//            matchCounts[id]++;
//        }
//    }
//
//    result.Dispose();
//
//
//return matchCounts
//        .OrderByDescending(kvp => kvp.Value)
//        .Select(kvp => kvp.Key)
//        .Skip(skip)
//        .Take(take)
//        .ToArray();

        try
        {
            return result.ToArray();
        }
        finally
        {
            result.Dispose();
        }
    }

	public ulong PackNGram(ReadOnlySpan<char> span)
    {
        Span<char> buffer = stackalloc char[span.Length];

        for(int i = 0; i < span.Length; i++)
        {
            buffer[i] = char.ToLowerInvariant(span[i]);
        }

        if(Sorted)
        {
            buffer.Sort();
        }

        ulong key = 0;
        for(int i = 0; i < buffer.Length; i++)
        {
            key = (key << 16) | buffer[i];
        }

        return key;
    }

    public void Dispose()
    {
        ///foreach (var bitmap in _index.Values)
        ///{
        ///    bitmap.Dispose();
        ///}
        ///_index.Clear();
    }

	public int ComputeDistance(string target, string query)
	{
		int d = 0;

		bool[]? rented = target.Length > 256 ? ArrayPool<bool>.Shared.Rent(target.Length) : null;
		Span<bool> visited = rented != null ? rented.AsSpan() : stackalloc bool[target.Length];

		visited.Clear();

		try
		{
			for(int i = 0; i < query.Length; i++)
			{
				int p = -1;
				int searchStart = 0;

				while(searchStart < target.Length)
				{
					int found = target.IndexOf(query[i], searchStart);
					
					if(found == -1)
						break;

					if(!visited[found])
					{
						p = found;
						break;
					}

					searchStart = found + 1;
				}

				if(p != -1)
				{
					visited[p] = true;
					d += Math.Abs(p - i);
				}
				else
				{
					d += Math.Abs(target.Length - i);
				}
			}

			return d;
		}
		finally
		{
			if(rented != null)
			{
				ArrayPool<bool>.Shared.Return(rented);
			}
		}
	}
}

public class NgramTableState<ID> : TableState<ID, Ngram<ID>, NgramTable<ID>> where ID : EntityId, new()
{
	public new NgramTable<ID> Table => base.Table as NgramTable<ID>;

	public NgramTableState(NgramTable<ID> table) : base(table)
	{
	}
}
