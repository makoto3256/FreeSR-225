﻿namespace FreeSR.Gateserver.Manager.Handlers
{
    using FreeSR.Gateserver.Manager.Handlers.Core;
    using FreeSR.Gateserver.Network;
    using FreeSR.Proto;
    using System.Numerics;


    internal static class SceneReqGroup
    {
        [Handler(CmdType.CmdGetCurSceneInfoCsReq)]
        public static void OnGetCurSceneInfoCsReq(NetSession session, int cmdId, object data)
        {
            SceneInfo scene = new SceneInfo
            {
                GameModeType = 2,
                //LeaderEntityId = 1,
                //Lgflfajffjl = 1,
                //Pjbjelcgkof =1,
                EntryId = 2010101,
                PlaneId = 20101,
                FloorId = 20101001,
            };

            /*scene.EntityLists.Add(new SceneEntityInfo
            {
                EntityId = 0,
                GroupId = 0,
                InstId = 0,
                Motion = new MotionInfo()
                {
                    Pos = new Vector(),
                    Rot = new Vector()
                }
            });*/

            session.Send(CmdType.CmdGetCurSceneInfoScRsp, new GetCurSceneInfoScRsp
            {
                Scene = scene,
                Retcode = 0
            });
        }

        [Handler(CmdType.CmdGetSceneMapInfoCsReq)]
        public static void OnGetSceneMapInfoCsReq(NetSession session, int cmdId, object data)
        {
            var request = data as GetSceneMapInfoCsReq;

            uint[] back = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 0 };

            var mapinfo = new SceneMapInfo
            {
                Retcode = 0,
                LightenSectionLists = back,
                ChestLists = {
                    new ChestInfo
                    {
                        ChestType = ChestType.MapInfoChestTypeNormal
                    },
                    new ChestInfo
                    {
                        ChestType = ChestType.MapInfoChestTypePuzzle
                    },
                    new ChestInfo
                    {
                        ChestType = ChestType.MapInfoChestTypeChallenge
                    }
                },
            };

            var response = new GetSceneMapInfoScRsp
            {
                Retcode = 0,
                EntryId = request.EntryIdLists[0],
                CurMapEntryId = request.EntryId,
                MapInfoLists = { mapinfo },
                LightenSectionLists = back,
            };

            session.Send(CmdType.CmdGetSceneMapInfoScRsp, response);
        }
    }
}
