using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaladimQBot.GoCqHttp.Apis;

public class GetGroupMemberListActionResultData : CqApiCallResultData, IList<GetGroupMemberInfoActionResultData>
{
    public List<GetGroupMemberInfoActionResultData> DataList = new();

    #region 使用DataList实现接口

    public GetGroupMemberInfoActionResultData this[int index] { get => ((IList<GetGroupMemberInfoActionResultData>)DataList)[index]; set => ((IList<GetGroupMemberInfoActionResultData>)DataList)[index] = value; }

    public int Count => ((ICollection<GetGroupMemberInfoActionResultData>)DataList).Count;

    public bool IsReadOnly => ((ICollection<GetGroupMemberInfoActionResultData>)DataList).IsReadOnly;

    public void Add(GetGroupMemberInfoActionResultData item)
    {
        ((ICollection<GetGroupMemberInfoActionResultData>)DataList).Add(item);
    }

    public void Clear()
    {
        ((ICollection<GetGroupMemberInfoActionResultData>)DataList).Clear();
    }

    public bool Contains(GetGroupMemberInfoActionResultData item)
    {
        return ((ICollection<GetGroupMemberInfoActionResultData>)DataList).Contains(item);
    }

    public void CopyTo(GetGroupMemberInfoActionResultData[] array, int arrayIndex)
    {
        ((ICollection<GetGroupMemberInfoActionResultData>)DataList).CopyTo(array, arrayIndex);
    }

    public IEnumerator<GetGroupMemberInfoActionResultData> GetEnumerator()
    {
        return ((IEnumerable<GetGroupMemberInfoActionResultData>)DataList).GetEnumerator();
    }

    public int IndexOf(GetGroupMemberInfoActionResultData item)
    {
        return ((IList<GetGroupMemberInfoActionResultData>)DataList).IndexOf(item);
    }

    public void Insert(int index, GetGroupMemberInfoActionResultData item)
    {
        ((IList<GetGroupMemberInfoActionResultData>)DataList).Insert(index, item);
    }

    public bool Remove(GetGroupMemberInfoActionResultData item)
    {
        return ((ICollection<GetGroupMemberInfoActionResultData>)DataList).Remove(item);
    }

    public void RemoveAt(int index)
    {
        ((IList<GetGroupMemberInfoActionResultData>)DataList).RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)DataList).GetEnumerator();
    }

    #endregion
}