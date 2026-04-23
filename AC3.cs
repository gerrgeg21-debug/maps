
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