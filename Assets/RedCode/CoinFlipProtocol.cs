using UnityEngine;

namespace RedCard {

    public class CoinFlipProtocol : MonoBehaviour {


        public enum ProcessStage {
            NotYetTime,
            ShowingSides,
            ReadyToToss,
            DeclaringResult,
            WaitingForWinnerDecision,
            WaitingForLoserDecision,
            SwitchSidesMaybe,
            HandingBallToKickOffCaptain,
            Done
        }

        public enum WinnerDecision {
            SwitchSides,
            Kickoff,
        }

        public enum LoserDecision {
            SwitchSides,
            NoSwitch,
        }

        [Header("ASSIGNATIONS")]
        public LayerMask captainEyeMask;
        public CaptainBody homeCaptain;
        public CaptainBody awayCaptain;
        public RefControls arbitro;

        [Header("SETTINGS")]
        public float eyeRange = 5f;
        public float eyePeripheralVision = 33f;
        public float captainTurnSpeed = 45f;

        [Header("PROCESSS VARS")]
        public Coin coin;
        public bool shownTailsHome = false;
        public bool shownTailsAway = false;
        public bool shownHeadsHome = false;
        public bool shownHeadsAway = false;
        public bool calledHeads = false;
        public bool saidHeads = false;
        public bool saidTails = false;
        public bool pocketedCoinPrematurely = false;
        public bool headsWon = false;
        public bool declaredResult = false;
        public bool winnerHasBeenAsked = false;
        public CaptainBody winningCaptain;
        public CaptainBody losingCaptain;
        public CaptainBody kickingCaptain;
        public ProcessStage stage;
        public WinnerDecision winnerDecision; 
        public LoserDecision loserDecision;


        public float t;
        public float s;

        Transform objectToFocus;


        private void Awake() {
            if (!arbitro || !homeCaptain || !awayCaptain) {
                enabled = false;
                Debug.LogWarning("can'at do coin flip missing parts");
            }

            //#DEBUG #TEMP
            Debug.Assert(homeCaptain != awayCaptain);
            homeCaptain.teamID = 0;
            awayCaptain.teamID = 1;

            arbitro.hud.wheel.PopulateBoxes(arbitro.hud.wheel.nothingToSay);
        }

        public void Update() {


            switch (stage) {

                case ProcessStage.NotYetTime:
                    t = 0f;
                    s = 0f;
                    if (arbitro.coin) {
                        stage = ProcessStage.ShowingSides;
                        RedMatch.OnRefSpoke += CheckIfRefSpokeToCaptains;
                        arbitro.hud.wheel.PopulateBoxes(arbitro.hud.wheel.coinFlipExplanation);
                        print("ProcessStage: " + stage);
                    }
                    break;

                case ProcessStage.ShowingSides:

                    if (!arbitro.coin) return;
                    coin = arbitro.coin;

                    objectToFocus = coin.transform;
                    if (arbitro.equipped != RefEquipment.Coin) objectToFocus = arbitro.cam.transform;
                    awayCaptain.FocusTransform(this, objectToFocus);
                    homeCaptain.FocusTransform(this, objectToFocus);

                    bool homeCapSees = false;
                    bool awayCapSees = false;
                    Vector3 eyesToCoin = arbitro.coin.transform.position - homeCaptain.eyes.position;
                    Ray captainsView = new Ray(homeCaptain.eyes.position, eyesToCoin.normalized);

                    RaycastHit hitInfo;
                    if (Physics.Raycast(captainsView, out  hitInfo, eyeRange, captainEyeMask)) {

                        eyesToCoin.y = 0f;
                        bool withinViewingAngle = Vector3.Angle(homeCaptain.eyes.forward, eyesToCoin) < eyePeripheralVision;
                        if (withinViewingAngle &&hitInfo.transform == arbitro.coin.transform) {
                            homeCapSees = true;
                        }
                    }

                    eyesToCoin = arbitro.coin.transform.position - awayCaptain.eyes.position;
                    captainsView = new Ray(awayCaptain.eyes.position, eyesToCoin.normalized);

                    if (Physics.Raycast(captainsView, out hitInfo, eyeRange, captainEyeMask)) {
                        eyesToCoin.y = 0f;
                        float angle = Vector3.Angle(awayCaptain.eyes.forward, eyesToCoin);
                        bool withinViewingAngle = angle < eyePeripheralVision;
                        if (withinViewingAngle && hitInfo.transform == arbitro.coin.transform) {
                            awayCapSees = true;
                        }
                        //else print($"awayCaptain can't see, angle {angle}, hit transform: {hitInfo.transform}");
                    }
                    //else print("not reaching" );

                    homeCaptain.eyes.transform.localScale = Vector3.one * (homeCapSees ? 2f : 1f);
                    awayCaptain.eyes.transform.localScale = Vector3.one * (awayCapSees ? 2f : 1f);

                    if (saidHeads) {
                        if (arbitro.coin.state != Coin.State.HeadsUp) {
                            // uh oh
                        }
                        else {
                            if (homeCapSees) shownHeadsHome = true;
                            if (awayCapSees) shownHeadsAway = true;
                        }
                    }

                    if (saidTails) {
                        if (arbitro.coin.state != Coin.State.TailsUp) {
                            // uh oh
                        }
                        else {
                            if (homeCapSees) shownTailsHome = true;
                            if (awayCapSees) shownTailsAway = true;
                        }
                    }

                    if (shownHeadsHome && shownHeadsAway && shownTailsHome && shownTailsAway) {
                        print("ProcessStage: ReadyToToss");
                        homeCaptain.eyes.transform.localScale = Vector3.one;
                        awayCaptain.eyes.transform.localScale = Vector3.one;
                        homeCaptain.SetEyeColor(Color.red);
                        awayCaptain.SetEyeColor(Color.red);
                        stage = ProcessStage.ReadyToToss;
                        arbitro.hud.wheel.PopulateBoxes(arbitro.hud.wheel.nothingToSay);
                    }
                    else if (arbitro.equipped != RefEquipment.Coin) {

                        s += Time.deltaTime;
                        if (s > 1f) {
                            // puzzlement, lose respect
                            s -= 1f;
                        }

                    }
                    break;
                case ProcessStage.ReadyToToss:
                    objectToFocus = coin.transform;
                    if (arbitro.equipped != RefEquipment.Coin) objectToFocus = arbitro.cam.transform;
                    awayCaptain.FocusTransform(this, objectToFocus);
                    homeCaptain.FocusTransform(this, objectToFocus);

                    if (coin.state == Coin.State.Flipping) {
                        pocketedCoinPrematurely = false;
                        stage = ProcessStage.DeclaringResult;
                        print("ProcessStage: DeclaringResult");
                        calledHeads = Random.value > .5f;
                        arbitro.hud.wheel.PopulateBoxes(arbitro.hud.wheel.coinFlipResults);
                    }
                    break;

                case ProcessStage.DeclaringResult:
                    objectToFocus = coin.transform;
                    if (arbitro.coin && arbitro.equipped != RefEquipment.Coin) objectToFocus = arbitro.cam.transform;
                    awayCaptain.FocusTransform(this, objectToFocus);
                    homeCaptain.FocusTransform(this, objectToFocus);

                    if (arbitro.coin && !pocketedCoinPrematurely) {
                        pocketedCoinPrematurely = true;
                        arbitro.hud.ShowBadCall();
                    }

                    if (coin.state == Coin.State.HeadsUp) {
                        if (calledHeads) {
                            winningCaptain = awayCaptain;
                            losingCaptain = homeCaptain;
                        }
                        else {
                            winningCaptain = homeCaptain;
                            losingCaptain = awayCaptain;
                        }
                    }
                    else if (coin.state == Coin.State.TailsUp) {
                        if (saidTails) {
                            if (calledHeads) {
                                winningCaptain = awayCaptain;
                                losingCaptain = homeCaptain;
                            }
                            else {
                                winningCaptain = homeCaptain;
                                losingCaptain = awayCaptain;
                            }
                        }
                    }

                    if (declaredResult) {
                        stage = ProcessStage.WaitingForWinnerDecision;
                        print("StageProcess " + stage);
                        arbitro.hud.wheel.PopulateBoxes(arbitro.hud.wheel.coinFlipWinnerQuestion);
                    }
                    break;

                case ProcessStage.WaitingForWinnerDecision:
                    winningCaptain.FocusTransform(this, arbitro.cam.transform);
                    losingCaptain.FocusTransform(this, winningCaptain.eyes.transform);

                    bool winnerHasMadeDecision = Random.value > .01f;
                    if (winnerHasBeenAsked && winnerHasMadeDecision) {
                        if (Random.value > .5f) {
                            winningCaptain.SetEyeColor(Color.green);
                            winnerDecision = WinnerDecision.Kickoff;
                            kickingCaptain = winningCaptain;
                            print("winner says kickoff");
                        }
                        else {
                            winningCaptain.SetEyeColor(Color.blue);
                            winnerDecision = WinnerDecision.SwitchSides;
                            if (homeCaptain == winningCaptain) kickingCaptain = awayCaptain;
                            else kickingCaptain = homeCaptain;
                            print("winner says switch sides");
                        }
                        print("StageProcess: WaitingForLoserDecision");
                        stage = ProcessStage.WaitingForLoserDecision;
                    }
                    break;

                case ProcessStage.WaitingForLoserDecision:
                    winningCaptain.FocusTransform(this, losingCaptain.transform);
                    losingCaptain.FocusTransform(this, arbitro.cam.transform);

                    if (winnerDecision == WinnerDecision.SwitchSides) {
                        print("loser has no decision to make");
                        print("StageProcess: SwitchSidesMaybe");
                        stage = ProcessStage.SwitchSidesMaybe;
                    }
                    else {
                        bool loserHasMadeDecision = Random.value > .01f;
                        if (loserHasMadeDecision) {
                            loserDecision = Random.value > .5f ? LoserDecision.SwitchSides : LoserDecision.NoSwitch;
                            if (loserDecision == LoserDecision.SwitchSides) {
                                print("loser says switch");
                                loserDecision = LoserDecision.SwitchSides;
                                if (homeCaptain == winningCaptain) awayCaptain.SetEyeColor(Color.blue);
                                else homeCaptain.SetEyeColor(Color.blue);
                            }
                            else {
                                print("loser says no switch");
                                if (homeCaptain == winningCaptain) homeCaptain.SetEyeColor(Color.green);
                                else awayCaptain.SetEyeColor(Color.green);
                            }

                            print("StageProcess: SwitchSidesMaybe");
                            stage = ProcessStage.SwitchSidesMaybe;
                        }
                        else print("waiting on loser)");
                    }
                    break;

                case ProcessStage.SwitchSidesMaybe:
                    homeCaptain.FocusTransform(this, arbitro.cam.transform);
                    awayCaptain.FocusTransform(this, arbitro.cam.transform);

                    if (winnerDecision == WinnerDecision.SwitchSides || loserDecision == LoserDecision.SwitchSides) {
                        // wait for gesture ?
                    }

                    print("StageProcess: HandingBallToKickOffCaptain");
                    stage = ProcessStage.HandingBallToKickOffCaptain;
                    break;

                case ProcessStage.HandingBallToKickOffCaptain:
                    homeCaptain.FocusTransform(this, arbitro.cam.transform);
                    awayCaptain.FocusTransform(this, arbitro.cam.transform);

                    if (kickingCaptain.hasBall) {
                        kickingCaptain.SetEyeColor(Color.yellow);
                        print("StageProcess: Done !!!");
                        RedMatch.OnRefSpoke -= CheckIfRefSpokeToCaptains;
                        stage = ProcessStage.Done;
                    }
                    break;

                case ProcessStage.Done:
                    break;
            }

        }

        public void CheckIfRefSpokeToCaptains(Semantics semantics, RefTarget target) {

            switch (stage) {

                case ProcessStage.ShowingSides:
                    if (semantics == Semantics.ThisIsHeads) {
                        if (arbitro.coin && arbitro.coin.state == Coin.State.HeadsUp) {
                            saidHeads = true;
                            saidTails = false;
                        }
                        else {
                            // #TODO lost respect
                            RedMatch.Match.hud.ShowBadCall();
                        }
                    }
                    else if (semantics == Semantics.ThisIsTails) {
                        if (arbitro.coin && arbitro.coin.state == Coin.State.TailsUp) {
                            saidHeads = false;
                            saidTails = true;
                        }
                        else {
                            // #TODO lost respect
                            RedMatch.Match.hud.ShowBadCall();
                        }
                    }
                    else Debug.LogWarning("what semantics while showing side? " + semantics);
                    break;


                case ProcessStage.DeclaringResult:
                    if (semantics == Semantics.CoinLandedHeads) {
                        if (coin && coin.state == Coin.State.HeadsUp) {
                            declaredResult = true;
                            print("declared result");
                        }
                        else {
                            // #TODO lost respect
                            RedMatch.Match.hud.ShowBadCall();
                        }
                    }
                    else if (semantics == Semantics.CoinLandedTails) {
                        if (coin && coin.state == Coin.State.TailsUp) {
                            declaredResult = true;
                            print("declared result");
                        }
                        else {
                            // #TODO lost respect
                            RedMatch.Match.hud.ShowBadCall();
                        }
                    }
                    else Debug.LogWarning("what semantics while showing side? " + semantics);
                    break;

                case ProcessStage.WaitingForWinnerDecision:
                    if (semantics == Semantics.CoinFlipWinnerQuestion) {

                        // check if looking correct way!
                        // players should really be following ref with their eyes tbh

                        Vector3 refEyesToCaptainEyes = winningCaptain.eyes.position - arbitro.cam.transform.position;
                        Vector3 dir = refEyesToCaptainEyes.normalized;
                        Vector3 origin = arbitro.cam.transform.position + dir * .25f;
                        if (Physics.SphereCast(origin, .1f, dir, out RaycastHit hitInfo, eyeRange)) {
                            if (hitInfo.collider.transform == winningCaptain.eyes) {
                                winnerHasBeenAsked = true;
                            }
                            else Debug.Log("asked wrong winner " + hitInfo.collider);
                        }

                        if (!winnerHasBeenAsked) RedMatch.Match.hud.ShowBadCall();
                    }
                    break;

            }

        }
    }
}
