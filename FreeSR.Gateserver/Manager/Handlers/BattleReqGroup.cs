using FreeSR.Gateserver.Manager.Handlers.Core;
using static FreeSR.Gateserver.Manager.Handlers.LineupReqGroup;
using FreeSR.Gateserver.Network;
using FreeSR.Proto;
using Newtonsoft.Json;

namespace FreeSR.Gateserver.Manager.Handlers
{
    internal static class BattleReqGroup
    {
        [Handler(CmdType.CmdSetLineupNameCsReq)]
        public static void OnSetLineupNameCsReq(NetSession session, int cmdId, object data)
        {
            var request = data as SetLineupNameCsReq;
            if (request.Name.StartsWith("battle"))
            {

                var lineupInfo = new LineupInfo
                {
                    ExtraLineupType = ExtraLineupType.LineupNone,
                    Name = "Squad 1",
                    Mp = 5,
                    MaxMp = 5,
                    LeaderSlot = 0
                };
                List<uint> characters = new List<uint> { Avatar1, Avatar2, Avatar3, Avatar4 };
                foreach (uint id in characters)
                {
                    if (id == 0) continue;
                    lineupInfo.AvatarLists.Add(new LineupAvatar
                    {
                        Id = id,
                        Hp = 10000,
                        Satiety = 100,
                        Sp = new AmountInfo { CurAmount = 10000, MaxAmount = 10000 },
                        AvatarType = AvatarType.AvatarFormalType,
                        Slot = (uint)lineupInfo.AvatarLists.Count
                    });
                }

                // spawn calyx
                if (request.Name == "battle-belobog")
                {
                    BeloboxCalyx(session, lineupInfo);
                }
                else
                {
                    PenaconyCalyx(session, lineupInfo);
                }
            }

            session.Send(CmdType.CmdSetLineupNameScRsp, new SetLineupNameScRsp
            {
                Retcode = 0,
                Name = request.Name,
                Index = request.Index
            });
        }


        [Handler(CmdType.CmdStartCocoonStageCsReq)]
        public static void OnStartCocoonStageCsReq(NetSession session, int cmdId, object data)
        {
            var request = data as StartCocoonStageCsReq;

            Dictionary<uint, List<uint>> monsterIds = new Dictionary<uint, List<uint>>
            {
                // { 1, new List<uint> { <MONSTER ID HERE> } }
            };

            Dictionary<uint, uint> monsterLevels = new Dictionary<uint, uint>
            {
                // {1, <MONSTER LEVEL HERE>}
            };

            // basic
            var battleInfo = new SceneBattleInfo
            {
                StageId = 201012311, // calyx
                LogicRandomSeed = (uint)new Random().Next(),
                WorldLevel = 6,
                RoundsLimit = 30, // round limit (not working outside challenge)
            };

            // avatar
            List<uint> SkillIdEnds = new List<uint> { 1, 2, 3, 4, 7, 101, 102, 103, 201, 202, 203, 204, 205, 206, 207, 208, 209, 210 };
            List<uint> characters = new List<uint> { Avatar1, Avatar2, Avatar3, Avatar4 };

            var battleAvatars = new Dictionary<uint, BattleAvatar>();
            var battleAvatarBuffs = new Dictionary<uint, List<uint>>();
            var pureFictionBlessings = new List<uint>();

            // freesrtools json parse
            try
            {
                using StreamReader reader = new StreamReader("freesrdata.json");
                var payload = JsonConvert.DeserializeObject<Payload>(reader.ReadToEnd());
                // avatars
                foreach (var item in payload.avatars)
                {
                    var avatarJson = item.Value;
                    var lightconeIndex = payload.lightcones.FindIndex((v) => v.equipAvatar == avatarJson.avatarId);
                    var relicsJson = payload.relics.FindAll((v) => v.equipAvatar == avatarJson.avatarId);

                    // avatar
                    var avatar = new BattleAvatar
                    {
                        Id = (uint)avatarJson.avatarId,
                        Level = (uint)avatarJson.level,
                        Promotion = (uint)avatarJson.promotion,
                        Rank = (uint)avatarJson.data.rank,
                        Hp = 10000,
                        AvatarType = AvatarType.AvatarFormalType,
                        WorldLevel = 6,
                        Sp = new AmountInfo { CurAmount = 10000, MaxAmount = 10000 },
                        RelicLists = { },
                        EquipmentLists = { }
                    };

                    // set energy
                    if (avatarJson.spValue != null /*&& avatarJson.spValue > 0*/ && avatarJson.spMax != null && avatarJson.spMax >= avatarJson.spValue)
                    {
                        avatar.Sp.CurAmount = (uint)avatarJson.spValue;
                        avatar.Sp.MaxAmount = (uint)avatarJson.spMax;
                    }

                    // set technique
                    if (avatarJson.useTechnique != null && avatarJson.useTechnique.Count > 0)
                    {
                        battleAvatarBuffs.Add(avatar.Id, new List<uint>());
                        foreach (uint techId in avatarJson.useTechnique)
                        {
                            battleAvatarBuffs[avatar.Id].Add(techId);
                        }
                    }

                    // lightcone
                    if (lightconeIndex > -1)
                    {
                        var lightcone = payload.lightcones[lightconeIndex];
                        avatar.EquipmentLists.Add(
                            new BattleEquipment
                            {
                                Id = (uint)lightcone.itemId,
                                Level = (uint)lightcone.level,
                                Promotion = (uint)lightcone.promotion,
                                Rank = (uint)lightcone.rank,
                            }
                        );
                    }


                    // relics
                    foreach (var relic in relicsJson)
                    {
                        var relicData = new BattleRelic
                        {
                            Id = (uint)relic.relicId,
                            Level = (uint)relic.level,
                            MainAffixId = (uint)relic.mainAffixId,
                            SubAffixLists = { }
                        };
                        foreach (var subAffix in relic.subAffixes)
                        {
                            relicData.SubAffixLists.Add(new RelicAffix
                            {
                                AffixId = (uint)subAffix.subAffixId,
                                Cnt = (uint)subAffix.count,
                                Step = (uint)subAffix.step,
                            });
                        }
                        avatar.RelicLists.Add(relicData);
                    }

                    // skills
                    foreach (var skill in avatarJson.data.skills)
                    {
                        var skillId = (uint)skill.Key;
                        var level = (uint)skill.Value;
                        avatar.SkilltreeLists.Add(
                            new AvatarSkillTree
                            {
                                PointId = (uint)skillId,
                                Level = level,
                            }
                        );
                    }


                    battleAvatars.Add(avatar.Id, avatar);
                }

                //monsters
                uint id = 1;
                foreach (var wave in payload.monsters)
                {
                    var monsters = new List<uint>();
                    foreach (var monster in wave)
                    {
                        // set stage id for turbulence
                        if (monster.stageId != null && monster.stageId > 0)
                        {
                            battleInfo.StageId = (uint)monster.stageId;
                        }

                        // cycle count
                        if (monster.cycleCount != null && monster.cycleCount > 0)
                        {
                            battleInfo.RoundsLimit = (uint)monster.cycleCount;
                        }

                        // detect purefiction
                        if (monster.pureFictionBlessings != null && monster.pureFictionBlessings.Count > 0)
                        {
                            pureFictionBlessings = monster.pureFictionBlessings;
                        }

                        monsters.Add((uint)monster.monsterId);
                    }

                    var level = wave.Aggregate(0, (prev, cur) =>
                        prev < cur.level ? cur.level : prev
                    );
                    monsterIds[id] = monsters;
                    monsterLevels[id] = (uint)level;
                    id++;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            // monsters sanity check
            if (monsterIds.Count == 0)
            {
                monsterIds.Add(1, new List<uint> { 3014022 });
            }
            if (monsterLevels.Count == 0)
            {
                monsterLevels.Add(1, 60);
            }

            // ?????
            int index = -1;
            foreach (uint avatarId in characters)
            {
                // ????????
                index++;

                BattleAvatar avatarData;

                // set avatar
                if (battleAvatars.TryGetValue(avatarId, out BattleAvatar value))
                {
                    avatarData = value;
                }
                // fallback
                else
                {
                    if (avatarId == 0) continue;
                    avatarData = new BattleAvatar
                    {
                        Id = avatarId,
                        Level = 80,
                        Promotion = 6,
                        Rank = 6,
                        Hp = 10000,
                        AvatarType = AvatarType.AvatarFormalType,
                        WorldLevel = 6,
                        Sp = new AmountInfo { CurAmount = 10000, MaxAmount = 10000 },
                        RelicLists = {
                                //begin relic loop
                                new BattleRelic
                                {
                                    Id = 61011,
                                    Level = 999,
                                    MainAffixId = 1,
                                    SubAffixLists = {
                                        new RelicAffix{AffixId = 4, Step = 999 },
                                    }
                                }
                                //end relic loop
                            },
                        EquipmentLists = {
                                //begin equipment
                                new BattleEquipment
                                {
                                    Id = 23006,
                                    Level = 80,
                                    Rank = 5,
                                    Promotion = 6
                                } 
                                //end equipment
                            }
                    };
                    foreach (uint end in SkillIdEnds)
                    {
                        uint level = 1;
                        if (end == 1) level = 6; // basic atk
                        else if (end < 4 || end == 4) level = 10; // skill, talent, ult
                        if (end > 4) level = 1; // technique
                        avatarData.SkilltreeLists.Add(new AvatarSkillTree
                        {
                            PointId = avatarId * 1000 + end,
                            Level = level
                        });
                    }
                }

                // set technique
                if (battleAvatarBuffs.TryGetValue(avatarId, out List<uint> buffs))
                {

                    foreach (var buffId in buffs)
                    {
                        var buff = new BattleBuff();
                        buff.WaveFlag = 0xffffffff; //(uint)(1 << monsterIds.Count); // waveflag
                        buff.OwnerId = (uint)index; // ownerid
                        buff.Level = 1;
                        buff.Id = buffId;
                        battleInfo.BuffLists.Add(buff);
                    }
                }

                battleInfo.BattleAvatarLists.Add(avatarData);
            }

            //monster
            for (uint i = 1; i <= monsterIds.Count; i++)
            {
                SceneMonsterWave monsterInfo = new SceneMonsterWave
                {
                    Imapolkmefn = i,
                    MonsterParam = new SceneMonsterParam
                    {
                        Level = monsterLevels[i],
                    },
                };

                if (monsterIds.ContainsKey(i))
                {
                    List<uint> monsterIdList = monsterIds[i];

                    foreach (uint monsterId in monsterIdList)
                    {
                        monsterInfo.MonsterLists.Add(new SceneMonsterInfo
                        {
                            MonsterId = monsterId
                        });
                    }

                }
                battleInfo.MonsterWaveLists.Add(monsterInfo);
            }

            // pure fiction
            if (pureFictionBlessings.Count > 0)
            {
                // pf score counter
                var bt = new BattleTargeList();
                bt.Fdfcmhbhnmcs.Add(new BattleTarget { Id = 10001, Progress = 0 });
                battleInfo.BattleTargeInfoes.Add(1, bt);

                // pf param
                for (uint i = 2; i <= 4; i++)
                {
                    battleInfo.BattleTargeInfoes.Add(i, new BattleTargeList());
                }
                //pf param 
                battleInfo.BattleTargeInfoes.Add(5, new BattleTargeList
                {
                    Fdfcmhbhnmcs =
                    {
                        new BattleTarget{ Id = 2001, Progress = 0 },
                        new BattleTarget{ Id = 2002, Progress = 0 },
                    }
                });


                // pf blessing
                foreach (var blessing in pureFictionBlessings)
                {
                    battleInfo.BuffLists.Add(new BattleBuff
                    {
                        WaveFlag = 0xffffffff,
                        OwnerId = 0xffffffff,
                        Level = 1,
                        Id = blessing
                    });
                }
            }

            var response = new StartCocoonStageScRsp
            {
                Retcode = 0,
                CocoonId = request.CocoonId,
                Wave = request.Wave,
                PropEntityId = request.PropEntityId,
                BattleInfo = battleInfo
            };

            session.Send(CmdType.CmdStartCocoonStageScRsp, response);
        }

        [Handler(CmdType.CmdPVEBattleResultCsReq)]
        public static void OnPVEBattleResultCsReq(NetSession session, int cmdId, object data)
        {
            var request = data as PVEBattleResultCsReq;
            session.Send(CmdType.CmdPVEBattleResultScRsp, new PVEBattleResultScRsp
            {
                Retcode = 0,
                EndStatus = request.EndStatus
            });
        }


        // Calyx config
        internal static void BeloboxCalyx(NetSession session, LineupInfo lineupInfo)
        {
            var sceneInfo = new SceneInfo
            {
                GameModeType = 2,
                EntryId = 2010101,
                PlaneId = 20101,
                FloorId = 20101001
            };

            var calaxInfoTest = new SceneEntityInfo
            {
                GroupId = 19,
                InstId = 300001,
                EntityId = 4194583,
                Prop = new ScenePropInfo
                {
                    PropState = 1,
                    PropId = 808
                },
                Motion = new MotionInfo
                {
                    Pos = new Vector
                    {
                        X = -570,
                        Y = 19364,
                        Z = 4480
                    },
                    Rot = new Vector
                    {
                        Y = 180000
                    }
                },
            };

            sceneInfo.EntityLists.Add(calaxInfoTest);

            session.Send(CmdType.CmdEnterSceneByServerScNotify, new EnterSceneByServerScNotify
            {
                Scene = sceneInfo,
                Lineup = lineupInfo
            });

            session.Send(CmdType.CmdSceneEntityMoveScNotify, new SceneEntityMoveScNotify
            {
                EntryId = 2010101,
                EntityId = 0,
                Motion = new MotionInfo
                {
                    Pos = new Vector
                    {
                        X = -570,
                        Y = 19364,
                        Z = 4480
                    },
                    Rot = new Vector
                    {
                        Y = 180000
                    }
                }
            });
        }

        internal static void PenaconyCalyx(NetSession session, LineupInfo lineupInfo)
        {
            var sceneInfo = new SceneInfo
            {
                GameModeType = 2,
                EntryId = 2031301,
                PlaneId = 20313,
                FloorId = 20313001
            };

            var calyxPenacony = new SceneGroupInfo
            {
                State = 0,
                GroupId = 186
            };

            // flower npc
            calyxPenacony.EntityLists.Add(new SceneEntityInfo
            {
                InstId = 300001, // flower id
                EntityId = 328, // flower entity id (can be anything??)
                GroupId = 186, //
                Prop = new ScenePropInfo { CreateTimeMs = 0, PropState = 8, LifeTimeMs = 0, PropId = 808 },
                Motion = new MotionInfo
                {
                    Pos = new Vector
                    {
                        X = 31440,
                        Y = 192820,
                        Z = 433790
                    },
                    Rot = new Vector
                    {
                        X = 0,
                        Y = 60000,
                        Z = 0
                    }
                },
            });

            sceneInfo.SceneGroupLists.Add(calyxPenacony);

            session.Send(CmdType.CmdEnterSceneByServerScNotify, new EnterSceneByServerScNotify
            {
                Scene = sceneInfo,
                Lineup = lineupInfo
            });

            session.Send(CmdType.CmdSceneEntityMoveScNotify, new SceneEntityMoveScNotify
            {
                EntryId = 2031301,
                EntityId = 0,
                Motion = new MotionInfo
                {
                    Pos = new Vector
                    {
                        X = 32342,
                        Y = 192820,
                        Z = 434276
                    },
                    Rot = new Vector
                    {
                        Y = 240000
                    }
                }
            });
        }
    }


    // FREESR_TOOLS
    internal class RelicJson
    {
        public int level { get; set; }
        public int relicId { get; set; }
        public int relicSetId { get; set; }
        /**
         * THIS IS ONLY THEIR STAT ordering ID (ie: atk=1), not the actual id. get from mainaffixmap[relicid] if you want mainaffixid instead
         */
        public int mainAffixId { get; set; }
        public List<SubAffix> subAffixes { get; set; }
        public bool _hasError { get; set; }
        /**
         * for backend, used as an identifier when fetching relics from backend
         */
        public int? internalUid { get; set; }
        /**
         * for frontend, used as an identifier when scanning relic result
         */
        public string internalUidFrontend { get; set; }
        /**
         * ONLY DEFINED WHEN GETTING RELICS FROM BACKEND,
         */
        public int? equipAvatar { get; set; }
    }

    internal class SubAffix
    {
        public int subAffixId { get; set; }
        public int count { get; set; }
        public int step { get; set; }
        public bool? _isError { get; set; }
        public int? _renderedValue { get; set; }
    }

    internal class GetCharactersResJson
    {
        public int ownerUid { get; set; }
        public int avatarId { get; set; }
        public PartialData data { get; set; }
        public int level { get; set; }
        public int promotion { get; set; }
        public int internalUid { get; set; }

        public int? spValue { get; set; }

        public int? spMax { get; set; }

#pragma warning disable
        public List<uint>? useTechnique { get; set; }
    }

    internal class PartialData
    {
        public int rank { get; set; }
        /**
         * key: skill_id
         * value: skill level
         */
        public Dictionary<int, int> skills { get; set; }
    }

    internal class LightconeJson
    {
        public int level { get; set; }
        public int itemId { get; set; }
        public int equipAvatar { get; set; }
        public int rank { get; set; }
        public int promotion { get; set; }
        public int internalUid { get; set; }
    }

    internal class MonsterJson
    {
        public int amount { get; set; }
        public int level { get; set; }
        public int monsterId { get; set; }

        public int? stageId { get; set; }

        public int? cycleCount { get; set; }

#pragma warning disable
        public List<uint>? pureFictionBlessings { get; set; }
    }

    internal class Payload
    {
        public Dictionary<string, GetCharactersResJson> avatars { get; set; }
        public List<LightconeJson> lightcones { get; set; }
        public List<List<MonsterJson>> monsters { get; set; }
        public List<RelicJson> relics { get; set; }
    }
}