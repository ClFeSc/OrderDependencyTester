using Attribute = OrderDependencyModels.Attribute;

namespace DependencyTester;

// Source: https://github.com/SchweizerischeBundesbahnen/BCNFStar/blob/main/frontend/src/model/schema/ColumnsTree.ts
public class ColumnsTree<T>
{
    private Dictionary<Attribute, ColumnsTree<T>> _children = new();
    private T? _content;
    
    /** Helper to cleanup sort functions*/
    private static int SortResult(string a, string b)
    {
        return StringComparer.InvariantCulture.Compare(a, b);
    }

    /** Helper to sort an array of columns. The sorted array is used to have an
    * unambiguous path through the tree. Should check for all fields referenced in Column.equals
        */
    private static int CompareColumns(Attribute c1, Attribute c2) => SortResult(c1.Name, c2.Name);

    /**
   * @returns Columns sorted to be a path along the tree
   */
    private static Attribute[] SortedColumns(HashSet<Attribute> cc)
    {
        var sorted = cc.ToArray();
        Array.Sort(sorted, CompareColumns);
        return sorted;
    }

    /**
   * Adds a value to the tree
   * @param content the value to be set
   * @param columns the location to store the value at
   */
    public void Add(T content, HashSet<Attribute> columns)
    {
        Traverse(columns)._content = content;
    }

    /**
   * @returns the value stored at `columns`, or undefined if nothing is there
   */
    public T? Get(HashSet<Attribute> columns) => Traverse(columns)._content;

    private ColumnsTree<T> Traverse(HashSet<Attribute> columns)
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

    /**
   * @returns all values stored in the tree for a subset of the given `columns`
   */
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

    /**
   * @returns a ColumnsTree containing only values stored at a subset of `columns`
   */
    private ColumnsTree<T> GetSubtree(HashSet<Attribute> columns)
    {
        var newTree = new ColumnsTree<T>();
        newTree._content = _content;
        foreach (var (column, subtree) in SortedEntries())
        {
            if (!columns.Contains(column)) continue;
            var newColumns = new HashSet<Attribute>(columns);
            newColumns.Remove(column);
            newTree._children.Add(column, subtree.GetSubtree(newColumns));
        }

        return newTree;
    }

    /** returns all values stored in this tree */
    public List<T> GetAll()
    {
        var result = SortedEntries().SelectMany(pair => pair.Value.GetAll()).ToList();
        if (_content is not null) result.Add(_content);
        return result;
    }
}