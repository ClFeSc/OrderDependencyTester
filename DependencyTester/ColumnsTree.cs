using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTester;

// Source: https://github.com/SchweizerischeBundesbahnen/BCNFStar/blob/main/frontend/src/model/schema/ColumnsTree.ts
/// <summary>
/// A prefix tree for sets of <see cref="Attribute">Attributes</see>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ColumnsTree<T>
{
    private readonly Dictionary<Attribute, ColumnsTree<T>> _children = new();
    private T? _content;

    /// <summary>
    /// Helper to cleanup sort functions.
    /// </summary>
    private static int SortResult(string a, string b) => StringComparer.InvariantCulture.Compare(a, b);

    /// <summary>
    /// Helper to sort an array of columns. The sorted array is used to have an
    /// unambiguous path through the tree. Should check for all fields referenced in Column.equals
    /// </summary>
    private static int CompareColumns(Attribute c1, Attribute c2) => SortResult(c1.Name, c2.Name);

    /// <returns>
    /// Columns sorted to be a path along the tree.
    /// </returns>
    private static IEnumerable<Attribute> SortedColumns(IEnumerable<Attribute> cc)
    {
        var sorted = cc.ToArray();
        Array.Sort(sorted, CompareColumns);
        return sorted;
    }

    /// <summary>
    /// Adds a value to the tree.
    ///
    /// <param name="content">The value to be set.</param>
    /// <param name="columns">The location to store the values at.</param>
    /// </summary>
    public void Add(T content, IEnumerable<Attribute> columns)
    {
        Traverse(columns)._content = content;
    }

    /// <returns>
    /// The value stored at <paramref name="columns"/>, or `null` if nothing is there.
    /// </returns>
    public T? Get(IEnumerable<Attribute> columns) => Traverse(columns)._content;

    private ColumnsTree<T> Traverse(IEnumerable<Attribute> columns)
    {
        var current = this;
        foreach (var column in SortedColumns(columns))
        {
            current._children.TryAdd(column, new ColumnsTree<T>());
            current = current._children[column];
        }

        return current;
    }

    private List<KeyValuePair<Attribute, ColumnsTree<T>>> SortedEntries()
    {
        var entries = new List<KeyValuePair<Attribute, ColumnsTree<T>>>(_children);
        entries.Sort((pair, valuePair) => CompareColumns(pair.Key, valuePair.Key));
        return entries;
    }

    /// <returns>
    /// All values stored in the tree for a subset of the given <paramref name="columns"/>.
    /// </returns>
    public List<T> GetSubsets(HashSet<Attribute> columns)
    {
        var result = SortedEntries().SelectMany<KeyValuePair<Attribute, ColumnsTree<T>>, T>(pair =>
        {
            var column = pair.Key;
            var lhsTree = pair.Value;
            if (!columns.Contains(column) || columns.Count == 0) return Array.Empty<T>();
            var newLhs = new HashSet<Attribute>(columns);
            newLhs.Remove(column);
            return lhsTree.GetSubsets(newLhs);
        }).ToList();
        if (_content is not null) result.Add(_content);
        return result;
    }

    /// <returns>
    /// A ColumnsTree containing only values stored at a subset of <paramref name="columns"/>.
    /// </returns>
    private ColumnsTree<T> GetSubtree(IReadOnlySet<Attribute> columns)
    {
        var newTree = new ColumnsTree<T>
        {
            _content = _content
        };
        foreach (var (column, subtree) in SortedEntries())
        {
            if (!columns.Contains(column)) continue;
            var newColumns = new HashSet<Attribute>(columns);
            newColumns.Remove(column);
            newTree._children.Add(column, subtree.GetSubtree(newColumns));
        }

        return newTree;
    }

    /// <returns>
    /// All values stored in this tree.
    /// </returns>
    public List<T> GetAll()
    {
        var result = SortedEntries().SelectMany(pair => pair.Value.GetAll()).ToList();
        if (_content is not null) result.Add(_content);
        return result;
    }
}
