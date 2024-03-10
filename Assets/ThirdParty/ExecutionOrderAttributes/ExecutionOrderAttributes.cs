using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ExecutionOrderAttribute : System.Attribute
{
	public int Order;

	public ExecutionOrderAttribute(int order)
	{
		this.Order = order;
	}
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ExecuteAfterAttribute : System.Attribute
{
	public Type TargetType;
	public int OrderIncrease;

	public ExecuteAfterAttribute(Type targetType)
	{
		this.TargetType = targetType;
		this.OrderIncrease = 10;
	}
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class ExecuteBeforeAttribute : System.Attribute
{
	public Type TargetType;
	public int OrderDecrease;

	public ExecuteBeforeAttribute(Type targetType)
	{
		this.TargetType = targetType;
		this.OrderDecrease = 10;
	}
}
