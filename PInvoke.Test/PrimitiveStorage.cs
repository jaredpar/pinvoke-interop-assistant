using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PInvoke.Primitive;

namespace PInvoke.Test
{
    public sealed class PrimitiveStorage : IPrimitiveReader, IPrimitiveWriter
    {
        private readonly List<NativeTypeId> _typeIdList = new List<NativeTypeId>();
        private readonly List<NativeMemberId> _memberIdList = new List<NativeMemberId>();

        public IEnumerable<NativeMemberId> ReadMembers(NativeTypeId typeId)
        {
            return _memberIdList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<NativeTypeId> ReadTypeIds()
        {
            return _typeIdList;
        }

        public void Write(NativeMemberId member)
        {
            _memberIdList.Add(member);
        }

        public void Write(NativeTypeId typeId)
        {
            _typeIdList.Add(typeId);
        }
    }
}
