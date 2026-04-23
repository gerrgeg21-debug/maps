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