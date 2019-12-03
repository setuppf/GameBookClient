using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{
    class World
    {
        private ResourceWorld _ref;
        public ResourceWorld RefMap => _ref;

        private MessagePackDispatcher _msgDispatcher = new MessagePackDispatcher();

        // <sn, SyncPlayer>
        private Dictionary<ulong, RoleAppear> _players = new Dictionary<ulong, RoleAppear>();

        public World(ResourceWorld refobj)
        {
            _ref = refobj;

            _msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.S2CRoleAppear, RoleAppear);
            _msgDispatcher.RegisterFollowPacket((int)Proto.MsgId.S2CMove, SyncMove);
        }

        #region 手势操作

        public void SetSelectObj(UnityEngine.GameObject obj)
        {

        }

        public void CancelSelectObj()
        {

        }

        #endregion

        #region 消息处理        

        public void HanderMsg(int msgId, Google.Protobuf.IMessage msg)
        {
            _msgDispatcher.Broadcasting(msgId, msg);
        }

        public void RoleAppear(Google.Protobuf.IMessage msg)
        {
            if (!(msg is Proto.RoleAppear appear))
            {
                UnityEngine.Debug.LogWarning($"parse RoleAppear error.");
                return;
            }

            foreach (var role in appear.Role)
            {
                var sn = role.Sn;
                if (!_players.ContainsKey(sn))
                {
                    RoleAppear appearObj = new RoleAppear();
                    appearObj.Parse(role);
                    _players.Add(sn, appearObj);
                    appearObj.Load3DObj();

                    UnityEngine.Debug.LogFormat($"sync player sn:{sn} world id:{_ref.GetId()}");
                }
                else
                {
                    _players[sn].Parse(role);
                }
            }
        }

        public void SyncMove(Google.Protobuf.IMessage msg)
        {
            Proto.Move moveProto = msg as Proto.Move;
            ulong playerSn = moveProto.PlayerSn;
            if (!_players.ContainsKey(playerSn))
            {
                UnityEngine.Debug.LogWarning($"sync move failed. can't find player. sn:{playerSn}");
                return;
            }

            var player = _players[playerSn];
            var moveComponent = player.GetGameObject().GetComponent<MoveComponent>();
            moveComponent.CornerPoints.AddRange(moveProto.Position);
        }

        #endregion
    }
}
