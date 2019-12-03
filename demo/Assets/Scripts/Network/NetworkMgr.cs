
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

namespace GEngine
{

    // 客户端会连接两个App
    enum AppType
    {
        Login,
        Game,
    }

    enum NetworkState
    {
        ReadyConnect,
        Connecting,
        Connected,
        Disconnected
    }

    class AsyncConnetInfo
    {
        public Socket socket;
        //public eEventType eventType;
    }

    class NetworkMgr : SingletonBehaviour<NetworkMgr>
    {
        private static int SendTimeInterval = 5;

        private Socket _sock;
        private NetworkState _state;
        private AppType _appType;

        private float _nextSendPing;

        private byte[] _recvBuf;
        private int _recvIndex;

        protected override void OnAwake()
        {
            this._recvIndex = 0;
            this._recvBuf = new byte[512 * 1024];

            RegisterPacket(Proto.MsgId.C2LAccountCheckRs, Process<Proto.AccountCheckRs>);
            RegisterPacket(Proto.MsgId.C2LCreatePlayerRs, Process<Proto.CreatePlayerRs>);
            RegisterPacket(Proto.MsgId.C2LSelectPlayerRs, Process<Proto.SelectPlayerRs>);
            RegisterPacket(Proto.MsgId.S2CEnterWorld, Process<Proto.EnterWorld>);
            RegisterPacket(Proto.MsgId.L2CGameToken, Process<Proto.GameToken>);
            RegisterPacket(Proto.MsgId.C2GLoginByTokenRs, Process<Proto.LoginByTokenRs>);
            RegisterPacket(Proto.MsgId.L2CPlayerList, Process<Proto.PlayerList>);
            RegisterPacket(Proto.MsgId.G2CSyncPlayer, Process<Proto.SyncPlayer>);
            RegisterPacket(Proto.MsgId.S2CRoleAppear, Process<Proto.RoleAppear>);
            RegisterPacket(Proto.MsgId.S2CMove, Process<Proto.Move>);
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        public void Connect(string ip, int port, AppType appType)
        {

            UnityEngine.Debug.Log("host:" + ip + " port:" + port);
            this._appType = appType;
            this._recvIndex = 0;

            this._sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _sock.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _sock.Blocking = true;
            _sock.SendBufferSize = this._recvBuf.Length;

            _state = NetworkState.Connecting;

            EventDispatcher.GetInstance().Broadcasting(eEventType.Connectting);

            // 异步连接,连接成功调用connectCallback方法  
            _sock.BeginConnect(ip, port, AsyncConnetCallback, new AsyncConnetInfo() { socket = _sock });

            _nextSendPing = UnityEngine.Time.realtimeSinceStartup + SendTimeInterval;
        }

        public void Disconnect()
        {
            if (_sock == null)
                return;

            UnityEngine.Debug.Log("network disconnect.socke is close.");

            _sock.Close();
            _sock = null;

            _state = NetworkState.Disconnected;

            EventDispatcher.GetInstance().Broadcasting(eEventType.Disconnect, _appType);
        }

        private void AsyncConnetCallback(IAsyncResult ar)
        {
            try
            {
                AsyncConnetInfo aInfo = (AsyncConnetInfo)ar.AsyncState;
                Socket socket = aInfo.socket;
                if (ar.IsCompleted)
                {
                    if (socket != null)
                    {
                        socket.EndConnect(ar);
                        socket.Blocking = false;
                    }

                    UnityEngine.Debug.Log("Connect succeeded.");
                    this._state = NetworkState.Connected;

                    EventDispatcher.GetInstance().Broadcasting(eEventType.Connected, _appType);

                    return;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(ex.Message);
            }

            UnityEngine.Debug.LogWarning("Connect failed");
            Disconnect();
        }

        public void FixedUpdate()
        {
            if (_sock == null)
                return;

            if (_state != NetworkState.Connected)
                return;

            if (_nextSendPing < UnityEngine.Time.realtimeSinceStartup)
            {
                _nextSendPing = UnityEngine.Time.realtimeSinceStartup + SendTimeInterval;
                UnityEngine.Debug.Log("send ping msg.");
                SendPacket(Proto.MsgId.MiPing, null);
            }
        }

        public void Update()
        {
            if (_sock == null)
                return;

            if (_state != NetworkState.Connected)
                return;

            try
            {
                // 接收数据
                while (true)
                {
                    var bufAvail = this._recvBuf.Length - this._recvIndex;
                    if (bufAvail <= 0)
                        break;

                    if (_sock.Available <= 0)
                        break;

                    int receivedSize = _sock.Receive(_recvBuf, _recvIndex, bufAvail, SocketFlags.None);
                    _recvIndex += receivedSize;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
                Disconnect();
                return;
            }

            // 如果收到的数据没有达到包头的数据大小，继续等待读包
            if (_recvIndex < PacketHead.HeadSize)
            {
                return;
            }

            // 拆包
            while (_recvIndex >= PacketHead.HeadSize)
            {
                PacketHead head = new PacketHead();
                var byteStream = new MemoryStream(_recvBuf);
                BinaryReader br = new BinaryReader(byteStream);
                var totalLen = br.ReadUInt16();
                var headLen = br.ReadUInt16();
                head.msg_id = br.ReadUInt16();

                // 如果收到的数据包不完整
                if (totalLen > _recvIndex)
                    break;

                UnPacket(head, _recvBuf, (int)byteStream.Position, totalLen - PacketHead.HeadSize);

                // 有协议可能会关闭网络
                if (_state != NetworkState.Connected)
                    return;

                _recvIndex -= totalLen;

                UnityEngine.Debug.LogFormat($"_recv_index:{_recvIndex}, msgid:{(Proto.MsgId)head.msg_id}, msg total len:{totalLen}");

                // 剩余字节放在缓冲区前面去
                if (_recvIndex > 0)
                {
                    Array.Copy(_recvBuf, totalLen, _recvBuf, 0, _recvIndex);
                }
            }
        }

        public void UnPacket(PacketHead head, byte[] byteArray, int offset, int length)
        {
            Proto.MsgId msgId = (Proto.MsgId)head.msg_id;
            if (!_msgHandler.ContainsKey(msgId))
            {
                UnityEngine.Debug.LogWarning("!!!!! not found handler. recv packet msgId:" + head.msg_id);
                return;
            }

            Google.Protobuf.IMessage msg = _msgHandler[msgId](byteArray, offset, length);
            MessagePackDispatcher.GetInstance().Broadcasting(head.msg_id, msg);
        }

        public bool SendPacket(Proto.MsgId msgId, Google.Protobuf.IMessage msg)
        {
            if (_state != NetworkState.Connected)
                return false;

            int size = 0;
            if (msg != null)
            {
                size = msg.CalculateSize();
            }

            if (msgId != Proto.MsgId.MiPing)
            {
                UnityEngine.Debug.Log("send msg. msg_id:" + msgId + " msg size:" + size);
            }

            var byteStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(byteStream);
            var totalLen = size + PacketHead.HeadSize;
            bw.Write((ushort)totalLen);     // total size
            bw.Write((ushort)2);            // head size
            bw.Write((ushort)msgId);

            if (msg != null)
            {
                Google.Protobuf.CodedOutputStream outStream = new Google.Protobuf.CodedOutputStream(byteStream);
                msg.WriteTo(outStream);
                outStream.Flush();
            }

            Byte[] buf = byteStream.ToArray();
            size = size + PacketHead.HeadSize;

            try
            {
                int pos = 0;
                while (size > 0)
                {
                    int sent = _sock.Send(buf, pos, size, SocketFlags.None);
                    size -= sent;
                    pos += sent;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex.Message);
                Disconnect();
                return false;
            }

            return true;
        }

        #region 协议分发处理

        // 每个协终最后都对应一个Packet处理
        public delegate Google.Protobuf.IMessage ProcessPacketDelegate(byte[] byteArray, int offset, int length);

        private readonly Dictionary<Proto.MsgId, ProcessPacketDelegate> _msgHandler =
            new Dictionary<Proto.MsgId, ProcessPacketDelegate>();

        protected void RegisterPacket(Proto.MsgId msgId, ProcessPacketDelegate callback)
        {
            if (!_msgHandler.ContainsKey(msgId))
                _msgHandler.Add(msgId, null);

            _msgHandler[msgId] = _msgHandler[msgId] + callback;
        }

        private Google.Protobuf.IMessage Process<T>(byte[] byteArray, int offset, int length)
            where T : Google.Protobuf.IMessage, new()
        {
            UnityEngine.Debug.Log("recv msg. " + typeof(T));

            T protoObj = new T();
            Google.Protobuf.CodedInputStream inputStream =
                new Google.Protobuf.CodedInputStream(byteArray, offset, length);
            protoObj.MergeFrom(inputStream);
            return protoObj;
        }

        #endregion

    }
}