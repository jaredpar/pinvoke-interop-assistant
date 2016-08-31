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
        private readonly List<NativeMemberData> _memberList = new List<NativeMemberData>();
        private readonly List<NativeEnumValueData> _enumValueList = new List<NativeEnumValueData>();

        public IEnumerable<NativeMemberData> ReadMembers(NativeTypeId typeId)
        {
            return _memberList.Where(x => x.ContainingTypeId == typeId);
        }

        public IEnumerable<NativeTypeId> ReadTypeIds()
        {
            return _typeIdList;
        }

        public IEnumerable<NativeEnumValueData> ReadEnumValues(NativeTypeId typeId)
        {
            return _enumValueList.Where(x => x.ContainingTypeId == typeId);
        }

        public void Write(NativeMemberData member)
        {
            _memberList.Add(member);
        }

        public void Write(NativeTypeId typeId)
        {
            _typeIdList.Add(typeId);
        }

        public void Write(NativeEnumValueData data)
        {
            _enumValueList.Add(data);
        }
    }
}
