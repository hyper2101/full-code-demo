using System;
using System.Collections.Generic;

[Serializable]
public class Graph
{
	public Node AddNode(string id, string name)
	{
		Node node = new Node
		{
			id = id,
			name = name
		};
		this.nodes.Add(node);
		return node;
	}

	private bool LinkExists(string source, string target)
	{
		foreach (Link link in this.links)
		{
			if (link.source == source && link.target == target)
			{
				return true;
			}
		}
		return false;
	}

	public void AddLink(string source, string target)
	{
		if (this.LinkExists(source, target))
		{
			return;
		}
		Link link = new Link
		{
			source = source,
			target = target
		};
		this.links.Add(link);
	}

	public List<Node> nodes = new List<Node>();

	public List<Link> links = new List<Link>();
}
