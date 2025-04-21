
using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public static class SimHashUtil
{
    public static ulong ComputeSimHash(string input)
    {
        int[] bits = new int[64];
        var tokens = input.Split(new[] { ' ', '.', ',', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            ulong hash = Fnv1aHash64(token.ToLowerInvariant());
            for (int i = 0; i < 64; i++)
                bits[i] += ((hash >> i) & 1UL) == 1UL ? 1 : -1;
        }

        ulong simHash = 0;
        for (int i = 0; i < 64; i++)
            if (bits[i] > 0)
                simHash |= (1UL << i);

        return simHash;
    }

    public static int HammingDistance(ulong a, ulong b)
    {
        ulong x = a ^ b;
        int dist = 0;
        while (x != 0)
        {
            dist += (int)(x & 1);
            x >>= 1;
        }
        return dist;
    }

    private static ulong Fnv1aHash64(string text)
    {
        const ulong FNV_OFFSET_BASIS = 14695981039346656037UL;
        const ulong FNV_PRIME = 1099511628211UL;

        ulong hash = FNV_OFFSET_BASIS;
        foreach (var c in Encoding.UTF8.GetBytes(text))
        {
            hash ^= c;
            hash *= FNV_PRIME;
        }

        return hash;
    }
}


public class HnswNode
{
	public string data { get; }
	public ulong hash { get; }
	public int level { get; }
	public Dictionary<int, List<HnswNode>> connections { get; }

	public HnswNode(string data, int level)
	{
		this.data = data;
		this.level = level;
		hash = SimHashUtil.ComputeSimHash(data);
		connections = new Dictionary<int, List<HnswNode>>();
	}

	public void AddConnection(int level, HnswNode node)
	{
		if(!connections.ContainsKey(level))
			connections[level] = new List<HnswNode>();
		connections[level].Add(node);
	}
}

public class HnswTable
{
	private readonly Random random = new();
	private readonly int maxLevel;
	private readonly int maxConnections;
	private readonly int efConstruction;
	private readonly int hammingThreshold;
	private readonly int minDiversity;

	private readonly Dictionary<int, List<HnswNode>> layers = new();
	private readonly List<HnswNode> entryPoints = new();

	public HnswTable(int maxLevel = 5, int maxConnections = 5, int efConstruction = 64, int hammingThreshold = 10, int minDiversity = 10)
	{
		this.maxLevel = maxLevel;
		this.maxConnections = maxConnections;
		this.efConstruction = efConstruction;
		this.hammingThreshold = hammingThreshold;
		this.minDiversity = minDiversity;
	}

	public void Add(string data)
	{
		int level = RandomLevel();
		var newNode = new HnswNode(data, level);

		for(int l = 0; l <= level; l++)
		{
			if(!layers.ContainsKey(l))
				layers[l] = new List<HnswNode>();

			var neighbors = entryPoints.Count > 0
				? EfSearch(newNode.hash, entryPoints[0], l, efConstruction)
				: new();

			if(neighbors.Count == 0)
			{
				var fallback = GlobalBestNeighbor(newNode, l);
				if(fallback != null)
					neighbors.Add((fallback, SimHashUtil.HammingDistance(newNode.hash, fallback.hash)));
			}

			var topNeighbors = ApplyEFHeuristic(neighbors, maxConnections);

			foreach(var neighbor in topNeighbors)
			{
				newNode.AddConnection(l, neighbor);
				neighbor.AddConnection(l, newNode);
			}

			layers[l].Add(newNode);
		}

		if(entryPoints.Count == 0 || level > entryPoints[0].level)
		{
			entryPoints.Clear();
			entryPoints.Add(newNode);
		}
	}

	public void Remove(string data)
	{
		bool removedEntry = false;

		foreach(var level in layers.Keys.ToList())
		{
			var node = layers[level].FirstOrDefault(n => n.data == data);
			if(node != null)
			{
				foreach(var conn in node.connections.GetValueOrDefault(level, new()))
					conn.connections[level]?.RemoveAll(n => n.data == data);

				layers[level].Remove(node);

				if(layers[level].Count == 0)
					layers.Remove(level);

				if(entryPoints.Contains(node))
					removedEntry = true;
			}
		}

		if(removedEntry)
		{
			var all = layers.SelectMany(kvp => kvp.Value).ToList();
			if(all.Any())
			{
				var newEntry = all.OrderByDescending(n => n.level).First();
				entryPoints.Clear();
				entryPoints.Add(newEntry);
			}
			else
			{
				entryPoints.Clear();
			}
		}
	}

	public void Rebuild()
	{
		Console.WriteLine("[HNSW] Rebuilding graph...");
		var allData = layers
			.SelectMany(kv => kv.Value)
			.Select(n => n.data)
			.Distinct()
			.ToList();

		layers.Clear();
		entryPoints.Clear();

		foreach(var item in allData)
			Add(item);

		Console.WriteLine($"[HNSW] Rebuild complete. Total nodes: {allData.Count}");
	}

	public List<(string, int)> Search(string query, int k, int efSearch = 32)
	{
		if(entryPoints.Count == 0) return new();

		ulong queryHash = SimHashUtil.ComputeSimHash(query);
		HnswNode current = entryPoints[0];

		for(int level = current.level; level >= 1; level--)
			current = SearchBestNeighbor(queryHash, current, level);

		var resultNodes = EfSearch(queryHash, current, 0, efSearch);

		return resultNodes
			.OrderBy(x => x.dist)
			.Take(k)
			.Select(x => (x.node.data, x.dist))
			.ToList();
	}

	private List<(HnswNode node, int dist)> EfSearch(ulong queryHash, HnswNode entry, int level, int ef)
	{
		var visited = new HashSet<HnswNode>();
		var candidates = new PriorityQueue<HnswNode, int>();
		var topCandidates = new SortedList<int, HnswNode>();

		int entryDist = SimHashUtil.HammingDistance(queryHash, entry.hash);
		candidates.Enqueue(entry, entryDist);
		visited.Add(entry);
		topCandidates[entryDist] = entry;

		while(candidates.Count > 0 && topCandidates.Count < ef)
		{
			candidates.TryDequeue(out var current, out _);

			foreach(var neighbor in current.connections.GetValueOrDefault(level, new()))
			{
				if(visited.Contains(neighbor)) continue;
				visited.Add(neighbor);

				int dist = SimHashUtil.HammingDistance(queryHash, neighbor.hash);

				if(!topCandidates.ContainsKey(dist))
					topCandidates[dist] = neighbor;

				candidates.Enqueue(neighbor, dist);

				while(topCandidates.Count > ef)
					topCandidates.RemoveAt(topCandidates.Count - 1);
			}
		}

		return topCandidates.Select(kvp => (kvp.Value, kvp.Key)).ToList();
	}

	private List<HnswNode> ApplyEFHeuristic(List<(HnswNode node, int dist)> candidates, int maxConnections)
	{
		var selected = new List<HnswNode>();

		foreach(var (candidate, _) in candidates.OrderBy(x => x.dist))
		{
			bool good = true;

			foreach(var existing in selected)
			{
				int d = SimHashUtil.HammingDistance(candidate.hash, existing.hash);
				if(d < minDiversity)
				{
					good = false;
					break;
				}
			}

			if(good)
			{
				selected.Add(candidate);
				if(selected.Count >= maxConnections)
					break;
			}
		}

		if(selected.Count < maxConnections)
		{
			foreach(var (node, _) in candidates)
			{
				if(!selected.Contains(node))
				{
					selected.Add(node);
					if(selected.Count >= maxConnections)
						break;
				}
			}
		}

		return selected;
	}

	private HnswNode? GlobalBestNeighbor(HnswNode node, int level)
	{
		if(!layers.ContainsKey(level)) return null;

		int bestDist = int.MaxValue;
		HnswNode? best = null;

		foreach(var n in layers[level])
		{
			int dist = SimHashUtil.HammingDistance(node.hash, n.hash);
			if(dist < bestDist)
			{
				bestDist = dist;
				best = n;
			}
		}

		return best;
	}

	private HnswNode SearchBestNeighbor(ulong queryHash, HnswNode start, int level)
	{
		var visited = new HashSet<HnswNode>();
		var queue = new Queue<HnswNode>();
		queue.Enqueue(start);

		HnswNode best = start;
		int bestDist = SimHashUtil.HammingDistance(queryHash, best.hash);

		while(queue.Count > 0)
		{
			var current = queue.Dequeue();
			visited.Add(current);

			foreach(var neighbor in current.connections.GetValueOrDefault(level, new()))
			{
				if(!visited.Contains(neighbor))
				{
					int dist = SimHashUtil.HammingDistance(queryHash, neighbor.hash);
					if(dist < bestDist)
					{
						best = neighbor;
						bestDist = dist;
						queue.Enqueue(neighbor);
					}
				}
			}
		}

		return best;
	}

	private int RandomLevel()
	{
		double p = 1.0 / Math.E;
		int level = 0;
		while(random.NextDouble() < p && level < maxLevel)
			level++;
		return level;
	}
}
