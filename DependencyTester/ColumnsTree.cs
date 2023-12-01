using System.Collections;
using Attribute = OrderDependencyModels.Attribute;

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

    private Dictionary<Attribute, int> _attributesMap;

    public ColumnsTree(List<Attribute> allAttributes)
    {
        _attributesMap = new Dictionary<Attribute, int>();
        for (int i = 0; i < allAttributes.Count; i++)
        {
            _attributesMap.Add(allAttributes[i], i);
        }
    }

    public ColumnsTree(Dictionary<Attribute, int> attributesMap)
    {
        _attributesMap = attributesMap;
    }

    public BitArray toBitArray(IEnumerable<Attribute> columns)
    {
        var result = new BitArray(_attributesMap.Count);
        foreach (var column in columns)
        {
            result.Set(_attributesMap[column], true);
        }
        return result;
    }

    /// <summary>
    /// Adds a value to the tree.
    ///
    /// <param name="content">The value to be set.</param>
    /// <param name="columns">The location to store the values at.</param>
    /// </summary>
    public void Add(T content, IEnumerable<Attribute> columns)
    {
        Traverse(toBitArray(columns))._content = content;
    }

    /// <returns>
    /// The value stored at <paramref name="columns"/>, or `null` if nothing is there.
    /// </returns>
    public T? Get(IEnumerable<Attribute> columns) => Traverse(toBitArray(columns))._content;

    private ColumnsTree<T> Traverse(BitArray columns)
    {
        var current = this;
        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i] == false) continue;
            current._children.TryAdd(i, new ColumnsTree<T>(_attributesMap));
            current = current._children[i];
        }
        return current;
    }
    public List<T> GetSubsets(HashSet<Attribute> columns)
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
