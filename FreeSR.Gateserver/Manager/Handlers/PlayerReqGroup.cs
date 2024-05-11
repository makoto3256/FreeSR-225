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
                    Data = Convert.FromBase64String("G0x1YVMBGZMNChoKBAQICHhWAAAAAAAAAAAAAAAod0ABTkBDOlxVc2Vyc1x4ZW9uZGV2XERvY3VtZW50c1xnaXRcaGtycGdfYnVpbGRfc2VjdXJpdHlcamFkZV9zZWN1cml0eV9tb2R1bGUubHVhAAAAAAAAAAAAAQQfAAAAJABAAClAQAApgEAAKcBAAFYAAQAsgAABHUBBAKSAQQDkAEAA6cDBAekAwgHpQMIBrAAAASyAAAAfwEKFJABAAClAQAApgEAAKcBAAFYAAwAsgAABHUBBAKSAQQDkAEAA6cDBAekAwgHpQMIBrAAAASyAAAAfQEOFGQCAAA4AAAAEA0NTBAxVbml0eUVuZ2luZQQLR2FtZU9iamVjdAQFRmluZAQpVUlSb290L0Fib3ZlRGlhbG9nL0JldGFIaW50RGlhbG9nKENsb25lKQQXR2V0Q29tcG9uZW50SW5DaGlsZHJlbgQHdHlwZW9mBARSUEcEB0NsaWVudAQOTG9jYWxpemVkVGV4dAQFdGV4dBQqSmFkZVNSIGlzIGEgZnJlZSBhbmQgb3BlbiBzb3VyY2Ugc29mdHdhcmUEDFZlcnNpb25UZXh0FC5WaXNpdCBkaXNjb3JkLmdnL3JldmVyc2Vkcm9vbXMgZm9yIG1vcmUgaW5mbyEBAAAAAQAAAAAAHwAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAAAAAAAAQAAAAVfRU5W")
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
                    Nickname = "xeondev",
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
                Uid = 1337,
                //BlackInfo = null,
                Msg = "OK",
                SecretKeySeed = 0
            });
        }
    }
}
