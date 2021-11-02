using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using SkillUpgrades.Util;
using UnityEngine.SceneManagement;
using System.IO;
using System.Reflection;

namespace OriKnight.Skills
{
    internal static class ChargedJump
    {
        private static bool verticalSuperdashEnabled => true;
        private static bool diagonalSuperdashEnabled => true;
        internal enum SuperdashDirection
        {
            Normal = 0,     // Anything not caused by this mod
            Upward,
            Diagonal
        }

        private static SuperdashDirection _queuedSuperdashState = SuperdashDirection.Normal;
        private static SuperdashDirection _superdashState = SuperdashDirection.Normal;
        internal static SuperdashDirection SuperdashState
        {
            get => _superdashState;

            set
            {
                float angle = HeroController.instance.superDash.FsmVariables.GetFsmFloat("Older Angle").Value;
                if (angle < 0)
                {
                    angle = 0;
                }

                if (value == SuperdashDirection.Upward && _superdashState == SuperdashDirection.Normal)
                {
                    HeroController.instance.transform.Rotate(0, 0, -90 * HeroController.instance.transform.localScale.x);
                }
                else if (value == SuperdashDirection.Normal && _superdashState == SuperdashDirection.Upward)
                {
                    // We need to set the SD Burst inactive before un-rotating the hero,
                    // so it doesn't rotate with it
                    if (GameObject.Find("SD Burst") is GameObject burst)
                    {
                        burst.transform.parent = HeroController.instance.gameObject.transform;
                        burst.SetActive(false);
                    }
                    HeroController.instance.transform.Rotate(0, 0, 90 * HeroController.instance.transform.localScale.x);
                }
                else if (value == SuperdashDirection.Diagonal && _superdashState == SuperdashDirection.Normal)
                {

                    HeroController.instance.transform.Rotate(0, 0, -angle * HeroController.instance.transform.localScale.x);


                }
                else if (value == SuperdashDirection.Normal && _superdashState == SuperdashDirection.Diagonal)
                {
                    // We need to set the SD Burst inactive before un-rotating the hero,
                    // so it doesn't rotate with it
                    if (GameObject.Find("SD Burst") is GameObject burst)
                    {
                        burst.transform.parent = HeroController.instance.gameObject.transform;
                        burst.SetActive(false);
                    }
                    HeroController.instance.transform.Rotate(0, 0, angle * HeroController.instance.transform.localScale.x);
                }

                _superdashState = value;
                _queuedSuperdashState = SuperdashDirection.Normal;
            }
        }


        internal static void Hook()
        {
            On.CameraTarget.Update += FixVerticalCamera;
            On.GameManager.FinishedEnteringScene += DisableUpwardOneways;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ResetSuperdashState;
            On.HeroController.Start += ModifySuperdashFsm;
        }

        private static void FixVerticalCamera(On.CameraTarget.orig_Update orig, CameraTarget self)
        {
            orig(self);

            if (self.hero_ctrl != null && GameManager.instance.IsGameplayScene())
            {
                if (!self.superDashing) return;

                if (SuperdashState == SuperdashDirection.Upward)     // if vertical cdash
                {
                    self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset);
                    self.dashOffset = 0;
                }
                else if (SuperdashState == SuperdashDirection.Diagonal)
                {
                    self.cameraCtrl.lookOffset += Math.Abs(self.dashOffset) * ((float)Math.Sqrt(2) / 2f);
                    self.dashOffset *= (float)Math.Sqrt(2) / 2f;
                }
            }
        }
        // Deactivate upward oneway transitions after spawning in so the player doesn't accidentally
        // softlock by vc-ing into them
        private static void DisableUpwardOneways(On.GameManager.orig_FinishedEnteringScene orig, GameManager self)
        {
            orig(self);

            switch (self.sceneName)
            {
                // The KP top transition is the only one that needs to be disabled; the others have collision
                case "Tutorial_01":
                    if (GameObject.Find("top1") is GameObject topTransition)
                        topTransition.SetActive(false);
                    break;
            }
        }
        private static void ResetSuperdashState(Scene arg0, Scene arg1)
        {
            SuperdashState = SuperdashDirection.Normal;
        }

        private static void ModifySuperdashFsm(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);

            PlayMakerFSM fsm = self.superDash;
            PlayMakerFSM spell = self.spellControl;


            #region Add FSM Variables
            FsmFloat vSpeed = fsm.AddFsmFloat("V Speed VC");
            FsmFloat hSpeed = fsm.AddFsmFloat("H Speed VC");
            FsmFloat speed = fsm.AddFsmFloat("Speed");
            FsmFloat angle = fsm.AddFsmFloat("Angle");
            FsmFloat OlderAngle = fsm.AddFsmFloat("Older Angle");
            GameObject arrow = LoadArrow();
            GameObject arrowObject = null;

            angle.Value = 0;
            #endregion

            #region Makes Charged Jump Wizzardry



            //Local variables  
            float arrowAngularSpeed = 1.5f;
            float maxAngle = 70;

            //Instatiate the arrow 
            fsm.GetState("Wall Charge").AddFirstAction(new ExecuteLambda(() =>
            {
                arrowObject = GameManager.Instantiate(arrow);
                if (!HeroController.instance.cState.facingRight) //It returns truen when its not facing right....... Why TC, WHY???
                {
                    arrowObject.transform.position = new Vector3(HeroController.instance.transform.position.x + 3,
                                                                    HeroController.instance.transform.position.y,
                                                                    HeroController.instance.transform.position.z);
                };
                if (HeroController.instance.cState.facingRight)

                {
                    arrowObject.transform.Rotate(new Vector3(0, 0, -1));
                    arrowObject.transform.position = new Vector3(HeroController.instance.transform.position.x - 3,
                                                                    HeroController.instance.transform.position.y,
                                                                    HeroController.instance.transform.position.z);
                };

                arrowObject.transform.localScale *= 0.1f * (HeroController.instance.cState.facingRight ? -1 : 1);
                arrowObject.SetActive(true);

            }));


            //Rotate the angle and the arrow 
            fsm.GetState("Wall Charged").AddFirstAction(new ExecuteLambdaEveryFrame(() =>
            {
                rotateAngle(angle, maxAngle, arrowAngularSpeed, arrowObject, OlderAngle);

            }));

            fsm.GetState("Wall Charge").AddFirstAction(new ExecuteLambdaEveryFrame(() =>
            {
                rotateAngle(angle, maxAngle, arrowAngularSpeed, arrowObject, OlderAngle);
            }));

            fsm.GetState("Charge Cancel Wall").AddAction(new ExecuteLambda(() =>
            {
                GameManager.Destroy(arrowObject);
                angle = 0;
            }));
            #endregion


            #region Set Direction
            fsm.GetState("Direction").AddFirstAction(new ExecuteLambda(() =>
            {
                bool shouldDiagonal = false;
                bool shouldVertical = false;

                if (diagonalSuperdashEnabled)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed || GameManager.instance.inputHandler.inputActions.down.IsPressed)
                    {
                        if (GameManager.instance.inputHandler.inputActions.right.IsPressed && HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                        else if (GameManager.instance.inputHandler.inputActions.left.IsPressed && !HeroController.instance.cState.facingRight)
                        {
                            shouldDiagonal = true;
                        }
                    }
                }
                if (verticalSuperdashEnabled && !shouldDiagonal)
                {
                    if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
                    {
                        shouldVertical = true;
                    }
                }

                if (shouldDiagonal)
                {
                    _queuedSuperdashState = SuperdashDirection.Diagonal;
                }
                else if (shouldVertical)
                {
                    _queuedSuperdashState = SuperdashDirection.Upward;
                }
            }));

            fsm.GetState("Direction Wall").AddFirstAction(new ExecuteLambda(() =>
            {

                _queuedSuperdashState = SuperdashDirection.Diagonal;
            }));

            fsm.GetState("Left").AddAction(new ExecuteLambda(() =>
            {
                SuperdashState = _queuedSuperdashState;
            }));
            fsm.GetState("Right").AddAction(new ExecuteLambda(() =>
            {
                SuperdashState = _queuedSuperdashState;
            }));
            #endregion

            #region Modify Dashing and Cancelable states
            FsmState dashing = fsm.GetState("Dashing");
            ExecuteLambda setVelocityVariables = new ExecuteLambda(() =>

            {
                GameManager.Destroy(arrowObject);
                float velComponent = Math.Abs(fsm.FsmVariables.GetFsmFloat("Current SD Speed").Value);
                switch (SuperdashState)
                {
                    case SuperdashDirection.Diagonal:

                        vSpeed.Value = (float)(velComponent * Math.Sin((angle.Value * Math.PI) / 180));
                        hSpeed.Value = (float)(velComponent * Math.Cos((angle.Value * Math.PI) / 180)) * (HeroController.instance.cState.facingRight ? 1 : -1);
                        HeroController.instance.SetCState("spellQuake", true);

                        break;
                    case SuperdashDirection.Upward:
                        vSpeed.Value = velComponent;
                        hSpeed.Value = 0f;
                        HeroController.instance.SetCState("spellQuake", true);



                        break;
                    default:
                    case SuperdashDirection.Normal:
                        vSpeed.Value = 0f;
                        hSpeed.Value = velComponent * (HeroController.instance.cState.facingRight ? 1 : -1);
                        break;
                }
            });

            SetVelocity2d setVel = dashing.GetActionOfType<SetVelocity2d>();
            setVel.x = hSpeed;
            setVel.y = vSpeed;


            DecideToStopSuperdash decideToStop = new DecideToStopSuperdash(hSpeed, vSpeed, angle, fsm.FsmVariables.GetFsmBool("Zero Last Frame"));

            //sets up dive state            
            dashing.Actions = new FsmStateAction[]
            {
                    setVelocityVariables,
                    spell.GetState("Quake1 Down").Actions[7], //alow dive to break walls
                    dashing.Actions[0],
                    dashing.Actions[1],
                    setVel,
                    dashing.Actions[3],
                    dashing.Actions[4],
                    decideToStop,
                    dashing.Actions[7],
                    dashing.Actions[8],

            };

            FsmState cancelable = fsm.GetState("Cancelable");
            cancelable.Actions = new FsmStateAction[]
            {
                    cancelable.Actions[0],
                    setVel,
                    cancelable.Actions[2],
                    decideToStop,
                    cancelable.Actions[5],
                    cancelable.Actions[6],
                    //spell.GetState("Quake1 Land").Actions[14],
            };
            #endregion

            #region Reset Vertical Charge variable
            fsm.GetState("Air Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
                HeroController.instance.SetCState("spellQuake", false);
            }));



            //****************************************************************************************//
            fsm.GetState("Cancel").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
                HeroController.instance.SetCState("spellQuake", false);
            }));
            fsm.GetState("Hit Wall").AddFirstAction(new ExecuteLambda(() =>
            {
                SuperdashState = SuperdashDirection.Normal;
                HeroController.instance.SetCState("spellQuake", false);
            }));

            //Doing this the dumb way cause I'm too tired to thing 
            fsm.GetState("Air Cancel").AddFirstAction(spell.GetState("Quake1 Land").Actions[14]);
            fsm.GetState("Cancel").AddFirstAction(spell.GetState("Quake1 Land").Actions[14]);
            fsm.GetState("Hit Wall").AddFirstAction(spell.GetState("Quake1 Land").Actions[14]);

            #endregion
        }

        private static GameObject LoadArrow()
        {
            string bundleN = "arrow";
            AssetBundle ab = null; // You probably want this to be defined somewhere more global.
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    string bundleName = Path.GetExtension(res).Substring(1);
                    if (bundleName != bundleN) continue;
                    // Allows us to directly load from stream.
                    ab = AssetBundle.LoadFromStream(s); // Store this somewhere you can access again.
                };
            }
            return ab.LoadAsset<GameObject>("arrow");
        }

        private static void rotateAngle(FsmFloat angle, float maxAngle, float arrowAngularSpeed, GameObject arrowObject, FsmFloat OlderAngle)
        {
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed && !GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                //arrowAngularSpeed *= (HeroController.instance.cState.facingRight ? -1 : 1);
                if (angle.Value < maxAngle)
                {

                    angle.Value += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed * (HeroController.instance.cState.facingRight ? -1 : 1));
                }

            }

            else if (!GameManager.instance.inputHandler.inputActions.up.IsPressed && GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                //arrowAngularSpeed *= (HeroController.instance.cState.facingRight ? -1 : 1);
                if (angle.Value > -maxAngle)
                {
                    angle.Value -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed * (HeroController.instance.cState.facingRight ? -1 : 1));
                }
            }
            OlderAngle.Value = angle.Value;
        }
    }
}