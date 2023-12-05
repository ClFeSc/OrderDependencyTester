using System.Collections;

namespace DependencyTester;

// Source: https://github.com/SchweizerischeBundesbahnen/BCNFStar/blob/main/frontend/src/model/schema/ColumnsTree.ts
/// <summary>
/// A prefix tree for sets of <see cref="Attribute">Attributes</see>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ColumnsTree<T>
{
    private readonly Dictionary<int, ColumnsTree<T>> _children = new();
    private T? _content;

    private int _numAttributes;

    public ColumnsTree(int numAttributes)
    {
        _numAttributes = numAttributes;
    }

    public BitArray toBitArray(IEnumerable<int> columns)
    {
        var result = new BitArray(_numAttributes);
        foreach (var column in columns)
        {
            result.Set(column, true);
        }
        return result;
    }

    /// <summary>
    /// Adds a value to the tree.
    ///
    /// <param name="content">The value to be set.</param>
    /// <param name="columns">The location to store the values at.</param>
    /// </summary>
    public void Add(T content, IEnumerable<int> columns)
    {
        Traverse(toBitArray(columns))._content = content;
    }

    /// <returns>
    /// The value stored at <paramref name="columns"/>, or `null` if nothing is there.
    /// </returns>
    public T? Get(IEnumerable<int> columns) => Traverse(toBitArray(columns))._content;

    private ColumnsTree<T> Traverse(BitArray columns)
    {
        var current = this;
        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i] == false) continue;
            current._children.TryAdd(i, new ColumnsTree<T>(_numAttributes));
            current = current._children[i];
        }
        return current;
    }
    public List<T> GetSubsets(HashSet<int> columns)
    {
        BitArray columnBits = toBitArray(columns);
        return GetSubsets(columnBits);
    }


    /// <returns>
    /// All values stored in the tree for a subset of the given <paramref name="columns"/>.
    /// </returns>
    public List<T> GetSubsets(BitArray columns)
    {
        var result = _children.Where(item => columns.Get(item.Key)).SelectMany((item) => {
            var childColumns = new BitArray(columns);
            childColumns.Set(item.Key, false);
            return _children[item.Key].GetSubsets(childColumns);
        }).ToList();
       
        if (_content is not null) result.Add(_content);
        return result;
    }

    /// <returns>
    /// All values stored in this tree.
    /// </returns>
    public List<T> GetAll()
    {
        var result = _children.SelectMany(pair => pair.Value.GetAll()).ToList();
        if (_content is not null) result.Add(_content);
        return result;
    }
}
