using System;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using GlobalEnums;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker.Actions;

namespace OriKnight.Skills
{
    internal class Launch: MonoBehaviour
    {
        internal static float angle = 0;
        internal static float arrowAngularSpeed = 5;
        public static float launchSpeed = 25;
        internal static float launchDuration = 2;
        internal static float timeInLaunch = 0f;

        internal static bool finishedLaunching=true;
        internal static float momemuntumDuration = 0.28f;

        public static bool canLaunch = true;
        public static bool isLauching = false;

        private HeroAnimationController animator;

        internal static GameObject arrow = LoadArrow();
        internal static GameObject arrowObject = null;

        private void Start()
        {
            //animator = HeroController.instance.GetComponent<HeroAnimationController>();            
            //animator.animator.enabled = true;
        }

        internal static void Hook()
        {
            On.HeroController.Start += AddLaunch;
            On.HeroController.Bounce += ResetLaunchPogo;
        }
        internal static void AddLaunch(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            HeroController.instance.gameObject.AddComponent<Launch>();

        }

        private void Update()
        {

            //If this not exists you can fall oob by mashing laucnh
            if (HeroController.instance.cState.transitioning) { return; }

            //Handles getting out of launch
            if ((!Input.GetKey(KeyCode.V) || timeInLaunch >= launchDuration) && isLauching && !finishedLaunching)
            {
                Modding.Logger.Log("if 1");
                #region resets the variables andd destroys the arrow
                isLauching = false;
                timeInLaunch = 0;
                Destroy(arrowObject);
                //HeroController.instance.shadowdashBurstPrefab.SetActive(true);
                
                //angle = 0;
                #endregion

            }

            if (!finishedLaunching && !isLauching && timeInLaunch > momemuntumDuration)
            {
                Modding.Logger.Log("if 3");

                finishedLaunching = true;
                angle = 0;
                timeInLaunch = 0;
                HeroController.instance.AffectedByGravity(true);
                HeroController.instance.hero_state = HeroController.instance.CheckTouchingGround() ? ActorStates.grounded : ActorStates.airborne;
            }

            // Handles da launch input
            if (Input.GetKey(KeyCode.V) && canLaunch && !isLauching)
            {
                Modding.Logger.Log("if 5");


                #region set variables
                HeroController.instance.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                isLauching = true;
                canLaunch = false;
                finishedLaunching = false;
                HeroController.instance.AffectedByGravity(false);
                HeroController.instance.hero_state = ActorStates.no_input;
                #endregion

                arrowObject = Instantiate(arrow);

                #region makes the arrow
                if (HeroController.instance.cState.facingRight)
                {
                    arrowObject.transform.position = new Vector3(HeroController.instance.transform.position.x + 3,
                                                                    HeroController.instance.transform.position.y,
                                                                    HeroController.instance.transform.position.z);
                    angle = 0;
                };
                if (!HeroController.instance.cState.facingRight)

                {
                    angle = 180;
                    arrowObject.transform.Rotate(new Vector3(0, 0, -1));
                    arrowObject.transform.position = new Vector3(HeroController.instance.transform.position.x - 3,
                                                                    HeroController.instance.transform.position.y,
                                                                    HeroController.instance.transform.position.z);
                };
                
                arrowObject.transform.localScale *= 0.1f * (HeroController.instance.cState.facingRight ? 1 : -1);
                arrowObject.SetActive(true);
                #endregion


            }


            //Resets your launch
            if (!isLauching && finishedLaunching && !canLaunch && (HeroController.instance.CheckTouchingGround() || HeroController.instance.cState.wallSliding))
            {
                canLaunch = true;
            }

        }

        private void FixedUpdate()
        {

            
            

            //Makes the momentum
            if (!finishedLaunching && !isLauching && (timeInLaunch < momemuntumDuration))
            {
                

                HeroController.instance.GetComponent<Rigidbody2D>().velocity = new Vector2((float)(launchSpeed*Math.Cos(angle*Math.PI/180)),
                                                                                            (float)(launchSpeed * Math.Sin(angle * Math.PI / 180)));
                //animator.Play("Dash");
                //HeroController.instance.GetComponent<HeroAnimationController>().PlayClip("Dash");
                
                HeroController.instance.GetComponent<tk2dSpriteAnimator>().Play("Shadow Dash Sharp");
                //tk2dSpriteAnimator.AddComponent(HeroController.instance.gameObject, )


                timeInLaunch += Time.fixedDeltaTime;
            }

           

            //Handles angle and arrow
            if (isLauching && timeInLaunch < launchDuration)
            {
                Modding.Logger.Log("if 4");

                RotateAngle();
                timeInLaunch += Time.fixedDeltaTime;
            }

    

                   
        }

       

        internal static void ResetLaunchPogo(On.HeroController.orig_Bounce orig, HeroController self)
        {
            orig(self);
            canLaunch = true;
            
            //HeroController.instance.GetComponent<tk2dSpriteAnimator>().
        }

        private static GameObject LoadArrow()
        {
            string bundleN = "arrow";
            AssetBundle ab = null; // You probably want HeroController.instance to be defined somewhere more global.
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                    if (s == null) continue;
                    string bundleName = Path.GetExtension(res).Substring(1);
                    if (bundleName != bundleN) continue;
                    // Allows us to directly load from stream.
                    ab = AssetBundle.LoadFromStream(s); // Store HeroController.instance somewhere you can access again.
                };
            }
            return ab.LoadAsset<GameObject>("arrow");
        }

        private static void RotateAngle()
        {

            if (GameManager.instance.inputHandler.inputActions.up.IsPressed && !GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                if (angle == 90) { }

                else if (Math.Cos(angle * Math.PI / 180) > 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Cos(angle * Math.PI / 180) < 0)
                {
                    angle -= arrowAngularSpeed;
                    angle = (Math.Cos(angle * Math.PI / 180) > 0) ? 90 : angle;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }

                else { }
            }

            else if (!GameManager.instance.inputHandler.inputActions.up.IsPressed && GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                if (angle == 270 || angle==-90) { }

                else if (Math.Cos(angle * Math.PI / 180) < 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Cos(angle * Math.PI / 180) > 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

            else if (GameManager.instance.inputHandler.inputActions.right.IsPressed && !GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                if (angle == 0) { }

                else if (Math.Sin(angle * Math.PI / 180) < 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Sin(angle * Math.PI / 180) > 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

            else if (!GameManager.instance.inputHandler.inputActions.right.IsPressed && GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                if (Math.Abs(angle) == 180 ) { }

                else if (Math.Sin(angle * Math.PI / 180) > 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Sin(angle * Math.PI / 180) < 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

        }

        
    }
}
