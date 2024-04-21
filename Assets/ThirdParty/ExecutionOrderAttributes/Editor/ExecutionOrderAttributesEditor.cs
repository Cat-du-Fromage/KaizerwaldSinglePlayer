using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.Serialization;

public static class ExecutionOrderAttributeEditor
{
	private struct ScriptExecutionOrderDefinition
	{
		public MonoScript Script;
		public int Order;

		public ScriptExecutionOrderDefinition(MonoScript script, int order)
		{
			Script = script;
			Order = order;
		}
	}

	private struct ScriptExecutionOrderDependency
	{
		public MonoScript FirstScript;
		public MonoScript SecondScript;
		public int OrderDelta;

		public ScriptExecutionOrderDependency(MonoScript firstScript, MonoScript secondScript, int orderDelta)
		{
			FirstScript = firstScript;
			SecondScript = secondScript;
			OrderDelta = orderDelta;
		}
	}
	
	private static class Graph
	{
		public struct Edge
		{
			public MonoScript Node;
			public int Weight;

			public Edge(MonoScript node, int weight)
			{
				Node = node;
				Weight = weight;
			}
		}

		public static Dictionary<MonoScript, List<Edge>> Create(List<ScriptExecutionOrderDefinition> definitions, List<ScriptExecutionOrderDependency> dependencies)
		{
			Dictionary<MonoScript, List<Edge>> graph = new Dictionary<MonoScript, List<Edge>>();
			foreach(ScriptExecutionOrderDependency dependency in dependencies)
			{
				MonoScript source = dependency.FirstScript;
				MonoScript dest = dependency.SecondScript;
				
				if(!graph.TryGetValue(source, out List<Edge> edges))
				{
					edges = new List<Edge>();
					graph.Add(source, edges);
				}
				edges.Add(new Edge(dest, dependency.OrderDelta));
				
				if(!graph.ContainsKey(dest))
				{
					graph.Add(dest, new List<Edge>());
				}
			}

			foreach(ScriptExecutionOrderDefinition definition in definitions)
			{
				graph.TryAdd(definition.Script, new List<Edge>());
			}
			return graph;
		}

		private static bool IsCyclicRecursion(Dictionary<MonoScript, List<Edge>> graph, MonoScript node, Dictionary<MonoScript, bool> visited, Dictionary<MonoScript, bool> inPath)
		{
			if (visited[node]) return inPath[node];
			visited[node] = true;
			inPath[node] = true;
			foreach(Edge edge in graph[node])
			{
				if (!IsCyclicRecursion(graph, edge.Node, visited, inPath)) continue;
				inPath[node] = false;
				return true;
			}
			inPath[node] = false;
			return false;
		}

		public static bool IsCyclic(Dictionary<MonoScript, List<Edge>> graph)
		{
			Dictionary<MonoScript, bool> visited = new Dictionary<MonoScript, bool>();
			Dictionary<MonoScript, bool> inPath = new Dictionary<MonoScript, bool>();
			foreach(MonoScript node in graph.Keys)
			{
				visited.Add(node, false);
				inPath.Add(node, false);
			}

			return graph.Keys.Any(node => IsCyclicRecursion(graph, node, visited, inPath));
		}

		public static List<MonoScript> GetRoots(Dictionary<MonoScript, List<Edge>> graph)
		{
			/*
			Dictionary<MonoScript, int> degrees = new Dictionary<MonoScript, int>();
			foreach(MonoScript node in graph.Keys)
			{
				degrees.Add(node, 0);
			}
			*/
			Dictionary<MonoScript, int> degrees = graph.Keys.ToDictionary(node => node, _ => 0);
			foreach(List<Edge> edges in graph.Values)
			{
				foreach(Edge edge in edges)
				{
					degrees[edge.Node]++;
				}
			}

			List<MonoScript> roots = new List<MonoScript>();
			foreach((MonoScript node, int degree) in degrees)
			{
				if (degree != 0) continue; 
				roots.Add(node);
			}
			return roots;
		}

		public static void PropagateValues(Dictionary<MonoScript, List<Edge>> graph, Dictionary<MonoScript, int> values)
		{
			Queue<MonoScript> queue = new Queue<MonoScript>(values.Keys.Count);
			foreach (MonoScript node in values.Keys)
			{
				queue.Enqueue(node);
			}
			
			while(queue.Count > 0)
			{
				MonoScript node = queue.Dequeue();
				int currentValue = values[node];
				foreach(Edge edge in graph[node])
				{
					int newValue = currentValue + edge.Weight;
					bool hasPrevValue = values.TryGetValue(edge.Node, out int prevValue);
					bool newValueBeyond = (edge.Weight > 0) ? (newValue > prevValue) : (newValue < prevValue);
					if (hasPrevValue && !newValueBeyond) continue;
					values[edge.Node] = newValue;
					queue.Enqueue(edge.Node);
				}
			}
		}
	}
	
	private static Dictionary<Type, MonoScript> GetTypeDictionary()
	{
		Dictionary<Type, MonoScript> types = new Dictionary<Type, MonoScript>();
		MonoScript[] scripts = MonoImporter.GetAllRuntimeMonoScripts();
		foreach(MonoScript script in scripts)
		{
			if (!TryGetValidType(script, out Type type)) continue;
			types.TryAdd(type, script);
		}
		return types;
	}

	private static bool TryGetValidType(MonoScript script, out Type type)
	{
		type = script.GetClass();
		return type != null && (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject)));
	}

	private static bool IsTypeValid(Type type)
	{
		return type != null && (type.IsSubclassOf(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(ScriptableObject)));
	}

	private static List<ScriptExecutionOrderDependency> GetExecutionOrderDependencies(Dictionary<Type, MonoScript> types)
	{
		List<ScriptExecutionOrderDependency> list = new List<ScriptExecutionOrderDependency>();

		foreach((Type type, MonoScript script) in types)
		{
			bool hasExecutionOrderAttribute = Attribute.IsDefined(type, typeof(ExecutionOrderAttribute));
			bool hasExecuteAfterAttribute   = Attribute.IsDefined(type, typeof(ExecuteAfterAttribute));
			bool hasExecuteBeforeAttribute  = Attribute.IsDefined(type, typeof(ExecuteBeforeAttribute));

			if(hasExecuteAfterAttribute)
			{
				if(hasExecutionOrderAttribute)
				{
					Debug.LogError(string.Format("Script {0} has both [ExecutionOrder] and [ExecuteAfter] attributes. Ignoring the [ExecuteAfter] attribute.", script.name), script);
					continue;
				}

				ExecuteAfterAttribute[] attributes = (ExecuteAfterAttribute[])Attribute.GetCustomAttributes(type, typeof(ExecuteAfterAttribute));
				foreach(ExecuteAfterAttribute attribute in attributes)
				{
					if(attribute.OrderIncrease < 0)
					{
						Debug.LogError(string.Format("Script {0} has an [ExecuteAfter] attribute with a negative orderIncrease. Use the [ExecuteBefore] attribute instead. Ignoring this [ExecuteAfter] attribute.", script.name), script);
						continue;
					}

					if(!attribute.TargetType.IsSubclassOf(typeof(MonoBehaviour)) && !attribute.TargetType.IsSubclassOf(typeof(ScriptableObject)))
					{
						Debug.LogError(string.Format("Script {0} has an [ExecuteAfter] attribute with targetScript={1} which is not a MonoBehaviour nor a ScriptableObject. Ignoring this [ExecuteAfter] attribute.", script.name, attribute.TargetType.Name), script);
						continue;
					}

					MonoScript targetScript = types[attribute.TargetType];
					//ScriptExecutionOrderDependency dependency = new () { FirstScript = targetScript, SecondScript = script, OrderDelta = attribute.orderIncrease };
					list.Add(new ScriptExecutionOrderDependency(targetScript, script, attribute.OrderIncrease));
				}
			}

			if (hasExecuteBeforeAttribute)
			{
				if(hasExecutionOrderAttribute)
				{
					Debug.LogError(string.Format("Script {0} has both [ExecutionOrder] and [ExecuteBefore] attributes. Ignoring the [ExecuteBefore] attribute.", script.name), script);
					continue;
				}

				if(hasExecuteAfterAttribute)
				{
					Debug.LogError(string.Format("Script {0} has both [ExecuteAfter] and [ExecuteBefore] attributes. Ignoring the [ExecuteBefore] attribute.", script.name), script);
					continue;
				}

				ExecuteBeforeAttribute[] attributes = (ExecuteBeforeAttribute[])Attribute.GetCustomAttributes(type, typeof(ExecuteBeforeAttribute));
				foreach(ExecuteBeforeAttribute attribute in attributes)
				{
					if(attribute.OrderDecrease < 0)
					{
						Debug.LogError(string.Format("Script {0} has an [ExecuteBefore] attribute with a negative orderDecrease. Use the [ExecuteAfter] attribute instead. Ignoring this [ExecuteBefore] attribute.", script.name), script);
						continue;
					}

					if(!attribute.TargetType.IsSubclassOf(typeof(MonoBehaviour)) && !attribute.TargetType.IsSubclassOf(typeof(ScriptableObject)))
					{
						Debug.LogError(string.Format("Script {0} has an [ExecuteBefore] attribute with targetScript={1} which is not a MonoBehaviour nor a ScriptableObject. Ignoring this [ExecuteBefore] attribute.", script.name, attribute.TargetType.Name), script);
						continue;
					}
					MonoScript targetScript = types[attribute.TargetType];
					//ScriptExecutionOrderDependency dependency = new (targetScript, script, -attribute.orderDecrease);
					list.Add(new ScriptExecutionOrderDependency(targetScript, script, -attribute.OrderDecrease));
				}
			}
		}

		return list;
	}

	private static List<ScriptExecutionOrderDefinition> GetExecutionOrderDefinitions(Dictionary<Type, MonoScript> types)
	{
		List<ScriptExecutionOrderDefinition> list = new List<ScriptExecutionOrderDefinition>();
		foreach((Type type, MonoScript script) in types)
		{
			if (!Attribute.IsDefined(type, typeof(ExecutionOrderAttribute))) continue;
			ExecutionOrderAttribute attribute = (ExecutionOrderAttribute)Attribute.GetCustomAttribute(type, typeof(ExecutionOrderAttribute));
			list.Add(new ScriptExecutionOrderDefinition(script, attribute.Order));
		}
		return list;
	}

	private static Dictionary<MonoScript, int> GetInitialExecutionOrder(List<ScriptExecutionOrderDefinition> definitions, List<MonoScript> graphRoots)
	{
		Dictionary<MonoScript, int> orders = definitions.ToDictionary(definition => definition.Script, definition => definition.Order);
		foreach(MonoScript script in graphRoots)
		{
			orders.TryAdd(script, MonoImporter.GetExecutionOrder(script));
		}
		return orders;
	}

	private static void UpdateExecutionOrder(Dictionary<MonoScript, int> orders)
	{
		bool startedEdit = false;
		foreach((MonoScript script, int order) in orders)
		{
			if (MonoImporter.GetExecutionOrder(script) == order) continue;
			if(!startedEdit)
			{
				AssetDatabase.StartAssetEditing();
				startedEdit = true;
			}
			MonoImporter.SetExecutionOrder(script, order);
		}
		
		if(startedEdit)
		{
			AssetDatabase.StopAssetEditing();
		}
	}

	[UnityEditor.Callbacks.DidReloadScripts]
	private static void OnDidReloadScripts()
	{
		Dictionary<Type, MonoScript> types = GetTypeDictionary();
		List<ScriptExecutionOrderDefinition> definitions = GetExecutionOrderDefinitions(types);
		List<ScriptExecutionOrderDependency> dependencies = GetExecutionOrderDependencies(types);
		Dictionary<MonoScript, List<Graph.Edge>> graph = Graph.Create(definitions, dependencies);

		if(Graph.IsCyclic(graph))
		{
			Debug.LogError("Circular script execution order definitions");
			return;
		}

		List<MonoScript> roots = Graph.GetRoots(graph);
		Dictionary<MonoScript, int> orders = GetInitialExecutionOrder(definitions, roots);
		Graph.PropagateValues(graph, orders);

		UpdateExecutionOrder(orders);
	}
}
