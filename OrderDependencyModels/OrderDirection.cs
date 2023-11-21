namespace OrderDependencyModels;

public enum OrderDirection
{
    Ascending,
    Descending,
}

public static class OrderDirectionHelper
{
    public static string ToString(OrderDirection orderDirection) =>
        orderDirection is OrderDirection.Ascending ? "↑" : "↓";
}
