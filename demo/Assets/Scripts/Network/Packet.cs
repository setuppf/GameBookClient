using System;

namespace GEngine {
    [Serializable]
    class PacketHead {

        // 修改了头一定要修改 HeadSize
        // msgId + totallen + headlen
        public static ushort HeadSize => 2 + 2 + 2;
         
        public ushort msg_id { get; set; }
    }

}