using BitSets;

namespace DependencyTester;

// Source: https://github.com/SchweizerischeBundesbahnen/BCNFStar/blob/main/frontend/src/model/schema/ColumnsTree.ts
/// <summary>
/// A prefix tree for sets of <see cref="Attribute">Attributes</see>.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TBitSet"></typeparam>
public class ColumnsTree<T, TBitSet> where TBitSet : IBitSet<TBitSet>
{
    private readonly Dictionary<int, ColumnsTree<T, TBitSet>> _children = new();
    private HashSet<T>? _content;

    private int _numAttributes;

    public ColumnsTree(int numAttributes)
    {
        _numAttributes = numAttributes;
    }

    /// <summary>
    /// Adds a value to the tree.
    ///
    /// <param name="content">The value to be set.</param>
    /// <param name="columns">The location to store the values at.</param>
    /// </summary>
    public void Add(T content, TBitSet columns)
    {
        var node = Traverse(columns);
        if (node._content == null) node._content = new();
        node._content.Add(content);
    }

    /// <returns>
    /// The value stored at <paramref name="columns"/>, or `null` if nothing is there.
    /// </returns>
    public HashSet<T>? Get(TBitSet columns) => Traverse(columns)._content;

    private ColumnsTree<T, TBitSet> Traverse(TBitSet columns)
    {
        var current = this;
        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i] == false) continue;
            current._children.TryAdd(i, new ColumnsTree<T, TBitSet>(_numAttributes));
            current = current._children[i];
        }
        return current;
    }

    /// <returns>
    /// All values stored in the tree for a subset of the given <paramref name="columns"/>.
    /// </returns>
    public List<T> GetSubsets(TBitSet columns)
    {
        var result = _children.Where(item => columns.Get(item.Key)).SelectMany((item) =>
        {
            var childColumns = columns.Copy();
            childColumns.Unset(item.Key);
            return _children[item.Key].GetSubsets(childColumns);
        }).ToList();

        if (_content is not null) result.AddRange(_content);
        return result;
    }

    /// <returns>
    /// All values stored in this tree.
    /// </returns>
    public List<T> GetAll()
    {
        var result = _children.SelectMany(pair => pair.Value.GetAll()).ToList();
        if (_content is not null) result.AddRange(_content);
        return result;
    }
}
