using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AC", "NoNo", "1.20.7")]
    public class AC : RustPlugin
    {
        private const float VlMin = 0f;
        private const float SetbackCooldown = 0.25f;
        private const float JumpGraceSeconds = 0.45f;
        private const float NoFallMinDistance = 8f;
        private const float NoFallDamageWindow = 1.20f;
        private const float NoFallSecondCheckWindow = 0.80f;
        private const float NoFallDamageTimeTolerance = 0.90f;
        private const float NoFallCheckCommandDuration = 90f;
        private const float NoFallCommandMinDistance = 3.5f;
        private const float NoFallCommandLaunchHeight = 14f;
        private const float NoFallCommandDetectionsPause = 12f;
        private const float ManipulatorFastShotFactorDefault = 0.55f;
        private const float ManipulatorFastShotFactorSemi = 0.45f;
        private const float ManipulatorFastShotFactorBow = 0.35f;
        private const float ManipulatorFastShotMinMargin = 0.035f;
        private const float ManipulatorMinTraceDistance = 2.2f;
        private const float ManipulatorEndObstacleGrace = 0.55f;
        private const float ManipulatorVisiblePointGrace = 0.35f;
        private const float SilentAimBulletShotAge = 0.45f;
        private const float SilentAimBowShotAge = 1.35f;
        private const float SilentAimViewBlockGrace = 0.08f;
        private const float SilentAimDotMinBullet = 0.32f;
        private const float SilentAimDotMinBow = 0.16f;
        private const float SilentAimMinDistance = 12f;
        private const float SilentAimWindowSeconds = 8f;
        private const int SilentAimMinStrikes = 2;
        private const float SilentAimMoveRewindMaxBullet = 0.22f;
        private const float SilentAimMoveRewindMaxBow = 0.45f;
        private const float SilentAimMoveGraceFactor = 0.70f;
        private const float FlyHardUpSpeed = 4.5f;
        private const float FlyHorizontalMin = 0.1f;
        private const float FlyMinAirSeconds = 1.9f;
        private const float FlyGroundDistanceMin = 1.35f;
        private const float FlyNoGroundMinAirSeconds = 1.0f;
        private const float FlyNoGroundRiseMin = 1.2f;
        private const float FlyClimbGraceSeconds = 0.55f;
        private const float FlyClimbGraceHeight = 1.0f;
        private const float FlyNearGroundIgnore = 1.35f;
        private const float FlyHalfWallGraceSeconds = 0.95f;
        private const float FlyHalfWallGraceHeight = 1.55f;
        private const float FlyHalfWallGroundDistance = 2.35f;
        private const float FlyLadderRampGraceSeconds = 1.10f;
        private const float FlyLadderRampGraceHeight = 1.80f;
        private const float FlyWallClimbMinAirSeconds = 0.45f;
        private const float FlyWallClimbUpSpeed = 0.75f;
        private const float FlyWallClimbMaxHorizontal = 0.85f;
        private const int FlySetbacksBeforePenalty = 2;
        private const float FlyMinVlBeforePenalty = 5.0f;
        private const float FlyFireMinAirTime = 0.40f;
        private const float FlyFireCdPerShot = 7.5f;
        private const float PilotFireCdPerShot = 3.5f;
        private const float NoFallCdPerDetect = 2.2f;
        private const float SpiderMinAirSeconds = 0.65f;
        private const float SpiderMinRise = 0.85f;
        private const float SpiderMinUpSpeed = 0.35f;
        private const float SpiderMaxHorizontal = 0.80f;
        private const float SpiderGroundDistanceMin = 1.05f;
        private const float SpiderCdPerDetect = 2.2f;
        private const int SpiderSetbacksBeforePenalty = 2;
        private const float SpiderMinVlBeforePenalty = 2.0f;
        private const float HeadshotWindowSeconds = 95f;
        private const int HeadshotMinQualifiedHits = 14;
        private const int HeadshotMinHeadHits = 9;
        private const float HeadshotMinDistance = 38f;
        private const float HeadshotRatioThreshold = 0.74f;
        private const float HeadshotCdBase = 2.8f;
        private const float HeadshotCdRatioScale = 12f;
        private const float HeadPatternResetOnBodyDelay = 120f;
        private const int HeadPatternMinHeadsOnly = 8;
        private const float HeadPatternVlPerHead = 1.35f;
        private const float HeadPatternVlThreshold = 11.0f;
        private const float SuspiciousLookCheckInterval = 0.30f;
        private const float SuspiciousLookDotThreshold = 0.985f;
        private const float SuspiciousLookMinDistance = 8f;
        private const float SuspiciousLookMaxDistance = 140f;
        private const float SuspiciousLookScoreToLog = 4.5f;
        private const float SuspiciousLookLogCooldown = 300f;
        private const float SuspiciousLookDecayPerSecond = 0.65f;
        private const float ConnectGraceSeconds = 25f;
        private const float RespawnGraceSeconds = 12f;
        private const float MountChangeGraceSeconds = 8f;
        private const float TeleportGraceSeconds = 10f;
        private const float TeleportJumpDistance = 18f;
        private const float HighPingLogOnlyMs = 150f;
        private const float HighJitterLogOnlyMs = 55f;
        private const float HighPacketLossLogOnlyPercent = 8f;
        private const float HighServerFrameLogOnlyMs = 55f;
        private const float RecentNetworkInstabilityHoldSeconds = 20f;
        private const string BanAppealMessage = "AC: Если вы не согласны пишете тикет! А так вы заблокированы на {time}.";
        private const string BanDurationText = "0";
        private const float NoFallMinAirGap = 2.7f;
        private const float NoFallProbeMinAirGap = 1.2f;
        private const float NoFallMinImpactSpeed = 6.3f;
        private const float VlResetIdleSeconds = 600f;
        private const float PingHighMs = 120f;
        private const float PingVeryHighMs = 180f;
        private const float PingExtremeMs = 260f;
        private readonly Dictionary<ulong, MoveState> _states = new Dictionary<ulong, MoveState>();
        private readonly Dictionary<ulong, TargetTrack> _targetTracks = new Dictionary<ulong, TargetTrack>();
        private readonly HashSet<ulong> _accountChecked = new HashSet<ulong>();
        private readonly HashSet<ulong> _noDamageUsers = new HashSet<ulong>();
        private readonly Dictionary<ulong, List<float>> _repeatFlyNoFallHits = new Dictionary<ulong, List<float>>();
        private readonly Dictionary<ulong, float> _espCombatRevealUntil = new Dictionary<ulong, float>();
        private PluginConfig _config;

        private class DetectRule
        {
            public bool Enabled { get; set; } = true;
            public bool Ban { get; set; } = true;
            public float Threshold { get; set; } = 15f;
            public string Reason { get; set; } = "CD";
        }

        private class PluginConfig
        {
            public bool SimpleConfigEnabled { get; set; } = true;
            public string SimplePreset { get; set; } = "balanced";
            public float SimpleThresholdScale { get; set; } = 1f;
            public bool SimpleMovementDetections { get; set; } = true;
            public bool SimpleCombatDetections { get; set; } = true;
            public bool SimpleIdentityChecks { get; set; } = true;
            public string SteamApiKey { get; set; } = string.Empty;
            public string DiscordWebhookUrl { get; set; } = string.Empty;
            public string ProxyCheckApiKey { get; set; } = string.Empty;
            public bool ProxyCheckEnabled { get; set; } = true;
            public int ProxyRiskMin { get; set; } = 0;
            public string BanCommandTemplate { get; set; } = "ban {steamid} {reason} 0";
            public string KickCommandTemplate { get; set; } = "kick {steamid} {reason}";
            public bool RequirePublicProfile { get; set; } = true;
            public int MinAccountAgeDays { get; set; } = 14;
            public int RepeatFlyNoFallLimit { get; set; } = 3;
            public float RepeatFlyNoFallWindowHours { get; set; } = 24f;
            public string RepeatFlyNoFallReason { get; set; } = "Нарушение правил";
            public string RepeatFlyNoFallDuration { get; set; } = "14d";
            public bool EspNetworkFilterEnabled { get; set; } = true;
            public bool EspRequireLineOfSight { get; set; } = true;
            public float EspRevealDistance { get; set; } = 16f;
            public bool EspHideSleepers { get; set; } = true;
            public float EspHideSleepersDistance { get; set; } = 120f;
            public bool EspHideStashes { get; set; } = true;
            public float EspHideStashesDistance { get; set; } = 100f;
            public bool EspHideTraps { get; set; } = true;
            public float EspHideTrapsDistance { get; set; } = 95f;
            public bool EspHideLootContainers { get; set; } = false;
            public float EspHideLootDistance { get; set; } = 85f;
            public float EspCombatRevealSeconds { get; set; } = 20f;
            public Dictionary<string, DetectRule> RuleSet { get; set; } = new Dictionary<string, DetectRule>();
            public Dictionary<string, DetectRule> Rules { get { return RuleSet; } set { RuleSet = value; } }
            public Dictionary<string, DetectRule> Detects { get { return RuleSet; } set { RuleSet = value; } }

            public static PluginConfig CreateDefault()
            {
                var cfg = new PluginConfig();
                cfg.RuleSet["Fly"] = new DetectRule { Enabled = true, Ban = false, Threshold = 10f, Reason = "FLY" };
                cfg.RuleSet["NoFallDamage"] = new DetectRule { Enabled = true, Ban = true, Threshold = 6f, Reason = "NFD" };
                cfg.RuleSet["Spyder"] = new DetectRule { Enabled = true, Ban = true, Threshold = 8f, Reason = "SPD" };
                cfg.RuleSet["PilotFire"] = new DetectRule { Enabled = true, Ban = true, Threshold = 8f, Reason = "PFR" };
                cfg.RuleSet["Manipulator"] = new DetectRule { Enabled = true, Ban = true, Threshold = 20f, Reason = "MAN" };
                cfg.RuleSet["SilentAim"] = new DetectRule { Enabled = true, Ban = true, Threshold = 26f, Reason = "SLM" };
                cfg.RuleSet["FlyFire"] = new DetectRule { Enabled = true, Ban = true, Threshold = 20f, Reason = "FFR" };
                cfg.RuleSet["HeadshotRate"] = new DetectRule { Enabled = true, Ban = true, Threshold = 24f, Reason = "HSR" };
                cfg.RuleSet["Account"] = new DetectRule { Enabled = true, Ban = true, Threshold = 1f, Reason = "ACC" };
                cfg.RuleSet["ProxyBlock"] = new DetectRule { Enabled = true, Ban = true, Threshold = 1f, Reason = "PRX" };
                return cfg;
            }
        }

        private class MoveState
        {
            public Vector3 LastPosition;
            public Vector3 LastSafePosition;
            public float LastTime;
            public float LastGroundedTime;
            public float FlyVl;
            public float FlyCheatDetect;
            public float ManipulatorCheatDetect;
            public float SilentAimCheatDetect;
            public float FlyFireCheatDetect;
            public float NoFallDamageCheatDetect;
            public float PilotFireCheatDetect;
            public float HeadshotCheatDetect;
            public float SpiderCheatDetect;
            public float AirTime;
            public float LastSetbackTime;
            public float LastAdminNotifyTime;
            public float LastFallDamageTime;
            public float PendingFallLandTime;
            public float PendingFallCheckTime;
            public float PendingFallDistance;
            public float LastRangedShotTime;
            public float LastShotCaptureTime;
            public float AirStartY;
            public float MaxAirY;
            public float MaxAirGroundDistance;
            public float LastVerticalSpeed;
            public Vector3 LastShotOrigin;
            public Vector3 LastShotForward;
            public int FastFireStreak;
            public int SilentAimStrikesWindow;
            public int HeadshotQualifiedHitsWindow;
            public int HeadshotHeadHitsWindow;
            public int HeadshotStreak;
            public int HeadPatternOnlyHeadHits;
            public bool WasOnGround;
            public bool HasSafePosition;
            public bool Initialized;
            public bool LastShotValid;
            public bool LastShotIsBowLike;
            public bool AwaitingFallDamage;
            public float NoFallProbeUntil;
            public float DetectionPauseUntil;
            public bool PendingFallIsProbe;
            public bool PendingFallSecondCheckUsed;
            public int FlySetbackCount;
            public int SpiderSetbackCount;
            public float LastViolationTime;
            public float CachedPingMs;
            public float LastPingSampleTime;
            public float LastPingJitterMs;
            public float LastPacketLossPercent;
            public float SilentAimWindowStart;
            public float LastSilentAimTime;
            public float HeadshotWindowStart;
            public float PenaltyCooldownUntil;
            public bool LastMounted;
            public float ObservationStartTime;
            public float LastServerFrameMs;
            public Dictionary<string, float> DetectKindTimes;
            public Queue<string> RecentEvidence;
            public float LastShotInterval;
            public float LastNetworkUnstableTime;
            public float SpiderVl;
            public float HeadPatternVl;
            public float LastHeadPatternHeadTime;
            public Vector3 LastLookForward;
            public float SuspiciousLookScore;
            public float LastSuspiciousLookCheckTime;
            public float LastSuspiciousLookLogTime;
        }

        private class TargetTrack
        {
            public Vector3 LastPosition;
            public float LastTime;
            public Vector3 Velocity;
            public bool Initialized;
        }

        private void Init()
        {
            LoadConfigValues();
            permission.RegisterPermission("ac.bypass", this);
            permission.RegisterPermission("ac.notify", this);
            EnsureAdminBypassPermission();
        }

        private void OnServerInitialized()
        {
            EnsureAdminBypassPermission();
        }

        [ChatCommand("nofall")]
        private void CmdNoFall(BasePlayer caller, string command, string[] args)
        {
            if (!IsPlayerConnected(caller)) return;
            if (!caller.IsAdmin)
            {
                caller.ChatMessage("[AC] Admin only command.");
                return;
            }

            if (args == null || args.Length < 1)
            {
                caller.ChatMessage("[AC] Usage: /nofall <nick|steamid>");
                return;
            }

            var query = args[0];
            int matchCount;
            var target = FindOnlinePlayer(query, out matchCount);
            if (target == null)
            {
                caller.ChatMessage(matchCount > 1 ? "[AC] Multiple players found. Use exact nickname or steamid." : "[AC] Player not found online.");
                return;
            }

            MoveState state;
            if (!_states.TryGetValue(target.userID, out state))
            {
                state = new MoveState();
                _states[target.userID] = state;
            }

            var now = Time.realtimeSinceStartup;
            state.NoFallProbeUntil = now + NoFallCheckCommandDuration;
            state.DetectionPauseUntil = now + NoFallCommandDetectionsPause;
            state.AwaitingFallDamage = false;
            state.PendingFallIsProbe = false;
            state.PendingFallSecondCheckUsed = false;

            var launchFrom = target.transform.position;
            target.Teleport(launchFrom + Vector3.up * NoFallCommandLaunchHeight);

            if (caller.userID != target.userID)
            {
                caller.ChatMessage("[AC] NoFall check started for " + target.displayName + ". Player launched up, checks paused for " + NoFallCommandDetectionsPause.ToString("0") + "s.");
            }

            NotifyAdmins("[AC] Admin " + caller.displayName + " started NoFall check for " + target.displayName + " (" + target.UserIDString + ") and launched target up", target.userID);
        }

        [ChatCommand("nodm")]
        private void CmdNoDamage(BasePlayer caller, string command, string[] args)
        {
            if (!IsPlayerConnected(caller)) return;
            if (!caller.IsAdmin)
            {
                caller.ChatMessage("[AC] Admin only command.");
                return;
            }

            if (args == null || args.Length < 1)
            {
                caller.ChatMessage("[AC] Usage: /nodm <nick|steamid>");
                return;
            }

            int matchCount;
            var target = FindOnlinePlayer(args[0], out matchCount);
            if (target == null)
            {
                caller.ChatMessage(matchCount > 1 ? "[AC] Multiple players found. Use exact nickname or steamid." : "[AC] Player not found online.");
                return;
            }

            var enabled = !_noDamageUsers.Contains(target.userID);
            if (enabled)
            {
                _noDamageUsers.Add(target.userID);
                caller.ChatMessage("[AC] NoDamage ON for " + target.displayName + " (" + target.UserIDString + ")");
                NotifyAdmins("[AC] Admin " + caller.displayName + " enabled NoDamage for " + target.displayName + " (" + target.UserIDString + ")", target.userID);
            }
            else
            {
                _noDamageUsers.Remove(target.userID);
                caller.ChatMessage("[AC] NoDamage OFF for " + target.displayName + " (" + target.UserIDString + ")");
                NotifyAdmins("[AC] Admin " + caller.displayName + " disabled NoDamage for " + target.displayName + " (" + target.UserIDString + ")", target.userID);
            }
        }

        [ChatCommand("teamcheck")]
        private void CmdTeamCheck(BasePlayer caller, string command, string[] args)
        {
            if (!IsPlayerConnected(caller)) return;
            if (!caller.IsAdmin)
            {
                caller.ChatMessage("[AC] Admin only command.");
                return;
            }

            if (args == null || args.Length < 1)
            {
                caller.ChatMessage("[AC] Usage: /teamcheck <nick|steamid>");
                return;
            }

            int matchCount;
            var target = FindOnlinePlayer(args[0], out matchCount);
            if (target == null)
            {
                caller.ChatMessage(matchCount > 1 ? "[AC] Multiple players found. Use exact nickname or steamid." : "[AC] Player not found online.");
                return;
            }

            var team = RelationshipManager.Instance != null
                ? RelationshipManager.Instance.FindPlayersTeam(target.userID)
                : null;
            if (team == null || team.members == null || team.members.Count == 0)
            {
                caller.ChatMessage("[AC] " + target.displayName + " has no team.");
                return;
            }

            caller.ChatMessage("[AC] Team of " + target.displayName + " | TeamID=" + team.teamID + " | Members=" + team.members.Count);
            for (var i = 0; i < team.members.Count; i++)
            {
                var memberId = team.members[i];
                var online = BasePlayer.FindByID(memberId);
                var sleeping = BasePlayer.FindSleeping(memberId);
                var memberEntity = online != null ? online : sleeping;
                var memberName = memberEntity != null ? memberEntity.displayName : memberId.ToString();
                var status = IsPlayerConnected(online) ? "online" : "offline";
                caller.ChatMessage("[AC TEAM] " + memberName + " (" + memberId + ") - " + status);
            }
        }

        protected override void LoadDefaultConfig()
        {
            _config = PluginConfig.CreateDefault();
            SaveConfig();
        }

        protected override void SaveConfig()
        {
            if (_config == null) _config = PluginConfig.CreateDefault();
            Config.WriteObject(_config, true);
        }

        private void LoadConfigValues()
        {
            try
            {
                _config = Config.ReadObject<PluginConfig>();
            }
            catch
            {
                PrintWarning("Config error. Loading defaults.");
                LoadDefaultConfig();
            }

            if (_config == null)
            {
                _config = PluginConfig.CreateDefault();
            }

            var defaults = PluginConfig.CreateDefault();
            if (_config.RuleSet == null)
            {
                _config.RuleSet = defaults.RuleSet;
            }
            else
            {
                foreach (var pair in defaults.RuleSet)
                {
                    if (!_config.RuleSet.ContainsKey(pair.Key))
                    {
                        _config.RuleSet[pair.Key] = pair.Value;
                    }
                }
            }

            foreach (var pair in defaults.RuleSet)
            {
                DetectRule current;
                if (!_config.RuleSet.TryGetValue(pair.Key, out current) || current == null) continue;
                if (current.Threshold < pair.Value.Threshold)
                {
                    current.Threshold = pair.Value.Threshold;
                }
            }

            ApplySimpleConfig(defaults);

            DetectRule configuredRule;
            foreach (var pair in _config.RuleSet)
            {
                configuredRule = pair.Value;
                if (configuredRule == null) continue;
                configuredRule.Ban = !string.Equals(pair.Key, "Fly", StringComparison.Ordinal);
            }

            if (_config.MinAccountAgeDays < 14)
            {
                _config.MinAccountAgeDays = 14;
            }

            if (_config.RepeatFlyNoFallLimit < 2)
            {
                _config.RepeatFlyNoFallLimit = 2;
            }

            if (_config.RepeatFlyNoFallWindowHours < 1f)
            {
                _config.RepeatFlyNoFallWindowHours = 1f;
            }

            if (string.IsNullOrEmpty(_config.RepeatFlyNoFallReason))
            {
                _config.RepeatFlyNoFallReason = "Нарушение правил";
            }

            if (string.IsNullOrEmpty(_config.RepeatFlyNoFallDuration))
            {
                _config.RepeatFlyNoFallDuration = "14d";
            }

            if (_config.EspRevealDistance < 4f) _config.EspRevealDistance = 4f;
            if (_config.EspHideSleepersDistance < _config.EspRevealDistance) _config.EspHideSleepersDistance = _config.EspRevealDistance;
            if (_config.EspHideStashesDistance < _config.EspRevealDistance) _config.EspHideStashesDistance = _config.EspRevealDistance;
            if (_config.EspHideTrapsDistance < _config.EspRevealDistance) _config.EspHideTrapsDistance = _config.EspRevealDistance;
            if (_config.EspHideLootDistance < _config.EspRevealDistance) _config.EspHideLootDistance = _config.EspRevealDistance;
            if (_config.EspCombatRevealSeconds < 0f) _config.EspCombatRevealSeconds = 0f;

            SaveConfig();
        }

        private void ApplySimpleConfig(PluginConfig defaults)
        {
            if (_config == null || defaults == null || _config.RuleSet == null || defaults.RuleSet == null) return;
            if (!_config.SimpleConfigEnabled) return;

            var preset = (_config.SimplePreset ?? "balanced").Trim().ToLowerInvariant();
            var movementPresetScale = 1f;
            var combatPresetScale = 1f;
            var identityPresetScale = 1f;

            if (preset == "soft" || preset == "easy" || preset == "lite")
            {
                movementPresetScale = 1.30f;
                combatPresetScale = 1.20f;
                identityPresetScale = 1f;
            }
            else if (preset == "strict" || preset == "hard")
            {
                movementPresetScale = 0.85f;
                combatPresetScale = 0.90f;
                identityPresetScale = 1f;
            }
            else
            {
                _config.SimplePreset = "balanced";
            }

            var globalScale = Mathf.Clamp(_config.SimpleThresholdScale, 0.80f, 1.60f);
            _config.SimpleThresholdScale = globalScale;

            var movementEnabled = _config.SimpleMovementDetections;
            var combatEnabled = _config.SimpleCombatDetections;
            var identityEnabled = _config.SimpleIdentityChecks;

            foreach (var pair in defaults.RuleSet)
            {
                DetectRule current;
                if (!_config.RuleSet.TryGetValue(pair.Key, out current) || current == null)
                {
                    current = new DetectRule();
                    _config.RuleSet[pair.Key] = current;
                }

                var isMovement = string.Equals(pair.Key, "Fly", StringComparison.Ordinal)
                    || string.Equals(pair.Key, "NoFallDamage", StringComparison.Ordinal)
                    || string.Equals(pair.Key, "Spyder", StringComparison.Ordinal)
                    || string.Equals(pair.Key, "Spider", StringComparison.Ordinal);
                var isIdentity = string.Equals(pair.Key, "Account", StringComparison.Ordinal)
                    || string.Equals(pair.Key, "ProxyBlock", StringComparison.Ordinal);
                var groupEnabled = isMovement ? movementEnabled : (isIdentity ? identityEnabled : combatEnabled);
                current.Enabled = groupEnabled;

                var groupScale = isMovement ? movementPresetScale : (isIdentity ? identityPresetScale : combatPresetScale);
                var threshold = pair.Value.Threshold * groupScale * globalScale;
                if (isIdentity) threshold = Mathf.Max(1f, threshold);
                else threshold = Mathf.Max(0.5f, threshold);

                current.Threshold = threshold;
                current.Reason = pair.Value.Reason;
                current.Ban = !string.Equals(pair.Key, "Fly", StringComparison.Ordinal);
            }

            _config.ProxyCheckEnabled = identityEnabled;
            _config.RequirePublicProfile = identityEnabled;
        }

        private void EnsureAdminBypassPermission()
        {
            if (!permission.GroupExists("admin")) return;
            if (permission.GroupHasPermission("admin", "ac.bypass")) return;
            permission.GrantGroupPermission("admin", "ac.bypass", this);
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            _states.Remove(player.userID);
            _targetTracks.Remove(player.userID);
            _accountChecked.Remove(player.userID);
            _noDamageUsers.Remove(player.userID);
            _repeatFlyNoFallHits.Remove(player.userID);
            _espCombatRevealUntil.Remove(player.userID);
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null) return;
            MoveState state;
            if (!_states.TryGetValue(player.userID, out state))
            {
                state = new MoveState();
                _states[player.userID] = state;
            }

            state.Initialized = false;
            state.PenaltyCooldownUntil = Time.realtimeSinceStartup + RespawnGraceSeconds;
            state.ObservationStartTime = Time.realtimeSinceStartup;
            if (state.DetectKindTimes == null) state.DetectKindTimes = new Dictionary<string, float>();
            if (state.RecentEvidence == null) state.RecentEvidence = new Queue<string>();
            _targetTracks.Remove(player.userID);
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (!IsPlayerConnected(player)) return;
            if (permission.UserHasPermission(player.UserIDString, "ac.bypass")) return;
            if (_accountChecked.Contains(player.userID)) return;

            MoveState state;
            if (!_states.TryGetValue(player.userID, out state))
            {
                state = new MoveState();
                _states[player.userID] = state;
            }
            state.PenaltyCooldownUntil = Mathf.Max(state.PenaltyCooldownUntil, Time.realtimeSinceStartup + ConnectGraceSeconds);
            if (state.ObservationStartTime <= 0f) state.ObservationStartTime = Time.realtimeSinceStartup;
            if (state.DetectKindTimes == null) state.DetectKindTimes = new Dictionary<string, float>();
            if (state.RecentEvidence == null) state.RecentEvidence = new Queue<string>();

            _accountChecked.Add(player.userID);
            timer.Once(6f, () =>
            {
                CheckSteamAccount(player);
                CheckProxyBlock(player);
            });
        }

        private object OnPlayerAttack(BasePlayer attacker, HitInfo info)
        {
            if (!IsPlayerConnected(attacker) || info == null) return null;
            if (permission.UserHasPermission(attacker.UserIDString, "ac.bypass")) return null;

            MoveState state;
            if (!_states.TryGetValue(attacker.userID, out state))
            {
                state = new MoveState();
                _states[attacker.userID] = state;
            }
            var now = Time.realtimeSinceStartup;
            if (IsDetectionsPaused(state, now)) return null;

            if (!IsRangedAttack(info, attacker))
            {
                return null;
            }

            if (IsMountedDriver(attacker))
            {
                var msg = "[AC DETECT] " + attacker.displayName + " (" + attacker.UserIDString + ") -> PilotFire(driver seat)";
                NotifyAdmins(msg);
                state.PilotFireCheatDetect = Mathf.Max(VlMin, state.PilotFireCheatDetect + ApplyPingLeniency(attacker, PilotFireCdPerShot));
                var pilotEvidence = "seat=driver heli=true shotDt=" + state.LastShotInterval.ToString("0.000") + "s";
                var blockPilot = HandlePenalty(attacker, "PilotFire", state.PilotFireCheatDetect, "PilotFire", false, pilotEvidence);
                if (blockPilot) return true;
            }

            if (CheckFlyFire(attacker, state))
            {
                return true;
            }

            CaptureShotSnapshot(attacker, state);

            return null;
        }

        private object OnEntityTakeDamage(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null || info.damageTypes == null) return null;

            var victim = entity as BasePlayer;
            if (victim == null) return null;

            MarkEspCombatReveal(victim);

            if (_noDamageUsers.Contains(victim.userID))
            {
                return true;
            }

            if (info.damageTypes.Get(Rust.DamageType.Fall) > 0f)
            {
                MoveState fallState;
                if (!_states.TryGetValue(victim.userID, out fallState))
                {
                    fallState = new MoveState();
                    _states[victim.userID] = fallState;
                }

                fallState.LastFallDamageTime = Time.realtimeSinceStartup;
                fallState.AwaitingFallDamage = false;
                fallState.PendingFallIsProbe = false;
                fallState.PendingFallSecondCheckUsed = false;
            }

            var attacker = info.InitiatorPlayer;
            MarkEspCombatReveal(attacker);
            if (!IsPlayerConnected(attacker) || attacker == victim) return null;
            if (permission.UserHasPermission(attacker.UserIDString, "ac.bypass")) return null;

            MoveState state;
            if (!_states.TryGetValue(attacker.userID, out state))
            {
                state = new MoveState();
                _states[attacker.userID] = state;
            }
            if (IsDetectionsPaused(state, Time.realtimeSinceStartup)) return null;

            if (IsRangedAttack(info, attacker))
            {
                if (CheckManipulator(attacker, info, state, victim))
                {
                    return true;
                }

                if (CheckSilentAim(attacker, victim, state))
                {
                    return true;
                }

                if (CheckHeadshotRate(attacker, victim, state, info))
                {
                    return true;
                }

                return null;
            }

            return null;
        }

        private object CanNetworkTo(BaseNetworkable entity, BasePlayer target)
        {
            if (_config == null || !_config.EspNetworkFilterEnabled) return null;
            if (!IsPlayerConnected(target)) return null;
            if (permission.UserHasPermission(target.UserIDString, "ac.bypass")) return null;
            if (entity == null) return null;

            var baseEntity = entity as BaseEntity;
            if (baseEntity == null) return null;
            if (baseEntity == target) return null;

            var sleeper = baseEntity as BasePlayer;
            if (sleeper != null)
            {
                if (!_config.EspHideSleepers) return null;
                if (!sleeper.IsSleeping()) return null;
                if (sleeper.userID == target.userID) return null;
                if (ArePlayersInSameTeam(sleeper.userID, target.userID)) return null;
                if (ShouldHideEntityForViewer(target, sleeper.transform.position + Vector3.up * 1.05f, _config.EspHideSleepersDistance))
                {
                    return false;
                }

                return null;
            }

            var shortName = baseEntity.ShortPrefabName;
            if (string.IsNullOrEmpty(shortName)) return null;
            shortName = shortName.ToLowerInvariant();

            if (baseEntity.OwnerID != 0 && (baseEntity.OwnerID == target.userID || ArePlayersInSameTeam(baseEntity.OwnerID, target.userID)))
            {
                return null;
            }

            if (_config.EspHideStashes && IsEspStash(shortName))
            {
                if (ShouldHideEntityForViewer(target, baseEntity.transform.position + Vector3.up * 0.2f, _config.EspHideStashesDistance))
                {
                    return false;
                }
            }

            if (_config.EspHideTraps && IsEspTrap(shortName))
            {
                if (ShouldHideEntityForViewer(target, baseEntity.transform.position + Vector3.up * 0.55f, _config.EspHideTrapsDistance))
                {
                    return false;
                }
            }

            if (_config.EspHideLootContainers && IsEspLootContainer(shortName))
            {
                if (ShouldHideEntityForViewer(target, baseEntity.transform.position + Vector3.up * 0.35f, _config.EspHideLootDistance))
                {
                    return false;
                }
            }

            return null;
        }

        private void OnPlayerTick(BasePlayer player, PlayerTick tick)
        {
            if (!IsPlayerConnected(player) || player.IsDead() || player.IsSleeping()) return;
            if (permission.UserHasPermission(player.UserIDString, "ac.bypass")) return;

            var now = Time.realtimeSinceStartup;
            var position = player.transform.position;

            MoveState state;
            if (!_states.TryGetValue(player.userID, out state))
            {
                state = new MoveState();
                _states[player.userID] = state;
            }

            if (!state.Initialized)
            {
                state.Initialized = true;
                state.LastPosition = position;
                state.LastSafePosition = position;
                state.HasSafePosition = true;
                state.LastTime = now;
                state.LastGroundedTime = now;
                state.AirStartY = position.y;
                state.MaxAirY = position.y;
                state.MaxAirGroundDistance = 0f;
                state.WasOnGround = player.IsOnGround();
                state.LastMounted = player.isMounted;
                state.PenaltyCooldownUntil = now + RespawnGraceSeconds;
                state.ObservationStartTime = now;
                state.LastLookForward = player.eyes != null ? player.eyes.BodyForward() : player.transform.forward;
                if (state.DetectKindTimes == null) state.DetectKindTimes = new Dictionary<string, float>();
                if (state.RecentEvidence == null) state.RecentEvidence = new Queue<string>();
                return;
            }

            var dt = Mathf.Clamp(now - state.LastTime, 0.01f, 0.5f);
            var delta = position - state.LastPosition;
            var horizontal = new Vector3(delta.x, 0f, delta.z).magnitude;
            var vertical = delta.y;
            state.LastVerticalSpeed = vertical / dt;
            state.LastServerFrameMs = Mathf.Max(0f, Time.smoothDeltaTime * 1000f);

            if (state.LastPingSampleTime <= 0f || (now - state.LastPingSampleTime) >= 1f)
            {
                var oldPing = state.CachedPingMs;
                state.CachedPingMs = GetPlayerPingMs(player);
                state.LastPingJitterMs = Mathf.Abs(state.CachedPingMs - oldPing);
                state.LastPacketLossPercent = GetPlayerPacketLossPercent(player);
                state.LastPingSampleTime = now;
            }

            if (IsNetworkLogOnly(state))
            {
                state.LastNetworkUnstableTime = now;
            }

            if (delta.magnitude >= TeleportJumpDistance && dt <= 0.30f)
            {
                state.PenaltyCooldownUntil = Mathf.Max(state.PenaltyCooldownUntil, now + TeleportGraceSeconds);
            }

            var mountedNow = player.isMounted;
            if (mountedNow != state.LastMounted)
            {
                state.LastMounted = mountedNow;
                state.PenaltyCooldownUntil = Mathf.Max(state.PenaltyCooldownUntil, now + MountChangeGraceSeconds);
            }

            if (state.LastViolationTime > 0f && (now - state.LastViolationTime) >= VlResetIdleSeconds && HasAnyViolation(state))
            {
                ResetAllViolations(state);
            }

            UpdateTargetTrack(player, now, position);

            var onGround = player.IsOnGround();
            var swimming = player.IsSwimming() || (player.modelState != null && player.modelState.waterLevel > 0.4f);
            var inTransport = IsInTransport(player);
            var onLadder = IsOnLadder(player);
            var safeState = onGround || swimming || inTransport || onLadder;
            var detectionsPaused = IsDetectionsPaused(state, now);

            if (state.AwaitingFallDamage)
            {
                if (state.LastFallDamageTime >= (state.PendingFallLandTime - NoFallDamageTimeTolerance))
                {
                    state.AwaitingFallDamage = false;
                    state.PendingFallIsProbe = false;
                    state.PendingFallSecondCheckUsed = false;
                }
                else if (now >= state.PendingFallCheckTime)
                {
                    if (!state.PendingFallSecondCheckUsed)
                    {
                        state.PendingFallSecondCheckUsed = true;
                        state.PendingFallCheckTime = now + NoFallSecondCheckWindow;
                    }
                    else
                    {
                        var probeTag = state.PendingFallIsProbe ? " (manual)" : string.Empty;
                        var actionTag = state.PendingFallIsProbe ? "INSTANT_BAN" : "VL_TRACK";
                        var msg = "[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> NoFallDamage" + probeTag + " fall=" + state.PendingFallDistance.ToString("0.0") + "m action=" + actionTag;
                        NotifyAdmins(msg);
                        state.NoFallDamageCheatDetect = Mathf.Max(VlMin, state.NoFallDamageCheatDetect + ApplyPingLeniency(player, NoFallCdPerDetect));
                        var nofallEvidence = "fall=" + state.PendingFallDistance.ToString("0.0") + "m impact=" + Mathf.Abs(state.LastVerticalSpeed).ToString("0.0");
                        if (state.PendingFallIsProbe)
                        {
                            var noFallRule = GetRule("NoFallDamage");
                            var reasonCode = (noFallRule == null || string.IsNullOrEmpty(noFallRule.Reason)) ? "NFD" : noFallRule.Reason;
                            RecordEvidence(state, "NoFallDamage", player, "NoFallDamage(ManualProbeFail)", state.NoFallDamageCheatDetect, nofallEvidence, now, 0.99f);
                            BanPlayer(player, reasonCode, state.NoFallDamageCheatDetect, "NoFallDamage(ManualProbeFail)", BuildEvidenceBundle(state));
                        }
                        else
                        {
                            HandlePenalty(player, "NoFallDamage", state.NoFallDamageCheatDetect, "NoFallDamage", false, nofallEvidence, 0.92f);
                        }
                        state.AwaitingFallDamage = false;
                        state.PendingFallIsProbe = false;
                        state.PendingFallSecondCheckUsed = false;
                        state.WasOnGround = onGround;
                        state.LastPosition = position;
                        state.LastTime = now;
                        return;
                    }
                }
            }

            if (!safeState)
            {
                if (state.WasOnGround || state.AirTime <= 0.001f)
                {
                    state.AirStartY = position.y;
                    state.MaxAirY = position.y;
                    state.MaxAirGroundDistance = 0f;
                }
                else if (position.y > state.MaxAirY)
                {
                    state.MaxAirY = position.y;
                }

                float currentAirGroundDistance;
                if (TryGetGroundDistance(player, out currentAirGroundDistance))
                {
                    if (currentAirGroundDistance > state.MaxAirGroundDistance)
                    {
                        state.MaxAirGroundDistance = currentAirGroundDistance;
                    }
                }
            }

            if (onGround && !state.WasOnGround && !swimming && !inTransport && !onLadder)
            {
                var fallDistance = state.MaxAirY - position.y;
                var probeActive = state.NoFallProbeUntil > now;
                var requiredDistance = probeActive ? NoFallCommandMinDistance : NoFallMinDistance;
                var requiredAirGap = probeActive ? NoFallProbeMinAirGap : NoFallMinAirGap;
                var impactSpeed = Mathf.Abs(state.LastVerticalSpeed);
                var impactOk = probeActive || impactSpeed >= NoFallMinImpactSpeed;
                var hadRecentFallDamage = state.LastFallDamageTime > 0f && (now - state.LastFallDamageTime) <= NoFallDamageTimeTolerance;
                if (fallDistance >= requiredDistance && state.MaxAirGroundDistance >= requiredAirGap && impactOk && !hadRecentFallDamage)
                {
                    state.AwaitingFallDamage = true;
                    state.PendingFallLandTime = now;
                    state.PendingFallCheckTime = now + NoFallDamageWindow;
                    state.PendingFallDistance = fallDistance;
                    state.PendingFallIsProbe = probeActive;
                    state.PendingFallSecondCheckUsed = false;
                }
            }

            if (safeState)
            {
                state.LastSafePosition = position;
                state.HasSafePosition = true;
                state.AirTime = 0f;
                state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.9f * dt);
                state.FlyCheatDetect = Mathf.Max(VlMin, state.FlyCheatDetect - 0.18f * dt);
                state.ManipulatorCheatDetect = Mathf.Max(VlMin, state.ManipulatorCheatDetect - 0.10f * dt);
                state.SilentAimCheatDetect = Mathf.Max(VlMin, state.SilentAimCheatDetect - 0.10f * dt);
                state.FlyFireCheatDetect = Mathf.Max(VlMin, state.FlyFireCheatDetect - 0.10f * dt);
                state.NoFallDamageCheatDetect = Mathf.Max(VlMin, state.NoFallDamageCheatDetect - 0.07f * dt);
                state.PilotFireCheatDetect = Mathf.Max(VlMin, state.PilotFireCheatDetect - 0.09f * dt);
                state.HeadshotCheatDetect = Mathf.Max(VlMin, state.HeadshotCheatDetect - 0.08f * dt);
                state.SpiderCheatDetect = Mathf.Max(VlMin, state.SpiderCheatDetect - 0.10f * dt);
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.25f * dt);
                state.HeadPatternVl = Mathf.Max(0f, state.HeadPatternVl - 0.02f * dt);
                state.LastGroundedTime = now;

                if (onGround)
                {
                    state.MaxAirGroundDistance = 0f;
                    state.FlySetbackCount = 0;
                    state.SpiderSetbackCount = 0;
                }
            }
            else if (!detectionsPaused)
            {
                CheckFlyHack(player, state, now, dt, horizontal, vertical, state.CachedPingMs);
                CheckSpiderHack(player, state, now, dt, horizontal, vertical, state.CachedPingMs);
            }

            if (!detectionsPaused)
            {
            }

            CheckSuspiciousLookPattern(player, state, now, dt);

            state.LastPosition = position;
            state.LastTime = now;
            state.WasOnGround = onGround;
        }

        private void CheckSuspiciousLookPattern(BasePlayer player, MoveState state, float now, float dt)
        {
            if (!IsPlayerConnected(player) || state == null) return;

            state.SuspiciousLookScore = Mathf.Max(0f, state.SuspiciousLookScore - SuspiciousLookDecayPerSecond * dt);

            if ((now - state.LastSuspiciousLookCheckTime) < SuspiciousLookCheckInterval)
            {
                return;
            }

            state.LastSuspiciousLookCheckTime = now;

            var look = player.eyes != null ? player.eyes.BodyForward() : player.transform.forward;
            if (look.sqrMagnitude < 0.0001f) return;
            look.Normalize();

            if (state.LastLookForward.sqrMagnitude < 0.0001f)
            {
                state.LastLookForward = look;
            }

            var turnRate = Vector3.Angle(state.LastLookForward, look) / Mathf.Max(0.01f, dt);
            state.LastLookForward = look;

            BasePlayer target;
            float targetDistance;
            float targetDot;
            if (!TryFindBestLookTarget(player, look, out target, out targetDistance, out targetDot))
            {
                return;
            }

            if (ArePlayersInSameTeam(player.userID, target.userID))
            {
                state.SuspiciousLookScore = Mathf.Max(0f, state.SuspiciousLookScore - 0.45f);
                return;
            }

            if (!IsCurrentViewBlocked(player, target))
            {
                state.SuspiciousLookScore = Mathf.Max(0f, state.SuspiciousLookScore - 0.35f);
                return;
            }

            var addScore = 0.70f + Mathf.Clamp((targetDot - SuspiciousLookDotThreshold) * 20f, 0f, 2.4f);
            if (turnRate > 220f) addScore += 0.70f;
            if (turnRate > 420f) addScore += 0.95f;
            state.SuspiciousLookScore = Mathf.Clamp(state.SuspiciousLookScore + addScore, 0f, 12f);

            if (state.SuspiciousLookScore < SuspiciousLookScoreToLog)
            {
                return;
            }

            if ((now - state.LastSuspiciousLookLogTime) < SuspiciousLookLogCooldown)
            {
                return;
            }

            state.LastSuspiciousLookLogTime = now;
            var message = "[AC DETECT_LOOK] " + player.displayName + " (" + player.UserIDString + ")"
                + " -> SuspiciousLook"
                + " dot=" + targetDot.ToString("0.000")
                + " dist=" + targetDistance.ToString("0.0")
                + " turn=" + turnRate.ToString("0") + "deg/s"
                + " score=" + state.SuspiciousLookScore.ToString("0.0")
                + " target=" + target.displayName + " (" + target.UserIDString + ")";
            NotifyAdmins(message);
        }

        private bool TryFindBestLookTarget(BasePlayer viewer, Vector3 lookForward, out BasePlayer bestTarget, out float bestDistance, out float bestDot)
        {
            bestTarget = null;
            bestDistance = 0f;
            bestDot = -1f;

            if (!IsPlayerConnected(viewer)) return false;

            var eyePos = viewer.eyes != null ? viewer.eyes.position : viewer.transform.position + Vector3.up * 1.5f;
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var target = BasePlayer.activePlayerList[i];
                if (!IsPlayerConnected(target)) continue;
                if (target == viewer || target.IsDead() || target.IsSleeping()) continue;

                var targetPos = target.transform.position + Vector3.up * 1.35f;
                var toTarget = targetPos - eyePos;
                var distance = toTarget.magnitude;
                if (distance < SuspiciousLookMinDistance || distance > SuspiciousLookMaxDistance) continue;

                var dir = toTarget / distance;
                var dot = Vector3.Dot(lookForward, dir);
                if (dot < SuspiciousLookDotThreshold) continue;

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget != null;
        }

        private void CheckFlyHack(BasePlayer player, MoveState state, float now, float dt, float horizontal, float vertical, float pingMs)
        {
            state.AirTime += dt;
            var upSpeed = vertical / dt;
            var pingFactor = GetPingThresholdFactor(pingMs);
            var hardUpSpeed = FlyHardUpSpeed * pingFactor;
            var sustainUpSpeed = 1.05f * pingFactor;
            var wallUpSpeed = FlyWallClimbUpSpeed * pingFactor;
            var minAirSeconds = FlyMinAirSeconds * pingFactor;

            var sinceGrounded = now - state.LastGroundedTime;
            if (sinceGrounded < JumpGraceSeconds)
            {
                state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.25f);
                return;
            }

            float groundDistance;
            var hasGround = TryGetGroundDistance(player, out groundDistance);
            var highAboveGround = hasGround && groundDistance >= FlyGroundDistanceMin;
            var riseFromTakeoff = Mathf.Max(0f, state.MaxAirY - state.AirStartY);

            if (state.AirTime <= FlyClimbGraceSeconds
                && riseFromTakeoff <= FlyClimbGraceHeight
                && hasGround
                && groundDistance <= FlyNearGroundIgnore
                && IsNearSmallClimbable(player))
            {
                state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.35f);
                return;
            }

            if (state.AirTime <= FlyHalfWallGraceSeconds
                && riseFromTakeoff <= FlyHalfWallGraceHeight
                && hasGround
                && groundDistance <= FlyHalfWallGroundDistance
                && IsNearHalfWallOrLowLedge(player))
            {
                state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.35f);
                return;
            }

            // Extra grace for legit ladder/ramp transfers where player briefly rises in air.
            if (state.AirTime <= FlyLadderRampGraceSeconds
                && riseFromTakeoff <= FlyLadderRampGraceHeight
                && hasGround
                && groundDistance <= (FlyHalfWallGroundDistance + 0.55f)
                && IsNearLadderOrRamp(player))
            {
                state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.40f);
                return;
            }

            var enoughFlyContext = highAboveGround || riseFromTakeoff > 1.1f;
            if (upSpeed > hardUpSpeed && horizontal >= FlyHorizontalMin && enoughFlyContext)
            {
                state.FlyVl += (upSpeed - hardUpSpeed) * 1.05f + 1.45f * dt;
                HandleFlyViolation(player, state, "Flyhack(UpSpeed)");
                return;
            }

            if (state.AirTime > (0.55f * pingFactor) && upSpeed > sustainUpSpeed && (highAboveGround || riseFromTakeoff >= 1.3f))
            {
                state.FlyVl += 2.8f * dt + Mathf.Clamp(upSpeed * 0.28f, 0f, 1.15f);
                HandleFlyViolation(player, state, "Flyhack(SustainUp)");
                return;
            }

            var wallContext = highAboveGround || !hasGround || riseFromTakeoff >= 0.55f;
            if (wallContext
                && state.AirTime >= FlyWallClimbMinAirSeconds
                && upSpeed >= wallUpSpeed
                && horizontal <= FlyWallClimbMaxHorizontal
                && IsNearWallSurface(player))
            {
                state.FlyVl += 2.9f * dt + Mathf.Clamp(upSpeed * 0.45f, 0f, 1.4f);
                HandleFlyViolation(player, state, "Flyhack(WallClimb)");
                return;
            }

            if (state.AirTime >= 0.90f
                && riseFromTakeoff >= 1.15f
                && upSpeed >= 0.45f
                && horizontal <= 0.45f
                && (highAboveGround || !hasGround)
                && IsNearWallSurface(player))
            {
                state.FlyVl += 2.2f * dt + Mathf.Clamp(upSpeed * 0.22f, 0f, 0.8f);
                HandleFlyViolation(player, state, "Flyhack(WallClimbSlow)");
                return;
            }

            if (!hasGround && state.AirTime >= FlyNoGroundMinAirSeconds && riseFromTakeoff >= FlyNoGroundRiseMin)
            {
                state.FlyVl += 1.9f * dt + Mathf.Clamp(upSpeed * 0.20f, 0f, 0.9f);
                HandleFlyViolation(player, state, "Flyhack(NoGround)");
                return;
            }

            if (!hasGround && state.AirTime >= 1.25f && Mathf.Abs(upSpeed) <= 0.08f)
            {
                state.FlyVl += 2.1f * dt;
                HandleFlyViolation(player, state, "Flyhack(NoGroundHover)");
                return;
            }

            if (state.AirTime >= minAirSeconds && highAboveGround)
            {
                var hovering = Mathf.Abs(vertical) <= 0.03f;
                state.FlyVl += (hovering ? 2.8f : 2.1f) * dt;
                if (upSpeed > 0.25f) state.FlyVl += 0.2f;
                HandleFlyViolation(player, state, "Flyhack(AirTime)");
                return;
            }

            state.FlyVl = Mathf.Max(0f, state.FlyVl - 0.2f);
        }

        private void HandleFlyViolation(BasePlayer player, MoveState state, string reason)
        {
            if (!IsPlayerConnected(player)) return;

            var didSetback = TrySetBack(player, state);
            if (didSetback)
            {
                state.FlySetbackCount += 1;
            }

            var addCd = ApplyPingLeniency(player, 2.0f);
            state.FlyCheatDetect = Mathf.Max(VlMin, state.FlyCheatDetect + addCd);

            var detectMsg = string.Format(
                "[AC DETECT] {0} ({1}) -> {2} VL={3:0.0} CheatDetect(Fly)={4:0.0} Setbacks={5}/{6}",
                player.displayName,
                player.UserIDString,
                reason,
                state.FlyVl,
                state.FlyCheatDetect,
                state.FlySetbackCount,
                FlySetbacksBeforePenalty);
            NotifyAdminsThrottled(state, detectMsg);

            if (state.FlySetbackCount < FlySetbacksBeforePenalty || state.FlyVl < FlyMinVlBeforePenalty)
            {
                return;
            }

            HandlePenalty(player, "Fly", state.FlyCheatDetect, reason, false, "air=" + state.AirTime.ToString("0.00") + "s vUp=" + state.LastVerticalSpeed.ToString("0.00"));
        }

        private void CheckSpiderHack(BasePlayer player, MoveState state, float now, float dt, float horizontal, float vertical, float pingMs)
        {
            if (!IsPlayerConnected(player) || state == null) return;

            var pingFactor = GetPingThresholdFactor(pingMs);
            var minAir = SpiderMinAirSeconds * pingFactor;
            if (state.AirTime < minAir)
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.18f);
                return;
            }

            var riseFromTakeoff = Mathf.Max(0f, state.MaxAirY - state.AirStartY);
            if (riseFromTakeoff < SpiderMinRise)
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.18f);
                return;
            }

            if (!IsNearWallSurface(player))
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.24f);
                return;
            }

            // Ignore legit climbs on short structures/deployables.
            if (IsNearSmallClimbable(player) || IsNearHalfWallOrLowLedge(player) || IsNearLadderOrRamp(player))
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.40f);
                return;
            }

            float groundDistance;
            var hasGround = TryGetGroundDistance(player, out groundDistance);
            if (!hasGround || groundDistance < SpiderGroundDistanceMin)
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.22f);
                return;
            }

            var upSpeed = vertical / Mathf.Max(0.01f, dt);
            if (upSpeed < SpiderMinUpSpeed * pingFactor || horizontal > SpiderMaxHorizontal)
            {
                state.SpiderVl = Mathf.Max(0f, state.SpiderVl - 0.16f);
                return;
            }

            state.SpiderVl += 2.4f * dt + Mathf.Clamp((upSpeed - SpiderMinUpSpeed) * 1.3f, 0f, 1.6f);
            HandleSpiderViolation(player, state, "SpiderHack(WallCrawl)", upSpeed, horizontal, groundDistance, riseFromTakeoff);
        }

        private void HandleSpiderViolation(BasePlayer player, MoveState state, string reason, float upSpeed, float horizontal, float groundDistance, float rise)
        {
            if (!IsPlayerConnected(player) || state == null) return;

            var didSetback = TrySetBack(player, state);
            if (didSetback)
            {
                state.SpiderSetbackCount++;
            }

            var addCd = ApplyPingLeniency(player, SpiderCdPerDetect);
            state.SpiderCheatDetect = Mathf.Max(VlMin, state.SpiderCheatDetect + addCd);

            var detectMsg = string.Format(
                "[AC DETECT] {0} ({1}) -> {2} SVL={3:0.0} CD={4:0.0} Setbacks={5}/{6}",
                player.displayName,
                player.UserIDString,
                reason,
                state.SpiderVl,
                state.SpiderCheatDetect,
                state.SpiderSetbackCount,
                SpiderSetbacksBeforePenalty);
            NotifyAdminsThrottled(state, detectMsg);

            if (state.SpiderSetbackCount < SpiderSetbacksBeforePenalty || state.SpiderVl < SpiderMinVlBeforePenalty)
            {
                return;
            }

            var evidence = "up=" + upSpeed.ToString("0.00")
                + " hor=" + horizontal.ToString("0.00")
                + " ground=" + groundDistance.ToString("0.00")
                + " rise=" + rise.ToString("0.00");
            HandlePenalty(player, "Spyder", state.SpiderCheatDetect, reason, false, evidence, 0.90f);
        }

        private bool TrySetBack(BasePlayer player, MoveState state)
        {
            if (!state.HasSafePosition) return false;

            var now = Time.realtimeSinceStartup;
            if (now - state.LastSetbackTime < SetbackCooldown) return false;
            if (Vector3.Distance(player.transform.position, state.LastSafePosition) < 0.2f) return false;

            state.LastSetbackTime = now;
            player.Teleport(state.LastSafePosition);
            return true;
        }

        private bool CheckManipulator(BasePlayer attacker, HitInfo info, MoveState state, BasePlayer victim)
        {
            if (victim == null) return false;
            if (IsManipulatorExcludedWeapon(attacker, info)) return false;

            var now = Time.realtimeSinceStartup;
            var addCd = 0f;
            var reason = string.Empty;

            var shotDelay = GetExpectedShotDelay(attacker);
            var shotDt = state.LastRangedShotTime > 0f ? (now - state.LastRangedShotTime) : 999f;
            state.LastRangedShotTime = now;

            var fastFactor = GetFastFireFactor(attacker);
            var fastNeed = GetFastFireStreakNeed(attacker);
            var fastThreshold = shotDelay * fastFactor;
            var isFastShot = shotDt > 0.001f && shotDt < fastThreshold && (shotDelay - shotDt) > ManipulatorFastShotMinMargin;

            if (isFastShot)
            {
                state.FastFireStreak++;
                if (state.FastFireStreak >= fastNeed)
                {
                    addCd += 3.2f + (state.FastFireStreak - fastNeed) * 0.8f;
                    reason = "Manipulator(FastFire x" + state.FastFireStreak + ")";
                }
            }
            else
            {
                if (shotDt >= shotDelay * 0.92f || shotDt > shotDelay * 1.35f)
                {
                    state.FastFireStreak = 0;
                }
                else if (state.FastFireStreak > 0)
                {
                    state.FastFireStreak--;
                }
            }

            var throughWall = IsShotThroughObstacle(attacker, info, victim);
            if (throughWall)
            {
                // Through-wall requires an additional independent sign
                // (impossible shot angle/path), otherwise ignore to reduce false flags.
                if (IsManipulatorImpossibleTrajectory(attacker, victim, state, info))
                {
                    addCd += 6.8f;
                    reason = string.IsNullOrEmpty(reason) ? "Manipulator(ThroughWall)" : "Manipulator(ThroughWall+FastFire)";
                }
            }

            if (addCd <= 0f)
            {
                state.ManipulatorCheatDetect = Mathf.Max(VlMin, state.ManipulatorCheatDetect - 0.25f);
                return false;
            }

            addCd = ApplyPingLeniency(attacker, addCd);
            state.ManipulatorCheatDetect = Mathf.Max(VlMin, state.ManipulatorCheatDetect + addCd);
            var detectMsg = string.Format(
                "[AC DETECT] {0} ({1}) -> {2} +{3:0.0}CD CheatDetect(Manipulator)={4:0.0}",
                attacker.displayName,
                attacker.UserIDString,
                reason,
                addCd,
                state.ManipulatorCheatDetect);
            NotifyAdminsThrottled(state, detectMsg);

            var manipEvidence = "shotDt=" + shotDt.ToString("0.000") + "s streak=" + state.FastFireStreak + " throughWall=" + throughWall;
            return HandlePenalty(attacker, "Manipulator", state.ManipulatorCheatDetect, reason, false, manipEvidence);
        }

        private float GetFastFireFactor(BasePlayer attacker)
        {
            if (IsBowLikeWeapon(attacker)) return ManipulatorFastShotFactorBow;
            if (IsSemiAutoClickWeapon(attacker)) return ManipulatorFastShotFactorSemi;
            return ManipulatorFastShotFactorDefault;
        }

        private int GetFastFireStreakNeed(BasePlayer attacker)
        {
            if (IsBowLikeWeapon(attacker)) return 3;
            if (IsSemiAutoClickWeapon(attacker)) return 3;
            return 3;
        }

        private bool CheckFlyFire(BasePlayer attacker, MoveState state)
        {
            if (attacker == null || state == null) return false;
            if (!state.Initialized) return false;
            // Bows and crossbows can legitimately shoot while jumping.
            if (IsBowLikeWeapon(attacker)) return false;
            if (attacker.IsOnGround()) return false;
            if (attacker.IsSwimming()) return false;
            if (IsInTransport(attacker) || IsOnLadder(attacker)) return false;

            var rule = GetRule("FlyFire");
            if (rule == null || !rule.Enabled) return false;

            var now = Time.realtimeSinceStartup;
            var sinceGrounded = now - state.LastGroundedTime;
            if (sinceGrounded < FlyFireMinAirTime) return false;

            var addCd = ApplyPingLeniency(attacker, FlyFireCdPerShot);
            state.FlyFireCheatDetect = Mathf.Max(VlMin, state.FlyFireCheatDetect + addCd);

            var detectMsg = "[AC DETECT] " + attacker.displayName + " (" + attacker.UserIDString + ") -> FlyFire(ShotInJump) CheatDetect(FlyFire)=" + state.FlyFireCheatDetect.ToString("0.0");
            NotifyAdminsThrottled(state, detectMsg);

            var flyFireEvidence = "airTime=" + sinceGrounded.ToString("0.00") + "s shotDt=" + state.LastShotInterval.ToString("0.000") + "s";
            if (state.FlyFireCheatDetect < rule.Threshold)
            {
                return false;
            }

            state.LastViolationTime = now;
            RecordEvidence(state, "FlyFire", attacker, "FlyFire(DamageBlock)", state.FlyFireCheatDetect, flyFireEvidence, now, 0.82f);
            NotifyAdminsThrottled(state, FormatAdminDetectLine("DETECT", attacker, "FlyFire", "FlyFire", state.FlyFireCheatDetect, rule.Threshold, "DMG_BLOCK", state, flyFireEvidence));
            return true;
        }

        private bool CheckHeadshotRate(BasePlayer attacker, BasePlayer victim, MoveState state, HitInfo info)
        {
            if (attacker == null || victim == null || state == null || info == null) return false;
            if (!IsHeadshotSupportedWeapon(attacker)) return false;

            var now = Time.realtimeSinceStartup;
            var isHead = IsHeadHit(info);

            if (!isHead && state.LastHeadPatternHeadTime > 0f && (now - state.LastHeadPatternHeadTime) >= HeadPatternResetOnBodyDelay)
            {
                ResetHeadshotTracking(state, true);
            }

            if (state.HeadshotWindowStart <= 0f || (now - state.HeadshotWindowStart) > HeadshotWindowSeconds)
            {
                state.HeadshotWindowStart = now;
                state.HeadshotQualifiedHitsWindow = 0;
                state.HeadshotHeadHitsWindow = 0;
                state.HeadshotStreak = 0;
            }

            var eyePos = attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f;
            var targetPos = victim.transform.position + Vector3.up * 1.35f;
            var distance = Vector3.Distance(eyePos, targetPos);
            if (distance < HeadshotMinDistance) return false;

            state.HeadshotQualifiedHitsWindow++;
            if (isHead)
            {
                state.HeadshotHeadHitsWindow++;
                state.HeadshotStreak++;
                state.HeadPatternOnlyHeadHits++;
                state.LastHeadPatternHeadTime = now;

                var patternAdd = HeadPatternVlPerHead + Mathf.Clamp((state.HeadshotStreak - 3) * 0.10f, 0f, 0.55f);
                state.HeadPatternVl = Mathf.Max(VlMin, state.HeadPatternVl + ApplyPingLeniency(attacker, patternAdd));
            }
            else
            {
                state.HeadshotStreak = Mathf.Max(0, state.HeadshotStreak - 1);
                state.HeadPatternOnlyHeadHits = Mathf.Max(0, state.HeadPatternOnlyHeadHits - 1);
                state.HeadPatternVl = Mathf.Max(VlMin, state.HeadPatternVl - 0.45f);
            }

            if (state.HeadPatternOnlyHeadHits >= HeadPatternMinHeadsOnly && state.HeadPatternVl >= HeadPatternVlThreshold)
            {
                var patternVl = state.HeadPatternVl + state.HeadshotCheatDetect * 0.30f;
                var patternEvidence = "headsOnly=" + state.HeadPatternOnlyHeadHits
                    + " patternVl=" + state.HeadPatternVl.ToString("0.0")
                    + " bodyReset=" + HeadPatternResetOnBodyDelay.ToString("0") + "s"
                    + " dist=" + distance.ToString("0.0");
                return HandlePenalty(attacker, "HeadshotRate", patternVl, "HeadshotPattern(OnlyHeads)", true, patternEvidence, 0.97f);
            }

            if (state.HeadshotQualifiedHitsWindow < HeadshotMinQualifiedHits) return false;
            if (state.HeadshotHeadHitsWindow < HeadshotMinHeadHits) return false;

            var ratio = state.HeadshotHeadHitsWindow / Mathf.Max(1f, (float)state.HeadshotQualifiedHitsWindow);
            if (ratio < HeadshotRatioThreshold) return false;

            var addCd = HeadshotCdBase + Mathf.Clamp((ratio - HeadshotRatioThreshold) * HeadshotCdRatioScale, 0f, 9f);
            if (distance >= 70f) addCd += 0.8f;
            if (distance >= 110f) addCd += 1.1f;
            if (state.HeadshotStreak >= 4) addCd += Mathf.Min(2.4f, (state.HeadshotStreak - 3) * 0.45f);

            addCd = ApplyPingLeniency(attacker, addCd);
            state.HeadshotCheatDetect = Mathf.Max(VlMin, state.HeadshotCheatDetect + addCd);

            var confidence = 0.52f
                + Mathf.Clamp((ratio - HeadshotRatioThreshold) * 1.2f, 0f, 0.28f)
                + Mathf.Clamp((distance - HeadshotMinDistance) / 140f, 0f, 0.10f)
                + Mathf.Clamp((state.HeadshotStreak - 3) * 0.03f, 0f, 0.10f);
            confidence = Mathf.Clamp01(confidence);

            var weapon = GetHeldWeaponShortName(attacker);
            var detectMsg = "[AC DETECT] " + attacker.displayName + " (" + attacker.UserIDString + ") -> HeadshotRate"
                + " ratio=" + ratio.ToString("0.00")
                + " heads=" + state.HeadshotHeadHitsWindow + "/" + state.HeadshotQualifiedHitsWindow
                + " streak=" + state.HeadshotStreak
                + " dist=" + distance.ToString("0.0")
                + " weapon=" + weapon
                + " CheatDetect(HeadshotRate)=" + state.HeadshotCheatDetect.ToString("0.0");
            NotifyAdminsThrottled(state, detectMsg);

            var evidence = "ratio=" + ratio.ToString("0.00")
                + " heads=" + state.HeadshotHeadHitsWindow + "/" + state.HeadshotQualifiedHitsWindow
                + " streak=" + state.HeadshotStreak
                + " dist=" + distance.ToString("0.0")
                + " weapon=" + weapon
                + " isHeadNow=" + isHead;

            var hardEvidence = ratio >= 0.88f && state.HeadshotStreak >= 6 && distance >= 60f;
            var block = HandlePenalty(attacker, "HeadshotRate", state.HeadshotCheatDetect, "HeadshotRate", hardEvidence, evidence, confidence);

            if (state.HeadshotQualifiedHitsWindow > HeadshotMinQualifiedHits)
            {
                state.HeadshotQualifiedHitsWindow = Mathf.Max(HeadshotMinQualifiedHits - 4, state.HeadshotQualifiedHitsWindow - 2);
                state.HeadshotHeadHitsWindow = Mathf.Max(HeadshotMinHeadHits - 3, state.HeadshotHeadHitsWindow - 1);
            }

            return block;
        }

        private void ResetHeadshotTracking(MoveState state, bool fullReset)
        {
            if (state == null) return;

            state.HeadPatternOnlyHeadHits = 0;
            state.HeadPatternVl = 0f;
            state.LastHeadPatternHeadTime = 0f;

            if (!fullReset) return;

            state.HeadshotCheatDetect = 0f;
            state.HeadshotWindowStart = 0f;
            state.HeadshotQualifiedHitsWindow = 0;
            state.HeadshotHeadHitsWindow = 0;
            state.HeadshotStreak = 0;
        }

        private void CaptureShotSnapshot(BasePlayer attacker, MoveState state)
        {
            if (attacker == null || state == null) return;

            if (!IsSilentAimSupportedWeapon(attacker))
            {
                state.LastShotValid = false;
                return;
            }

            var look = attacker.eyes != null ? attacker.eyes.BodyForward() : attacker.transform.forward;
            if (look.sqrMagnitude < 0.0001f)
            {
                state.LastShotValid = false;
                return;
            }

            look.Normalize();
            var now = Time.realtimeSinceStartup;
            state.LastShotInterval = state.LastShotCaptureTime > 0f ? Mathf.Clamp(now - state.LastShotCaptureTime, 0f, 3f) : 0f;
            state.LastShotForward = look;
            state.LastShotOrigin = attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f;
            state.LastShotCaptureTime = now;
            state.LastShotIsBowLike = IsBowLikeWeapon(attacker);
            state.LastShotValid = true;
        }

        private void UpdateTargetTrack(BasePlayer player, float now, Vector3 position)
        {
            if (player == null) return;

            TargetTrack track;
            if (!_targetTracks.TryGetValue(player.userID, out track))
            {
                track = new TargetTrack();
                _targetTracks[player.userID] = track;
            }

            if (!track.Initialized)
            {
                track.Initialized = true;
                track.LastPosition = position;
                track.LastTime = now;
                track.Velocity = Vector3.zero;
                return;
            }

            var dt = Mathf.Clamp(now - track.LastTime, 0.01f, 0.5f);
            track.Velocity = (position - track.LastPosition) / dt;
            track.LastPosition = position;
            track.LastTime = now;
        }

        private Vector3 GetTrackedVelocity(BasePlayer player)
        {
            if (player == null) return Vector3.zero;

            TargetTrack track;
            if (!_targetTracks.TryGetValue(player.userID, out track) || !track.Initialized)
            {
                return Vector3.zero;
            }

            return track.Velocity;
        }

        private bool CheckSilentAim(BasePlayer attacker, BasePlayer victim, MoveState state)
        {
            if (attacker == null || victim == null || state == null || !state.LastShotValid) return false;
            if (!IsSilentAimSupportedWeapon(attacker)) return false;

            var now = Time.realtimeSinceStartup;
            var maxShotAge = state.LastShotIsBowLike ? SilentAimBowShotAge : SilentAimBulletShotAge;
            var shotAge = now - state.LastShotCaptureTime;
            if (shotAge < 0f || shotAge > maxShotAge) return false;

            if (state.SilentAimWindowStart <= 0f || (now - state.SilentAimWindowStart) > SilentAimWindowSeconds)
            {
                state.SilentAimWindowStart = now;
                state.SilentAimStrikesWindow = 0;
            }

            if (shotAge > SilentAimViewBlockGrace && IsCurrentViewBlocked(attacker, victim))
            {
                return false;
            }

            var targetPoint = victim.transform.position + Vector3.up * 1.35f;
            var victimVelocity = GetTrackedVelocity(victim);
            var projectileSpeed = Mathf.Max(25f, GetProjectileSpeed(attacker));
            var travelTime = Vector3.Distance(state.LastShotOrigin, targetPoint) / projectileSpeed;
            var rewindCap = state.LastShotIsBowLike ? SilentAimMoveRewindMaxBow : SilentAimMoveRewindMaxBullet;
            var rewindTime = Mathf.Clamp(Mathf.Min(shotAge, travelTime), 0f, rewindCap);
            if (rewindTime > 0.001f && victimVelocity.sqrMagnitude > 0.0001f)
            {
                // Compensate target movement after shot to avoid false flags
                // when player runs into a legit bullet path.
                targetPoint -= victimVelocity * rewindTime;
            }

            var toTarget = targetPoint - state.LastShotOrigin;
            var distance = toTarget.magnitude;
            if (distance < SilentAimMinDistance) return false;
            if (distance < 0.5f) return false;

            var dir = toTarget / distance;
            var dot = Vector3.Dot(state.LastShotForward, dir);
            var projected = Vector3.Dot(toTarget, state.LastShotForward);
            var closest = state.LastShotOrigin + state.LastShotForward * Mathf.Max(0f, projected);
            var offRadius = Vector3.Distance(targetPoint, closest);
            var allowRadius = GetSilentAimRadius(distance, state.LastShotIsBowLike);
            var movedSinceShot = victimVelocity.magnitude * rewindTime;
            var extraMoveGrace = Mathf.Clamp(movedSinceShot * SilentAimMoveGraceFactor, 0f, state.LastShotIsBowLike ? 0.95f : 0.55f);
            allowRadius += extraMoveGrace;

            var dotMin = state.LastShotIsBowLike ? SilentAimDotMinBow : SilentAimDotMinBullet;
            var hardBack = projected < 0f || dot < -0.14f;
            var badAngle = dot < dotMin;
            var badRadius = offRadius > allowRadius;
            if (!hardBack
                && movedSinceShot > 0.55f
                && dot >= (dotMin - 0.08f)
                && offRadius <= (allowRadius + 0.30f))
            {
                return false;
            }

            if (!hardBack && !(badAngle && badRadius)) return false;

            state.LastSilentAimTime = now;
            state.SilentAimStrikesWindow++;

            var strikeNeed = hardBack ? 1 : SilentAimMinStrikes;
            if (state.SilentAimStrikesWindow < strikeNeed)
            {
                return false;
            }

            var addCd = hardBack ? 5.6f : (state.LastShotIsBowLike ? 2.4f : 3.0f);
            addCd += Mathf.Clamp((dotMin - dot) * 4.2f, 0f, 4.5f);
            addCd += Mathf.Clamp(offRadius - allowRadius, 0f, 3.0f);
            if (state.SilentAimStrikesWindow > SilentAimMinStrikes)
            {
                addCd += (state.SilentAimStrikesWindow - SilentAimMinStrikes) * 0.85f;
            }

            addCd = ApplyPingLeniency(attacker, addCd);
            state.SilentAimCheatDetect = Mathf.Max(VlMin, state.SilentAimCheatDetect + addCd);

            var reason = hardBack ? "SilentAim(360)" : "SilentAim(Radius)";
            var detectMsg = "[AC DETECT] " + attacker.displayName + " (" + attacker.UserIDString + ") -> " + reason
                + " dot=" + dot.ToString("0.00")
                + " off=" + offRadius.ToString("0.00")
                + " rw=" + rewindTime.ToString("0.00")
                + " strikes=" + state.SilentAimStrikesWindow + "/" + SilentAimMinStrikes
                + " CheatDetect(SilentAim)=" + state.SilentAimCheatDetect.ToString("0.0");
            NotifyAdminsThrottled(state, detectMsg);

            var saEvidence = "dot=" + dot.ToString("0.00")
                + " off=" + offRadius.ToString("0.00")
                + " allow=" + allowRadius.ToString("0.00")
                + " dist=" + distance.ToString("0.0")
                + " rw=" + rewindTime.ToString("0.00")
                + " shotDt=" + state.LastShotInterval.ToString("0.000") + "s";
            return HandlePenalty(attacker, "SilentAim", state.SilentAimCheatDetect, reason, hardBack, saEvidence);
        }

        private float GetSilentAimRadius(float distance, bool bowLike)
        {
            if (bowLike)
            {
                return Mathf.Clamp(0.65f + distance * 0.018f, 0.65f, 2.5f);
            }

            return Mathf.Clamp(0.33f + distance * 0.010f, 0.33f, 1.25f);
        }

        private bool IsCurrentViewBlocked(BasePlayer attacker, BasePlayer victim)
        {
            if (attacker == null || victim == null) return false;

            var start = attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f;
            var end = victim.transform.position + Vector3.up * 1.35f;

            RaycastHit hit;
            var mask = LayerMask.GetMask("Terrain", "World", "Construction", "Default", "Deployed");
            if (!Physics.Linecast(start, end, out hit, mask)) return false;

            var hitEntity = hit.collider != null ? hit.collider.GetComponentInParent<BaseEntity>() : null;
            if (hitEntity == null) return true;
            if (hitEntity == attacker) return false;
            if (hitEntity == victim) return false;

            return true;
        }

        private float GetExpectedShotDelay(BasePlayer attacker)
        {
            if (attacker == null) return 0.11f;

            var projectile = attacker.GetHeldEntity() as BaseProjectile;
            if (projectile != null)
            {
                var delay = projectile.repeatDelay;
                if (delay > 0.01f)
                {
                    return Mathf.Clamp(delay, 0.06f, 0.35f);
                }
            }

            return 0.11f;
        }

        private float GetProjectileSpeed(BasePlayer attacker)
        {
            if (attacker == null) return 300f;
            if (IsBowLikeWeapon(attacker)) return 65f;

            var item = attacker.GetActiveItem();
            if (item != null && item.info != null)
            {
                var shortName = item.info.shortname;
                if (!string.IsNullOrEmpty(shortName))
                {
                    shortName = shortName.ToLowerInvariant();
                    if (shortName.Contains("shotgun")) return 230f;
                    if (shortName.Contains("pistol")) return 285f;
                }
            }

            return 300f;
        }

        private bool IsManipulatorImpossibleTrajectory(BasePlayer attacker, BasePlayer victim, MoveState state, HitInfo info)
        {
            if (attacker == null || victim == null || state == null) return false;

            var origin = state.LastShotValid ? state.LastShotOrigin : (attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f);
            var forward = state.LastShotValid ? state.LastShotForward : (attacker.eyes != null ? attacker.eyes.BodyForward() : attacker.transform.forward);
            if (forward.sqrMagnitude < 0.0001f) return false;
            forward.Normalize();

            var targetPoint = victim.transform.position + Vector3.up * 1.35f;
            var toTarget = targetPoint - origin;
            var distance = toTarget.magnitude;
            if (distance < 1.2f) return false;

            var dir = toTarget / distance;
            var dot = Vector3.Dot(forward, dir);
            var projected = Vector3.Dot(toTarget, forward);
            var closest = origin + forward * Mathf.Max(0f, projected);
            var offRadius = Vector3.Distance(targetPoint, closest);

            var hardBack = projected < 0f || dot < -0.10f;
            var badAngle = dot < 0.22f;
            var badRadius = offRadius > Mathf.Clamp(0.55f + distance * 0.012f, 0.55f, 2.2f);

            if (info != null && info.HitPositionWorld != Vector3.zero)
            {
                var hitDistance = Vector3.Distance(origin, info.HitPositionWorld);
                if (hitDistance > 12f)
                {
                    var toHit = (info.HitPositionWorld - origin).normalized;
                    var hitDot = Vector3.Dot(forward, toHit);
                    if (hitDot < 0.20f) return true;
                }
            }

            return hardBack || (badAngle && badRadius);
        }

        private bool IsShotThroughObstacle(BasePlayer attacker, HitInfo info, BasePlayer victim)
        {
            if (attacker == null || info == null) return false;

            var targetEntity = (info.HitEntity as BaseEntity) ?? victim;
            if (targetEntity == null) return false;
            if (!(targetEntity is BasePlayer)) return false;

            var start = attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f;
            var end = info.HitPositionWorld;
            if (end == Vector3.zero) end = targetEntity.transform.position;

            var distance = Vector3.Distance(start, end);
            if (distance < ManipulatorMinTraceDistance) return false;

            RaycastHit hit;
            var mask = LayerMask.GetMask("Terrain", "World", "Construction", "Default", "Deployed");
            if (!Physics.Linecast(start, end, out hit, mask)) return false;

            var hitEntity = hit.collider != null ? hit.collider.GetComponentInParent<BaseEntity>() : null;
            if (hitEntity == null) return true;
            if (hitEntity == attacker) return false;
            if (hitEntity == targetEntity) return false;

            var remainToTarget = Mathf.Max(0f, distance - hit.distance);
            if (remainToTarget <= ManipulatorEndObstacleGrace) return false;

            if (victim != null && IsAnyVictimPointVisible(attacker, victim)) return false;

            return true;
        }

        private bool IsAnyVictimPointVisible(BasePlayer attacker, BasePlayer victim)
        {
            if (attacker == null || victim == null) return false;

            var start = attacker.eyes != null ? attacker.eyes.position : attacker.transform.position + Vector3.up * 1.5f;
            var points = new[]
            {
                victim.transform.position + Vector3.up * 1.62f,
                victim.transform.position + Vector3.up * 1.35f,
                victim.transform.position + Vector3.up * 1.05f
            };

            for (var i = 0; i < points.Length; i++)
            {
                if (!IsLineBlockedByWorld(start, points[i], victim))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLineBlockedByWorld(Vector3 start, Vector3 end, BasePlayer victim)
        {
            RaycastHit hit;
            var mask = LayerMask.GetMask("Terrain", "World", "Construction", "Default", "Deployed");
            if (!Physics.Linecast(start, end, out hit, mask)) return false;

            var hitEntity = hit.collider != null ? hit.collider.GetComponentInParent<BaseEntity>() : null;
            if (hitEntity == null)
            {
                var remain = Vector3.Distance(hit.point, end);
                return remain > ManipulatorVisiblePointGrace;
            }

            if (hitEntity == victim) return false;

            var remainToTarget = Vector3.Distance(hit.point, end);
            if (remainToTarget <= ManipulatorVisiblePointGrace) return false;

            return true;
        }

        private bool IsBowLikeWeapon(BasePlayer attacker)
        {
            if (attacker == null) return false;

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            return shortName.Contains("bow") || shortName.Contains("crossbow");
        }

        private bool IsHeadshotSupportedWeapon(BasePlayer attacker)
        {
            if (attacker == null) return false;

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            if (shortName.Contains("shotgun")
                || shortName.Contains("bow")
                || shortName.Contains("crossbow")
                || shortName.Contains("rocket")
                || shortName.Contains("launcher")
                || shortName.Contains("grenade")
                || shortName.Contains("explosive")
                || shortName.Contains("satchel")
                || shortName.Contains("beancan")
                || shortName.Contains("nailgun")
                || shortName.Contains("eoka"))
            {
                return false;
            }

            return shortName.Contains("rifle")
                || shortName.Contains("smg")
                || shortName.Contains("lmg")
                || shortName.Contains("pistol")
                || shortName.Contains("revolver");
        }

        private bool IsHeadHit(HitInfo info)
        {
            if (info == null) return false;

            try
            {
                var boneName = StringPool.Get(info.HitBone);
                if (!string.IsNullOrEmpty(boneName))
                {
                    boneName = boneName.ToLowerInvariant();
                    if (boneName.Contains("head") || boneName.Contains("skull") || boneName.Contains("face"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            try
            {
                var type = info.GetType();
                var prop = type.GetProperty("boneArea");
                if (prop != null)
                {
                    var value = prop.GetValue(info, null);
                    if (value != null)
                    {
                        var area = value.ToString().ToLowerInvariant();
                        if (area.Contains("head") || area.Contains("skull") || area.Contains("face"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private string GetHeldWeaponShortName(BasePlayer player)
        {
            if (player == null) return "none";
            var item = player.GetActiveItem();
            if (item == null || item.info == null || string.IsNullOrEmpty(item.info.shortname)) return "none";
            return item.info.shortname;
        }

        private bool IsSemiAutoClickWeapon(BasePlayer attacker)
        {
            if (attacker == null) return false;

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            return shortName.Contains("pistol")
                || shortName.Contains("revolver")
                || shortName.Contains("eoka")
                || shortName.Contains("nailgun");
        }

        private bool IsSilentAimSupportedWeapon(BasePlayer attacker)
        {
            if (attacker == null) return false;

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            if (shortName.Contains("rocket")
                || shortName.Contains("launcher")
                || shortName.Contains("grenade")
                || shortName.Contains("shotgun")
                || shortName.Contains("nailgun")
                || shortName.Contains("flame"))
            {
                return false;
            }

            return shortName.Contains("rifle")
                || shortName.Contains("smg")
                || shortName.Contains("lmg")
                || shortName.Contains("pistol")
                || shortName.Contains("revolver")
                || shortName.Contains("bow")
                || shortName.Contains("crossbow");
        }

        private bool IsManipulatorExcludedWeapon(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null) return false;

            // Splash/explosive damage is noisy for through-wall and fire-rate checks.
            if (info != null && info.damageTypes != null && info.damageTypes.Has(Rust.DamageType.Explosion))
            {
                return true;
            }

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            return shortName.Contains("shotgun")
                || shortName.Contains("rocket")
                || shortName.Contains("launcher")
                || shortName.Contains("grenade")
                || shortName.Contains("explosive.timed")
                || shortName.Contains("c4")
                || shortName.Contains("satchel")
                || shortName.Contains("beancan")
                || shortName.Contains("40mm");
        }

        private void NotifyAdminsThrottled(MoveState state, string message)
        {
            var now = Time.realtimeSinceStartup;
            if (now - state.LastAdminNotifyTime < 0.65f) return;

            state.LastAdminNotifyTime = now;
            NotifyAdmins(message);
        }

        private bool IsMountedDriver(BasePlayer player)
        {
            if (player == null || !player.isMounted) return false;

            var mount = player.GetMounted();
            if (mount == null) return false;

            var mountName = (mount.ShortPrefabName ?? string.Empty).ToLowerInvariant();
            var parent = mount.GetParentEntity();
            var parentName = parent == null ? string.Empty : (parent.ShortPrefabName ?? string.Empty).ToLowerInvariant();

            var isHeliSeat = mountName.Contains("minicopter")
                || mountName.Contains("scraptransporthelicopter")
                || parentName.Contains("minicopter")
                || parentName.Contains("scraptransporthelicopter");
            if (!isHeliSeat) return false;

            var method = mount.GetType().GetMethod("IsDriver", new[] { typeof(BasePlayer) });
            if (method != null)
            {
                try
                {
                    var result = method.Invoke(mount, new object[] { player });
                    if (result is bool && (bool)result) return true;
                }
                catch
                {
                }
            }

            if (mountName.Contains("passenger") || mountName.Contains("rear") || mountName.Contains("back"))
            {
                return false;
            }

            return true;
        }

        private bool IsRangedAttack(HitInfo info, BasePlayer attacker)
        {
            if (info == null || info.damageTypes == null) return false;

            if (info.damageTypes.Get(Rust.DamageType.Bullet) > 0f
                || info.damageTypes.Get(Rust.DamageType.Arrow) > 0f
                || info.damageTypes.Get(Rust.DamageType.Explosion) > 0f)
            {
                return true;
            }

            if (attacker == null) return false;

            var item = attacker.GetActiveItem();
            if (item == null || item.info == null) return false;

            var shortName = item.info.shortname;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            return shortName.Contains("rifle")
                || shortName.Contains("pistol")
                || shortName.Contains("shotgun")
                || shortName.Contains("smg")
                || shortName.Contains("lmg")
                || shortName.Contains("bow")
                || shortName.Contains("crossbow")
                || shortName.Contains("launcher");
        }

        private bool IsOnLadder(BasePlayer player)
        {
            var center = player.transform.position + Vector3.up * 0.8f;
            var mask = LayerMask.GetMask("Construction", "Default", "Deployed", "World");
            var overlaps = Physics.OverlapSphere(center, 0.9f, mask);
            for (var i = 0; i < overlaps.Length; i++)
            {
                var col = overlaps[i];
                if (col == null) continue;

                var colName = col.name == null ? string.Empty : col.name.ToLowerInvariant();
                if (colName.Contains("ladder")) return true;

                var entity = col.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;

                var shortName = entity.ShortPrefabName;
                if (!string.IsNullOrEmpty(shortName) && shortName.ToLowerInvariant().Contains("ladder"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNearSmallClimbable(BasePlayer player)
        {
            if (player == null) return false;

            var center = player.transform.position + Vector3.up * 0.35f;
            var mask = LayerMask.GetMask("Deployed", "Construction", "Default", "World");
            var overlaps = Physics.OverlapSphere(center, 1.05f, mask);
            for (var i = 0; i < overlaps.Length; i++)
            {
                var col = overlaps[i];
                if (col == null) continue;

                var name = col.name == null ? string.Empty : col.name.ToLowerInvariant();
                if (name.Contains("furnace") || name.Contains("bbq") || name.Contains("campfire") || name.Contains("refinery"))
                {
                    return true;
                }

                var entity = col.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;

                var shortName = entity.ShortPrefabName == null ? string.Empty : entity.ShortPrefabName.ToLowerInvariant();
                if (shortName.Contains("furnace") || shortName.Contains("bbq") || shortName.Contains("campfire") || shortName.Contains("refinery"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNearHalfWallOrLowLedge(BasePlayer player)
        {
            if (player == null) return false;

            var center = player.transform.position + Vector3.up * 0.8f;
            var mask = LayerMask.GetMask("Construction", "World", "Default");
            var overlaps = Physics.OverlapSphere(center, 1.2f, mask);
            for (var i = 0; i < overlaps.Length; i++)
            {
                var col = overlaps[i];
                if (col == null) continue;

                var colName = col.name == null ? string.Empty : col.name.ToLowerInvariant();
                if (colName.Contains("half") || colName.Contains("low") || colName.Contains("window"))
                {
                    return true;
                }

                var entity = col.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;

                var shortName = entity.ShortPrefabName == null ? string.Empty : entity.ShortPrefabName.ToLowerInvariant();
                if (shortName.Contains("half") || shortName.Contains("wall.half") || shortName.Contains("window"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsNearLadderOrRamp(BasePlayer player)
        {
            if (player == null) return false;

            var center = player.transform.position + Vector3.up * 0.75f;
            var mask = LayerMask.GetMask("Construction", "World", "Default", "Deployed");
            var overlaps = Physics.OverlapSphere(center, 1.35f, mask);
            for (var i = 0; i < overlaps.Length; i++)
            {
                var col = overlaps[i];
                if (col == null) continue;

                var colName = col.name == null ? string.Empty : col.name.ToLowerInvariant();
                if (colName.Contains("ladder")
                    || colName.Contains("ramp")
                    || colName.Contains("stair")
                    || colName.Contains("steps")
                    || colName.Contains("roof"))
                {
                    return true;
                }

                var entity = col.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;

                var shortName = entity.ShortPrefabName == null ? string.Empty : entity.ShortPrefabName.ToLowerInvariant();
                if (shortName.Contains("ladder")
                    || shortName.Contains("ramp")
                    || shortName.Contains("stair")
                    || shortName.Contains("steps")
                    || shortName.Contains("roof"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetGroundDistance(BasePlayer player, out float distance)
        {
            distance = 0f;
            RaycastHit hit;
            var start = player.transform.position + Vector3.up * 0.15f;
            var mask = LayerMask.GetMask("Terrain", "World", "Construction", "Default");
            if (Physics.Raycast(start, Vector3.down, out hit, 180f, mask))
            {
                distance = hit.distance;
                return true;
            }

            return false;
        }

        private bool IsInTransport(BasePlayer player)
        {
            if (player.isMounted) return true;

            var parent = player.GetParentEntity();
            if (parent == null) return false;

            var shortName = parent.ShortPrefabName;
            if (string.IsNullOrEmpty(shortName)) return false;

            shortName = shortName.ToLowerInvariant();
            return shortName.Contains("car")
                || shortName.Contains("boat")
                || shortName.Contains("mini")
                || shortName.Contains("scrap")
                || shortName.Contains("horse")
                || shortName.Contains("sub")
                || shortName.Contains("train")
                || shortName.Contains("vehicle");
        }

        private bool IsNearWallSurface(BasePlayer player)
        {
            if (player == null) return false;

            var origin = player.transform.position + Vector3.up * 0.95f;
            var mask = LayerMask.GetMask("Construction", "World", "Default", "Deployed");
            var fwd = player.transform.forward;
            var right = player.transform.right;
            var dirs = new Vector3[] { fwd, -fwd, right, -right };

            for (var i = 0; i < dirs.Length; i++)
            {
                var dir = dirs[i];
                if (dir.sqrMagnitude < 0.0001f) continue;
                dir = dir.normalized;

                RaycastHit hit;
                if (!Physics.Raycast(origin, dir, out hit, 0.95f, mask)) continue;

                var col = hit.collider;
                if (col == null) continue;

                var colName = col.name == null ? string.Empty : col.name.ToLowerInvariant();
                if (colName.Contains("ladder") || colName.Contains("furnace") || colName.Contains("bbq") || colName.Contains("campfire") || colName.Contains("refinery"))
                {
                    continue;
                }

                var entity = col.GetComponentInParent<BaseEntity>();
                if (entity != null)
                {
                    var shortName = entity.ShortPrefabName == null ? string.Empty : entity.ShortPrefabName.ToLowerInvariant();
                    if (shortName.Contains("ladder") || shortName.Contains("furnace") || shortName.Contains("bbq") || shortName.Contains("campfire") || shortName.Contains("refinery"))
                    {
                        continue;
                    }
                }

                if (Mathf.Abs(hit.normal.y) <= 0.45f)
                {
                    return true;
                }
            }

            return false;
        }

        private void MarkEspCombatReveal(BasePlayer player)
        {
            if (!IsPlayerConnected(player) || _config == null) return;
            if (_config.EspCombatRevealSeconds <= 0f) return;

            _espCombatRevealUntil[player.userID] = Time.realtimeSinceStartup + _config.EspCombatRevealSeconds;
        }

        private bool IsEspCombatRevealActive(BasePlayer player)
        {
            if (!IsPlayerConnected(player)) return false;

            float until;
            if (!_espCombatRevealUntil.TryGetValue(player.userID, out until)) return false;
            if (until <= Time.realtimeSinceStartup)
            {
                _espCombatRevealUntil.Remove(player.userID);
                return false;
            }

            return true;
        }

        private bool ShouldHideEntityForViewer(BasePlayer viewer, Vector3 entityPos, float hideDistance)
        {
            if (!IsPlayerConnected(viewer)) return false;
            if (_config == null) return false;
            if (hideDistance <= 0f) return false;

            var distance = Vector3.Distance(viewer.transform.position, entityPos);
            if (distance <= _config.EspRevealDistance) return false;
            if (IsEspCombatRevealActive(viewer)) return false;

            if (distance >= hideDistance)
            {
                return true;
            }

            if (!_config.EspRequireLineOfSight)
            {
                return true;
            }

            return !HasEntityLineOfSight(viewer, entityPos);
        }

        private bool HasEntityLineOfSight(BasePlayer viewer, Vector3 entityPos)
        {
            if (!IsPlayerConnected(viewer)) return true;

            var start = viewer.eyes != null ? viewer.eyes.position : viewer.transform.position + Vector3.up * 1.5f;
            RaycastHit hit;
            var mask = LayerMask.GetMask("Terrain", "World", "Construction", "Default", "Deployed");
            if (!Physics.Linecast(start, entityPos, out hit, mask)) return true;

            var remain = Vector3.Distance(hit.point, entityPos);
            return remain <= 0.50f;
        }

        private bool ArePlayersInSameTeam(ulong firstUserId, ulong secondUserId)
        {
            if (firstUserId == 0 || secondUserId == 0) return false;
            if (firstUserId == secondUserId) return true;
            if (RelationshipManager.Instance == null) return false;

            var team = RelationshipManager.Instance.FindPlayersTeam(firstUserId);
            if (team == null || team.members == null || team.members.Count == 0) return false;
            return team.members.Contains(secondUserId);
        }

        private bool IsEspStash(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return false;
            return shortName.Contains("stash");
        }

        private bool IsEspTrap(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return false;
            return shortName.Contains("autoturret")
                || shortName.Contains("guntrap")
                || shortName.Contains("flameturret")
                || shortName.Contains("landmine")
                || shortName.Contains("beartrap")
                || shortName.Contains("sam_site");
        }

        private bool IsEspLootContainer(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return false;
            return shortName.Contains("loot")
                || shortName.Contains("crate")
                || shortName.Contains("barrel")
                || shortName.Contains("supply_drop")
                || shortName.Contains("box.wooden")
                || shortName.Contains("item_drop_backpack");
        }

        private bool IsDetectionsPaused(MoveState state, float now)
        {
            return state != null && state.DetectionPauseUntil > now;
        }

        private bool IsPlayerConnected(BasePlayer player)
        {
            if (player == null) return false;
            return player.net != null && player.net.connection != null;
        }

        private DetectRule GetRule(string detectKey)
        {
            if (_config == null || _config.RuleSet == null || string.IsNullOrEmpty(detectKey)) return null;

            DetectRule rule;
            if (_config.RuleSet.TryGetValue(detectKey, out rule)) return rule;
            return null;
        }

        private bool HandlePenalty(BasePlayer player, string detectKey, float vl, string logReason, bool hardEvidence = false, string evidence = "", float confidence = 0.60f)
        {
            if (!IsPlayerConnected(player)) return false;

            MoveState state;
            _states.TryGetValue(player.userID, out state);

            var now = Time.realtimeSinceStartup;
            if (!string.Equals(detectKey, "NoFallDamage", StringComparison.Ordinal))
            {
                if (state != null && IsDetectionsPaused(state, now))
                {
                    return false;
                }
            }

            var rule = GetRule(detectKey);
            if (rule == null || !rule.Enabled) return false;

            if (state != null)
            {
                state.LastViolationTime = now;
                RecordEvidence(state, detectKey, player, logReason, vl, evidence, now, confidence);
            }

            if (state != null && state.PenaltyCooldownUntil > now)
            {
                NotifyAdminsThrottled(state, FormatAdminDetectLine("DETECT", player, detectKey, logReason, vl, rule.Threshold, "COOLDOWN " + (state.PenaltyCooldownUntil - now).ToString("0.0") + "s", state, evidence));
                return ShouldBlockDetect(detectKey);
            }

            if (IsNetworkLogOnly(state))
            {
                NotifyAdmins(FormatAdminDetectLine("DETECT", player, detectKey, logReason, vl, rule.Threshold, "LOG_ONLY_NET", state, evidence));
                return ShouldBlockDetect(detectKey);
            }

            if (state != null && state.LastNetworkUnstableTime > 0f && (now - state.LastNetworkUnstableTime) < RecentNetworkInstabilityHoldSeconds)
            {
                NotifyAdminsThrottled(state, FormatAdminDetectLine("DETECT", player, detectKey, logReason, vl, rule.Threshold, "LOG_ONLY_HOLD", state, evidence));
                return ShouldBlockDetect(detectKey);
            }

            var reasonCode = string.IsNullOrEmpty(rule.Reason) ? detectKey : rule.Reason;
            NotifyAdminsThrottled(state, FormatAdminDetectLine("DETECT", player, detectKey, logReason, vl, rule.Threshold, "VL", state, evidence));

            if (vl < rule.Threshold)
            {
                return ShouldBlockDetect(detectKey);
            }

            var evidenceBundle = BuildEvidenceBundle(state);
            if (RegisterFlyNoFallHitAndMaybeBan(player, detectKey, vl, evidenceBundle))
            {
                return true;
            }

            if (rule.Ban)
            {
                BanPlayer(player, reasonCode, vl, logReason, evidenceBundle);
                return true;
            }

            KickPlayer(player, reasonCode, vl, logReason);
            return true;
        }

        private bool IsNetworkLogOnly(MoveState state)
        {
            if (state == null) return false;
            if (state.CachedPingMs >= HighPingLogOnlyMs) return true;
            if (state.LastPingJitterMs >= HighJitterLogOnlyMs) return true;
            if (state.LastPacketLossPercent >= HighPacketLossLogOnlyPercent) return true;
            if (state.LastServerFrameMs >= HighServerFrameLogOnlyMs) return true;
            return false;
        }

        private bool ShouldBlockDetect(string detectKey)
        {
            return string.Equals(detectKey, "SilentAim", StringComparison.Ordinal)
                || string.Equals(detectKey, "Manipulator", StringComparison.Ordinal)
                || string.Equals(detectKey, "FlyFire", StringComparison.Ordinal)
                || string.Equals(detectKey, "PilotFire", StringComparison.Ordinal);
        }

        private string FormatAdminDetectLine(string type, BasePlayer player, string detectKey, string reason, float vl, float threshold, string action, MoveState state, string evidence)
        {
            var ping = state == null ? 0f : state.CachedPingMs;
            var jitter = state == null ? 0f : state.LastPingJitterMs;
            var loss = state == null ? 0f : state.LastPacketLossPercent;
            var weapon = GetHeldWeaponShortName(player);
            var line = "[AC " + type + "] " + player.displayName + " (" + player.UserIDString + ")"
                + " | detect=" + detectKey
                + " | reason=" + reason
                + " | action=" + action
                + " | vl=" + vl.ToString("0.0") + "/" + threshold.ToString("0.0")
                + " | weapon=" + weapon
                + " | ping=" + ping.ToString("0") + "ms"
                + " | jitter=" + jitter.ToString("0") + "ms"
                + " | loss=" + loss.ToString("0.0") + "%";
            if (!string.IsNullOrEmpty(evidence)) line += " | " + evidence;
            return line;
        }

        private void RecordEvidence(MoveState state, string detectKey, BasePlayer player, string logReason, float vl, string evidence, float now, float confidence)
        {
            if (state == null) return;

            if (state.DetectKindTimes == null) state.DetectKindTimes = new Dictionary<string, float>();
            if (state.RecentEvidence == null) state.RecentEvidence = new Queue<string>();

            state.DetectKindTimes[detectKey] = now;

            var weapon = GetHeldWeaponShortName(player);
            var line = now.ToString("0.0")
                + "|" + detectKey
                + "|" + logReason
                + "|vl=" + vl.ToString("0.0")
                + "|conf=" + Mathf.Clamp01(confidence).ToString("0.00")
                + "|ping=" + state.CachedPingMs.ToString("0")
                + "|jitter=" + state.LastPingJitterMs.ToString("0")
                + "|loss=" + state.LastPacketLossPercent.ToString("0.0")
                + "|frame=" + state.LastServerFrameMs.ToString("0")
                + "|w=" + weapon;
            if (!string.IsNullOrEmpty(evidence)) line += "|" + evidence;

            state.RecentEvidence.Enqueue(line);
            while (state.RecentEvidence.Count > 5)
            {
                state.RecentEvidence.Dequeue();
            }
        }

        private string BuildEvidenceBundle(MoveState state)
        {
            if (state == null || state.RecentEvidence == null || state.RecentEvidence.Count == 0)
            {
                return "evidence=none";
            }

            return string.Join(" || ", state.RecentEvidence.ToArray());
        }

        private bool RegisterFlyNoFallHitAndMaybeBan(BasePlayer player, string detectKey, float vl, string evidenceBundle)
        {
            if (!IsPlayerConnected(player)) return false;
            if (!string.Equals(detectKey, "Fly", StringComparison.Ordinal)
                && !string.Equals(detectKey, "NoFallDamage", StringComparison.Ordinal))
            {
                return false;
            }

            if (_config == null) return false;

            List<float> hits;
            if (!_repeatFlyNoFallHits.TryGetValue(player.userID, out hits))
            {
                hits = new List<float>();
                _repeatFlyNoFallHits[player.userID] = hits;
            }

            var now = Time.realtimeSinceStartup;
            var windowSeconds = Mathf.Max(3600f, _config.RepeatFlyNoFallWindowHours * 3600f);
            var minTime = now - windowSeconds;
            hits.RemoveAll(t => t < minTime);
            hits.Add(now);

            if (hits.Count < Mathf.Max(2, _config.RepeatFlyNoFallLimit))
            {
                return false;
            }

            hits.Clear();
            var reason = string.IsNullOrEmpty(_config.RepeatFlyNoFallReason) ? "Нарушение правил" : _config.RepeatFlyNoFallReason;
            var duration = string.IsNullOrEmpty(_config.RepeatFlyNoFallDuration) ? "14d" : _config.RepeatFlyNoFallDuration;
            BanPlayerWithDuration(player, reason, duration, vl, "RepeatFlyNoFall", evidenceBundle);
            return true;
        }

        private void KickPlayer(BasePlayer player, string reasonCode, float vl, string logReason)
        {
            if (!IsPlayerConnected(player)) return;

            var msg = "[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> " + logReason + " reason=" + reasonCode + " action=KICK VL=" + vl.ToString("0.0");
            NotifyAdmins(msg);

            var template = (_config == null || string.IsNullOrEmpty(_config.KickCommandTemplate))
                ? "kick {steamid} {reason}"
                : _config.KickCommandTemplate;
            var command = template
                .Replace("{steamid}", player.UserIDString)
                .Replace("{reason}", reasonCode)
                .Replace("{time}", "0");
            rust.RunServerCommand(command);

            player.Kick("AC: Если вы не согласны пишете тикет!");
        }

        private void BanPlayer(BasePlayer player, string reasonCode, float vl, string logReason, string evidenceBundle)
        {
            BanPlayerWithDuration(player, reasonCode, BanDurationText, vl, logReason, evidenceBundle);
        }

        private void BanPlayerWithDuration(BasePlayer player, string reasonCode, string duration, float vl, string logReason, string evidenceBundle)
        {
            if (!IsPlayerConnected(player)) return;

            var msg = "[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> " + logReason + " reason=" + reasonCode + " action=BAN VL=" + vl.ToString("0.0");
            NotifyAdmins(msg);
            if (!string.IsNullOrEmpty(evidenceBundle))
            {
                NotifyAdmins("[AC EVIDENCE] " + player.displayName + " (" + player.UserIDString + ") -> " + evidenceBundle);
            }

            var template = (_config == null || string.IsNullOrEmpty(_config.BanCommandTemplate))
                ? "ban {steamid} {reason} 0"
                : _config.BanCommandTemplate;

            var command = template
                .Replace("{steamid}", player.UserIDString)
                .Replace("{reason}", reasonCode)
                .Replace("{time}", string.IsNullOrEmpty(duration) ? "0" : duration);

            rust.RunServerCommand(command);
            var banTime = string.IsNullOrEmpty(duration) ? "0" : duration;
            player.Kick(BanAppealMessage.Replace("{time}", banTime));
        }

        private bool IsApiTrue(object value)
        {
            if (value == null) return false;
            if (value is bool) return (bool)value;

            bool parsedBool;
            if (bool.TryParse(value.ToString(), out parsedBool)) return parsedBool;

            float parsedNumber;
            if (float.TryParse(value.ToString(), out parsedNumber)) return parsedNumber > 0f;

            return false;
        }

        private float ApplyPingLeniency(BasePlayer player, float addValue)
        {
            if (addValue <= 0f) return 0f;
            var ping = GetPlayerPingMs(player);
            if (ping >= PingExtremeMs) return addValue * 0.55f;
            if (ping >= PingVeryHighMs) return addValue * 0.70f;
            if (ping >= PingHighMs) return addValue * 0.85f;
            return addValue;
        }

        private float GetPingThresholdFactor(float pingMs)
        {
            if (pingMs >= PingExtremeMs) return 1.32f;
            if (pingMs >= PingVeryHighMs) return 1.22f;
            if (pingMs >= PingHighMs) return 1.10f;
            return 1f;
        }

        private float GetPlayerPingMs(BasePlayer player)
        {
            try
            {
                if (player == null || player.net == null || player.net.connection == null) return 0f;
                var connection = player.net.connection;
                var type = connection.GetType();

                var method = type.GetMethod("GetEstimatedPing");
                if (method != null)
                {
                    var value = method.Invoke(connection, null);
                    return ToFloat(value);
                }

                method = type.GetMethod("GetAveragePing");
                if (method != null)
                {
                    var value = method.Invoke(connection, null);
                    return ToFloat(value);
                }

                var field = type.GetField("averagePing");
                if (field != null)
                {
                    var value = field.GetValue(connection);
                    return ToFloat(value);
                }
            }
            catch
            {
            }

            return 0f;
        }

        private float GetPlayerPacketLossPercent(BasePlayer player)
        {
            try
            {
                if (player == null || player.net == null || player.net.connection == null) return 0f;
                var connection = player.net.connection;
                var type = connection.GetType();

                var method = type.GetMethod("GetAveragePacketLoss");
                if (method != null)
                {
                    var value = method.Invoke(connection, null);
                    return ToPercent(value);
                }

                method = type.GetMethod("GetPacketLoss");
                if (method != null)
                {
                    var value = method.Invoke(connection, null);
                    return ToPercent(value);
                }

                var field = type.GetField("packetLoss");
                if (field != null)
                {
                    var value = field.GetValue(connection);
                    return ToPercent(value);
                }

                var property = type.GetProperty("packetLoss");
                if (property != null)
                {
                    var value = property.GetValue(connection, null);
                    return ToPercent(value);
                }
            }
            catch
            {
            }

            return 0f;
        }

        private float ToPercent(object value)
        {
            var raw = ToFloat(value);
            if (raw <= 0f) return 0f;
            if (raw <= 1f) return raw * 100f;
            return raw;
        }

        private float ToFloat(object value)
        {
            if (value == null) return 0f;
            if (value is float) return (float)value;
            if (value is double) return (float)(double)value;
            if (value is int) return (int)value;
            if (value is long) return (long)value;
            float parsed;
            return float.TryParse(value.ToString(), out parsed) ? parsed : 0f;
        }

        private bool HasAnyViolation(MoveState state)
        {
            if (state == null) return false;

            return state.FlyVl > 0.01f
                || state.FlyCheatDetect > 0.01f
                || state.SpiderVl > 0.01f
                || state.SpiderCheatDetect > 0.01f
                || state.ManipulatorCheatDetect > 0.01f
                || state.SilentAimCheatDetect > 0.01f
                || state.FlyFireCheatDetect > 0.01f
                || state.NoFallDamageCheatDetect > 0.01f
                || state.PilotFireCheatDetect > 0.01f
                || state.HeadshotCheatDetect > 0.01f
                || state.HeadPatternVl > 0.01f
                || state.FlySetbackCount > 0
                || state.SpiderSetbackCount > 0;
        }

        private void ResetAllViolations(MoveState state)
        {
            if (state == null) return;

            state.FlyVl = 0f;
            state.FlyCheatDetect = 0f;
            state.ManipulatorCheatDetect = 0f;
            state.SilentAimCheatDetect = 0f;
            state.FlyFireCheatDetect = 0f;
            state.NoFallDamageCheatDetect = 0f;
            state.PilotFireCheatDetect = 0f;
            state.HeadshotCheatDetect = 0f;
            state.SpiderCheatDetect = 0f;
            state.FlySetbackCount = 0;
            state.SpiderSetbackCount = 0;
            state.SpiderVl = 0f;
            state.SilentAimStrikesWindow = 0;
            state.SilentAimWindowStart = 0f;
            state.LastSilentAimTime = 0f;
            state.HeadshotWindowStart = 0f;
            state.HeadshotQualifiedHitsWindow = 0;
            state.HeadshotHeadHitsWindow = 0;
            state.HeadshotStreak = 0;
            state.HeadPatternOnlyHeadHits = 0;
            state.HeadPatternVl = 0f;
            state.LastHeadPatternHeadTime = 0f;
            state.LastViolationTime = 0f;
            if (state.DetectKindTimes != null) state.DetectKindTimes.Clear();
            if (state.RecentEvidence != null) state.RecentEvidence.Clear();
        }

        private BasePlayer FindOnlinePlayer(string input, out int matches)
        {
            matches = 0;
            if (string.IsNullOrEmpty(input)) return null;

            ulong steamId;
            if (ulong.TryParse(input, out steamId))
            {
                var byId = BasePlayer.FindByID(steamId);
                if (IsPlayerConnected(byId))
                {
                    matches = 1;
                    return byId;
                }

                return null;
            }

            var query = input.ToLowerInvariant();
            BasePlayer found = null;
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var candidate = BasePlayer.activePlayerList[i];
                if (!IsPlayerConnected(candidate)) continue;

                var name = candidate.displayName == null ? string.Empty : candidate.displayName.ToLowerInvariant();
                if (name != query && !name.Contains(query)) continue;

                matches++;
                if (found == null) found = candidate;
                if (matches > 1) return null;
            }

            return found;
        }

        private void NotifyAdmins(string message)
        {
            var plain = StripRichText(message);
            Puts(plain);
            var chatLine = BuildAdminChatMessage(plain);
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var target = BasePlayer.activePlayerList[i];
                if (!IsPlayerConnected(target)) continue;
                if (!permission.UserHasPermission(target.UserIDString, "ac.notify")) continue;
                target.ChatMessage(chatLine);
            }

            SendDiscord(plain);
        }

        private void NotifyAdmins(string message, ulong excludedUserId)
        {
            var plain = StripRichText(message);
            Puts(plain);
            var chatLine = BuildAdminChatMessage(plain);
            for (var i = 0; i < BasePlayer.activePlayerList.Count; i++)
            {
                var target = BasePlayer.activePlayerList[i];
                if (!IsPlayerConnected(target)) continue;
                if (target.userID == excludedUserId) continue;
                if (!permission.UserHasPermission(target.UserIDString, "ac.notify")) continue;
                target.ChatMessage(chatLine);
            }

            SendDiscord(plain);
        }

        private string BuildAdminChatMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;

            var line = message;

            line = line.Replace("[AC DETECT]", "<color=#f43f5e>[AC DETECT]</color>");
            line = line.Replace("[AC EVIDENCE]", "<color=#fb923c>[AC EVIDENCE]</color>");
            line = line.Replace("[AC DETECT_REPEAT]", "<color=#ef4444>[AC DETECT_REPEAT]</color>");
            line = line.Replace("[AC DETECT_FINAL]", "<color=#ef4444>[AC DETECT_FINAL]</color>");
            line = line.Replace("[AC DETECT_RULE]", "<color=#f59e0b>[AC DETECT_RULE]</color>");
            line = line.Replace("[AC DETECT_PATTERN]", "<color=#f97316>[AC DETECT_PATTERN]</color>");
            line = line.Replace("[AC DETECT_HS]", "<color=#f97316>[AC DETECT_HS]</color>");
            line = line.Replace("[AC DETECT_HEADSHOT]", "<color=#f97316>[AC DETECT_HEADSHOT]</color>");
            line = line.Replace("[AC DETECT_MANIP]", "<color=#f97316>[AC DETECT_MANIP]</color>");
            line = line.Replace("[AC DETECT_SILENTAIM]", "<color=#f97316>[AC DETECT_SILENTAIM]</color>");
            line = line.Replace("[AC DETECT_LOOK]", "<color=#f97316>[AC DETECT_LOOK]</color>");
            line = line.Replace("[AC DETECT_FLY]", "<color=#fb7185>[AC DETECT_FLY]</color>");
            line = line.Replace("[AC DETECT_SPIDER]", "<color=#fb7185>[AC DETECT_SPIDER]</color>");
            line = line.Replace("[AC DETECT_NOFALL]", "<color=#fb7185>[AC DETECT_NOFALL]</color>");
            line = line.Replace("[AC]", "<color=#22d3ee>[AC]</color>");

            line = line.Replace("action=BAN", "action=<color=#ef4444>BAN</color>");
            line = line.Replace("action=KICK", "action=<color=#f59e0b>KICK</color>");
            line = line.Replace("action=LOG_ONLY_NET", "action=<color=#a78bfa>LOG_ONLY_NET</color>");
            line = line.Replace("action=LOG_ONLY_HOLD", "action=<color=#a78bfa>LOG_ONLY_HOLD</color>");
            line = line.Replace("action=VL", "action=<color=#38bdf8>VL</color>");

            line = Regex.Replace(line, "\\b\\d{17}\\b", "<color=#fca5a5>$0</color>");
            line = Regex.Replace(line, "\\| detect=([^|]+)", "| detect=<color=#fbbf24>$1</color>");
            line = Regex.Replace(line, "\\| reason=([^|]+)", "| reason=<color=#fde68a>$1</color>");
            line = Regex.Replace(line, "\\| vl=([^|]+)", "| vl=<color=#93c5fd>$1</color>");
            line = Regex.Replace(line, "\\| weapon=([^|]+)", "| weapon=<color=#86efac>$1</color>");
            line = Regex.Replace(line, "\\| ping=([^|]+)", "| ping=<color=#c4b5fd>$1</color>");
            line = Regex.Replace(line, "\\| jitter=([^|]+)", "| jitter=<color=#c4b5fd>$1</color>");
            line = Regex.Replace(line, "\\| loss=([^|]+)", "| loss=<color=#c4b5fd>$1</color>");

            line = Regex.Replace(line, "\\]\\s([^\\(\\|]+)\\s\\(<color=#fca5a5>(\\d{17})</color>\\)", "] <color=#7dd3fc>$1</color> (<color=#fca5a5>$2</color>)");
            return line;
        }

        private string StripRichText(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            return Regex.Replace(message, "<.*?>", string.Empty);
        }

        private void SendDiscord(string message)
        {
            if (_config == null || string.IsNullOrEmpty(_config.DiscordWebhookUrl)) return;
            if (string.IsNullOrEmpty(message)) return;

            var quote = ((char)34).ToString();
            var slash = ((char)92).ToString();
            var safe = message.Replace(slash, slash + slash);
            safe = safe.Replace(quote, slash + quote);
            if (safe.Length > 1800) safe = safe.Substring(0, 1800);

            var payload = "{" + quote + "content" + quote + ":" + quote + safe + quote + "}";
            var headers = new Dictionary<string, string>();
            headers["Content-Type"] = "application/json";

            webrequest.Enqueue(_config.DiscordWebhookUrl, payload, (code, response) => { }, this, Core.Libraries.RequestMethod.POST, headers, 10f);
        }

        private void CheckSteamAccount(BasePlayer player)
        {
            if (!IsPlayerConnected(player)) return;
            var rule = GetRule("Account");
            if (rule == null || !rule.Enabled) return;
            if (_config == null || string.IsNullOrEmpty(_config.SteamApiKey)) return;

            var url = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + _config.SteamApiKey + "&steamids=" + player.UserIDString;
            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (!IsPlayerConnected(player)) return;
                if (code != 200 || string.IsNullOrEmpty(response)) return;

                var visibility = ExtractJsonInt(response, "communityvisibilitystate");
                var timeCreated = ExtractJsonLong(response, "timecreated");

                if (_config.RequirePublicProfile && visibility < 3)
                {
                    NotifyAdmins("[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> Account(PrivateProfile)");
                    HandlePenalty(player, "Account", 1f, "AccountPrivate");
                    return;
                }

                if (timeCreated <= 0) return;

                var created = UnixToDateTime(timeCreated);
                var ageDays = (DateTime.UtcNow - created).TotalDays;
                if (ageDays < _config.MinAccountAgeDays)
                {
                    NotifyAdmins("[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> Account(New " + ageDays.ToString("0.0") + "d)");
                    var minDays = _config.MinAccountAgeDays;
                    var kickMessage = "AC: account too new. Minimum " + minDays + " days.";
                    player.Kick(kickMessage);
                }
            }, this);
        }

        private void CheckProxyBlock(BasePlayer player)
        {
            if (!IsPlayerConnected(player)) return;
            var rule = GetRule("ProxyBlock");
            if (rule == null || !rule.Enabled) return;
            if (_config == null || !_config.ProxyCheckEnabled) return;

            var ip = GetPlayerIp(player);
            if (string.IsNullOrEmpty(ip)) return;

            var url = "http://proxycheck.io/v2/" + ip + "?vpn=1&asn=1&risk=1";
            if (!string.IsNullOrEmpty(_config.ProxyCheckApiKey))
            {
                url += "&key=" + _config.ProxyCheckApiKey;
            }

            webrequest.Enqueue(url, null, (code, response) =>
            {
                if (!IsPlayerConnected(player)) return;
                if (code != 200 || string.IsNullOrEmpty(response)) return;

                var proxy = ExtractJsonString(response, "proxy");
                if (!string.Equals(proxy, "yes", StringComparison.OrdinalIgnoreCase)) return;

                var risk = ExtractJsonInt(response, "risk");
                if (_config != null && _config.ProxyRiskMin > 0 && risk > 0 && risk < _config.ProxyRiskMin) return;

                var proxyType = ExtractJsonString(response, "type");
                if (string.IsNullOrEmpty(proxyType)) proxyType = "unknown";

                var msg = "[AC DETECT] " + player.displayName + " (" + player.UserIDString + ") -> ProxyBlock(type=" + proxyType + ", risk=" + risk + ", ip=" + ip + ")";
                NotifyAdmins(msg);
                HandlePenalty(player, "ProxyBlock", 1f, "ProxyBlock");
            }, this);
        }

        private string GetPlayerIp(BasePlayer player)
        {
            if (player == null || player.net == null || player.net.connection == null) return string.Empty;

            var ip = player.net.connection.ipaddress;
            if (string.IsNullOrEmpty(ip)) return string.Empty;

            if (ip.Contains("."))
            {
                var sep = ip.LastIndexOf(':');
                if (sep > 0) ip = ip.Substring(0, sep);
            }

            return ip;
        }

        private DateTime UnixToDateTime(long unix)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unix);
        }

        private int ExtractJsonInt(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return 0;
            var number = ExtractJsonNumber(json, key);
            int value;
            return int.TryParse(number, out value) ? value : 0;
        }

        private long ExtractJsonLong(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return 0L;
            var number = ExtractJsonNumber(json, key);
            long value;
            return long.TryParse(number, out value) ? value : 0L;
        }

        private string ExtractJsonNumber(string json, string key)
        {
            var keyToken = ((char)34).ToString() + key + ((char)34) + ":";
            var idx = json.IndexOf(keyToken, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return string.Empty;

            idx += keyToken.Length;
            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;

            var start = idx;
            while (idx < json.Length && char.IsDigit(json[idx])) idx++;
            if (start >= idx) return string.Empty;

            return json.Substring(start, idx - start);
        }

        private string ExtractJsonString(string json, string key)
        {
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return string.Empty;

            var quoteChar = (char)34;
            var slashChar = (char)92;
            var keyToken = quoteChar + key + quoteChar + ":";
            var idx = json.IndexOf(keyToken, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return string.Empty;

            idx += keyToken.Length;
            while (idx < json.Length && char.IsWhiteSpace(json[idx])) idx++;
            if (idx >= json.Length || json[idx] != quoteChar) return string.Empty;

            idx++;
            var start = idx;
            var escaped = false;
            while (idx < json.Length)
            {
                var ch = json[idx];
                if (ch == quoteChar && !escaped)
                {
                    break;
                }

                if (ch == slashChar && !escaped)
                {
                    escaped = true;
                }
                else
                {
                    escaped = false;
                }

                idx++;
            }

            if (idx <= start || idx > json.Length) return string.Empty;
            return json.Substring(start, idx - start);
        }
    }
}