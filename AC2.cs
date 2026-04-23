
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
