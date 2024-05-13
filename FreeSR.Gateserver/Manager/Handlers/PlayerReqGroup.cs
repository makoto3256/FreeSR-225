namespace FreeSR.Gateserver.Manager.Handlers
{
    using FreeSR.Gateserver.Manager.Handlers.Core;
    using FreeSR.Gateserver.Network;
    using FreeSR.Proto;
    using NLog;

    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;
    using Newtonsoft.Json;

    internal static class PlayerReqGroup
    {
        private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

        [Handler(CmdType.CmdPlayerHeartBeatCsReq)]
        public static void OnPlayerHeartBeatCsReq(NetSession session, int cmdId, object data)
        {
            var heartbeatReq = data as PlayerHeartBeatCsReq;

            session.Send(CmdType.CmdPlayerHeartBeatScRsp, new PlayerHeartBeatScRsp
            {
                Retcode = 0,

                DownloadData = new ClientDownloadData
                {
                    Version = 51,
                    Time = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    Data = Convert.FromBase64String("bG9jYWwgZnVuY3Rpb24gYmV0YV90ZXh0KCkKICAgIGxvY2FsIGdhbWVPYmplY3QgPSBDUy5Vbml0eUVuZ2luZS5HYW1lT2JqZWN0LkZpbmQoIlVJUm9vdC9BYm92ZURpYWxvZy9CZXRhSGludERpYWxvZyhDbG9uZSkiKQogICAgaWYgZ2FtZU9iamVjdCB0aGVuCiAgICAgICAgbG9jYWwgdGV4dENvbXBvbmVudCA9IGdhbWVPYmplY3Q6R2V0Q29tcG9uZW50SW5DaGlsZHJlbih0eXBlb2YoQ1MuUlBHLkNsaWVudC5Mb2NhbGl6ZWRUZXh0KSkKICAgICAgICBpZiB0ZXh0Q29tcG9uZW50IHRoZW4KICAgICAgICAgICAgdWlkID0gdGV4dENvbXBvbmVudC50ZXh0OwogICAgICAgICAgICB0ZXh0Q29tcG9uZW50LnRleHQgPSAnPHNpemU9MTU+PGNvbG9yPSNGRkZGMDA+RnJlZVNS5piv5LiA5Liq5byA5rqQ5LiU5YWN6LS555qE6aG555uuPC9jb2xvcj48L3NpemU+JwogICAgICAgICAgICB0ZXh0Q29tcG9uZW50LmZvbnRTaXplID0gMTAwCiAgICAgICAgZW5kCiAgICBlbmQKZW5kCgpsb2NhbCBmdW5jdGlvbiBtaHlfdGV4dChvYmopCiAgICBsb2NhbCBnYW1lT2JqZWN0ID0gQ1MuVW5pdHlFbmdpbmUuR2FtZU9iamVjdC5GaW5kKCJJRE1BUDEiKQogICAgaWYgZ2FtZU9iamVjdCB0aGVuCiAgICAgICAgbG9jYWwgdGV4dENvbXBvbmVudCA9IGdhbWVPYmplY3Q6R2V0Q29tcG9uZW50SW5DaGlsZHJlbih0eXBlb2YoQ1MuUlBHLkNsaWVudC5NZXNzYWdlQm94RGlhbG9nVXRpbCkpCiAgICAgICAgaWYgdGV4dENvbXBvbmVudCB0aGVuCiAgICAgICAgICAgIHRleHRDb21wb25lbnQuU2hvd0Fib3ZlRGlhbG9nVGV4dCA9IGZhbHNlCiAgICAgICAgICAgIHRleHRDb21wb25lbnQuZm9udFNpemUgPSAxMDAKICAgICAgICBlbmQKICAgIGVuZAplbmQKCmJldGFfdGV4dCgpCm1oeV90ZXh0KCk=")
                },

                ClientTimeMs = heartbeatReq.ClientTimeMs,
                ServerTimeMs = (ulong)DateTimeOffset.Now.ToUnixTimeMilliseconds()
            });
        }

        [Handler(CmdType.CmdGetHeroBasicTypeInfoCsReq)]
        public static void OnGetHeroBasicTypeInfoCsReq(NetSession session, int cmdId, object _)
        {
            session.Send(CmdType.CmdGetHeroBasicTypeInfoScRsp, new GetHeroBasicTypeInfoScRsp
            {
                Retcode = 0,
                Gender = Gender.GenderMan,
                BasicTypeInfoLists ={
                    new PlayerHeroBasicTypeInfo
                    {
                        BasicType = HeroBasicType.BoyWarrior,
                        Rank = 1,
                        SkillTreeLists = {}
                    }
                },
                CurBasicType = HeroBasicType.BoyWarrior,
            });
        }

        [Handler(CmdType.CmdGetBasicInfoCsReq)]
        public static void OnGetBasicInfoCsReq(NetSession session, int cmdId, object _)
        {
            session.Send(CmdType.CmdGetBasicInfoScRsp, new GetBasicInfoScRsp
            {
                CurDay = 1,
                ExchangeTimes = 0,
                Retcode = 0,
                NextRecoverTime = 2281337,
                WeekCocoonFinishedCount = 0
            });
        }

        [Handler(CmdType.CmdPlayerLoginCsReq)]
        public static void OnPlayerLoginCsReq(NetSession session, int cmdId, object data)
        {
            var request = data as PlayerLoginCsReq;

            session.Send(CmdType.CmdPlayerLoginScRsp, new PlayerLoginScRsp
            {
                Retcode = 0,
                //IsNewPlayer = false,
                LoginRandom = request.LoginRandom,
                Stamina = 240,
                ServerTimestampMs = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds() * 1000,
                BasicInfo = new PlayerBasicInfo
                {
                    Nickname = "FreeSR",
                    Level = 70,
                    Exp = 0,
                    Stamina = 100,
                    Mcoin = 0,
                    Hcoin = 0,
                    Scoin = 0,
                    WorldLevel = 6
                }
            });
        }

        [Handler(CmdType.CmdPlayerGetTokenCsReq)]
        public static void OnPlayerGetTokenCsReq(NetSession session, int cmdId, object data)
        {
            session.Send(CmdType.CmdPlayerGetTokenScRsp, new PlayerGetTokenScRsp
            {
                Retcode = 0,
                Uid = 10001,
                //BlackInfo = null,
                Msg = "OK",
                SecretKeySeed = 0
            });
        }
    }
}
