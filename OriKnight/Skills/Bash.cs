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
using SkillUpgrades.Util;
using HutongGames.PlayMaker;
using System.Runtime.Serialization.Json;

namespace OriKnight.Skills
{
    class Bash: MonoBehaviour
    {
        #region Defining variables
        internal static float angle = 0;
        internal static GameObject arrowObject;
        internal static GameObject arrowPrefab = LoadArrow();
        internal static float arrowAngularSpeed = 3;

        internal static float bashCD =0.7f;

        internal static bool canBash = true;
        internal static bool isBashing = false;
        internal static bool isFrozen = false;
        internal static bool wasOnGround;

        internal static GameObject bashedEnemy;
        internal static DamageEnemies damageConfig;
        internal static BoxCollider2D body;

        internal static float bashSpeed = 28;
        internal static float bashDuration=0.26f;
        internal static float bashTimeout = 2;
        internal static float timeCounter =0;
        internal static int prevLayer;

        internal static int bashDamage = 131;
        internal static ActivateGameObject flash;
        internal static Vector2 enemySpeed;

        //internal static bool waitForBash = false;


        public static KeyCode bashButton;// = (KeyCode)System.Enum.Parse(typeof(KeyCode), "V");
        
        #endregion


        public static void Hook()
        {
            #region Create the hooks
            On.HeroController.Start += AddBash; //Adds the MonoB to the players gameObject
            On.HeroController.TakeDamage += MakeBash; //Bash condition
            On.EnemyBullet.OnTriggerEnter2D += DontDestroyProjectilesT; //Stops projectiles from destroying when they hit the knight
            //On.EnemyBullet.OnCollisionEnter2D += DeleteDamage;
            On.DamageEnemies.DoDamage += DeleteDamage;


            #endregion
        }
        

        private void Awake()
        {
            arrowPrefab.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default")); //Makes the arrow Pretty
            flash= HeroController.instance.superDash.GetState("Ground Charged").GetActionsOfType<ActivateGameObject>()[1];
            body = HeroController.instance.GetComponent<BoxCollider2D>();
        
        }
        private static void DeleteDamage(On.DamageEnemies.orig_DoDamage orig, DamageEnemies self, GameObject target)
        {
            orig(self, target);
            if (self.damageDealt == bashDamage)
            {
                {
                    Destroy(self.gameObject.GetComponent<DamageEnemies>());
                    bashedEnemy.layer = prevLayer;


                }
                //if(self.gameObject == bashedEnemy) 


            }
        }

        private static void DontDestroyProjectilesT(On.EnemyBullet.orig_OnTriggerEnter2D orig, EnemyBullet self, Collider2D collision)
        {
            if (((canBash && Input.GetKey(bashButton)) || isBashing)&& collision.attachedRigidbody.name == "Knight") 
            {
                return; 
            }

            else {
                orig(self, collision); }
        }

        private static void AddBash(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            HeroController.instance.gameObject.AddComponent<Bash>();
        }

        internal static void MakeBash(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
        {
            

            #region Handles entering bash
            if (Input.GetKey(bashButton) && hazardType==1 && canBash && !isFrozen && (go.layer ==22 || go.layer == 19 || go.layer ==12 || go.layer ==17 || go.layer == 11) )// hazardType == 1
            {
                #region Sets Variables
                bashedEnemy = go;
                wasOnGround = HeroController.instance.CheckTouchingGround();
               

                //Modding.Logger.Log( + "  " + go.transform.localEulerAngles);

                try
                {
                    enemySpeed = go.GetComponent<Rigidbody2D>().velocity;
                    
                }
                catch{ }

                canBash = false;
                isFrozen = true;
                
                #endregion
                
                
                arrowObject = Instantiate(arrowPrefab);

                arrowObject.transform.position = new Vector3 (HeroController.instance.transform.position.x,
                HeroController.instance.transform.position.y-HeroController.instance.GetComponent<BoxCollider2D>().size.y/ 2,
                HeroController.instance.transform.position.z);

                #region ajusts the initial angle based on the current inputs

                //up right
                if (GameManager.instance.inputHandler.inputActions.right.IsPressed
                    && GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.down.IsPressed)
                {
                    angle = 45;
                }

                //up
                else if (!GameManager.instance.inputHandler.inputActions.right.IsPressed
                    && GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.left.IsPressed)
                {
                    angle = 90;
                }

                //upleft
                else if (!GameManager.instance.inputHandler.inputActions.right.IsPressed
                    && GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && GameManager.instance.inputHandler.inputActions.left.IsPressed)
                {
                    angle = 135;
                }

                //left
                else if (GameManager.instance.inputHandler.inputActions.left.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.down.IsPressed)
                {
                    angle = 180;
                    
                }

                //left down
                else if (GameManager.instance.inputHandler.inputActions.left.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && GameManager.instance.inputHandler.inputActions.down.IsPressed)
                {
                    angle = 225;

                }

                //down

                else if (!GameManager.instance.inputHandler.inputActions.left.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.right.IsPressed
                    && GameManager.instance.inputHandler.inputActions.down.IsPressed)
                {
                    angle = 270;

                }

                //down right
                else if (GameManager.instance.inputHandler.inputActions.right.IsPressed
                    && !GameManager.instance.inputHandler.inputActions.up.IsPressed
                    && GameManager.instance.inputHandler.inputActions.down.IsPressed)
                {
                    angle = 315;

                }

                //right
                else {angle = 0;}
                #endregion



                arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), angle);
                arrowObject.transform.localScale *= 1.5f; //* (HeroController.instance.cState.facingRight ? 1 : -1);              
                arrowObject.SetActive(true);
                #endregion

                GameManager.instance.StartCoroutine(HandleFreeze(damageSide));
                Time.timeScale = 0.001f;
            }


            else if (isFrozen) { }
            else { orig(self, go, damageSide, damageAmount, hazardType); }

        }

        private static IEnumerator HandleFreeze(CollisionSide damageSide)
        {

           // Modding.Logger.Log(bashedEnemy.layer);
            HeroController.instance.hero_state = ActorStates.no_input;
            
            //Loop that handles the arrow
            while (Input.GetKey(bashButton) && timeCounter < bashTimeout )
            { 
                RotateAngle();
                yield return new WaitForSecondsRealtime(Time.deltaTime);
                timeCounter += Time.deltaTime;
            }
            #region resets variables
            isFrozen = false;
            HeroController.instance.QuakeInvuln();
            GameManager.instance.StartCoroutine(BashCD());
            
            
            flash.gameObject.GameObject.Value.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0,0,1),angle+90);
            flash.OnEnter();
            timeCounter = 0;
            Destroy(arrowObject);
            Time.timeScale = 1f;
            #endregion
            //HeroController.instance.FlipSprite();

            #region sets the bashed enemy damage if it is an attack and rotate it to point



            prevLayer = bashedEnemy.gameObject.layer;
            bashedEnemy.gameObject.layer = ((int)GlobalEnums.PhysLayers.HERO_ATTACK);

            if (prevLayer != ((int)PhysLayers.ENEMIES)){

                bashedEnemy.transform.position = HeroController.instance.transform.position;
                bashedEnemy.transform.eulerAngles = new Vector3(0, 0, 0);
                bashedEnemy.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), angle+180);
                //bashedEnemy.transform.
            }
            
            damageConfig = bashedEnemy.gameObject.transform.gameObject.AddComponent<DamageEnemies>();
            damageConfig.damageDealt = bashDamage;
            damageConfig.ignoreInvuln = false;

            //bashedEnemy.tag = "bashed";

            #endregion

            isBashing = true; //sets Bashing state
            
        }

        private static IEnumerator BashCD()
        {
            yield return new WaitForSeconds(bashCD);
            canBash = true;
        }

        private void  FixedUpdate() 
        {

            #region Handles Hero's and EnemyBashed momentum
            if (isBashing && (timeCounter < bashDuration) && !HeroController.instance.cState.transitioning)
            {
                //Modding.Logger.Log(bashedEnemy.GetComponent<Recoil>());

                timeCounter += Time.fixedDeltaTime;
                try
                {
                    bashedEnemy.GetComponent<Rigidbody2D>().velocity = new Vector2((float)(-bashSpeed * Math.Cos(angle * Math.PI / 180)),
                                                                                            (float)(-bashSpeed * Math.Sin(angle * Math.PI / 180)));
                 //   (float)(-Math.Abs(enemySpeed.x) * Math.Cos(angle * Math.PI / 180)),
                   //                                                                         (float)(-Math.Abs(enemySpeed.y) * Math.Sin(angle * Math.PI / 180)));
                }
                finally
                {
                    Rigidbody2D _body = HeroController.instance.GetComponent<Rigidbody2D>();

                    _body.velocity = new Vector2((float)(bashSpeed * Math.Cos(angle * Math.PI / 180)),
                                                                                            (float)(bashSpeed * Math.Sin(angle * Math.PI / 180)));

                
                }

              

            }
            #endregion

            #region post bashed handler
            else if (((timeCounter > bashDuration) || HeroController.instance.cState.onGround) && isBashing) 
            { 
                flash.gameObject.GameObject.Value.transform.RotateAround(HeroController.instance.transform.position, new Vector3(0, 0, 1), -angle-90);;
                timeCounter = 0;isBashing = false; angle = 0;
                HeroController.instance.ResetAirMoves();
                HeroController.instance.hero_state = HeroController.instance.cState.onGround? ActorStates.idle:ActorStates.airborne;
                #region resets enemys bash damage unless its a projectile
                if (bashedEnemy)
                {
                    if (prevLayer != ((int)PhysLayers.ENEMIES)) { return; }
                    else { bashedEnemy.gameObject.layer = prevLayer; }
                }
                #endregion
            }
            else { }
            #endregion
        }

        private void Update()
        {
            /*if(!isBashing && Input.GetKey(bashButton) && !waitForBash)
            {
                body.size *=2f;
                waitForBash = true;
            }
            else if (waitForBash && !Input.GetKey(bashButton))
            {
                waitForBash = false;
                body.size /= 2f;
            }*/
        }

        private static GameObject LoadArrow()
        {
            string bundleN = "arrow2";
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
            return ab.LoadAsset<GameObject>("bashGameArrowG");
        }

        private static void RotateAngle()
        {
            angle *= angle == +-360 ? 0 : 1; // makes so that the angle stays in raneg

            #region handles diagonals
            //up left
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed
                && !GameManager.instance.inputHandler.inputActions.down.IsPressed
                && !GameManager.instance.inputHandler.inputActions.right.IsPressed
                && GameManager.instance.inputHandler.inputActions.left.IsPressed
                && (angle == 135 || angle == -225)) { }

            // down left
            else if (!GameManager.instance.inputHandler.inputActions.up.IsPressed
                && GameManager.instance.inputHandler.inputActions.down.IsPressed
                && !GameManager.instance.inputHandler.inputActions.right.IsPressed
                && GameManager.instance.inputHandler.inputActions.left.IsPressed
                && (angle == -135 || angle == 225)) { }

            //down right

            else if (!GameManager.instance.inputHandler.inputActions.up.IsPressed
                && GameManager.instance.inputHandler.inputActions.down.IsPressed
                && GameManager.instance.inputHandler.inputActions.right.IsPressed
                && !GameManager.instance.inputHandler.inputActions.left.IsPressed
                && (angle == 315 || angle == -45)) { }

            //up right

            else if (GameManager.instance.inputHandler.inputActions.up.IsPressed
                && !GameManager.instance.inputHandler.inputActions.down.IsPressed
                && GameManager.instance.inputHandler.inputActions.right.IsPressed
                && !GameManager.instance.inputHandler.inputActions.left.IsPressed
                && (angle == 45 || angle == -315)) { }

            #endregion

            #region handles cardinals
            //up
            else if (GameManager.instance.inputHandler.inputActions.up.IsPressed && !GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                if (angle == 90) { }

                else if (Math.Cos(angle * Math.PI / 180) > 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Cos(angle * Math.PI / 180) < 0)
                {
                    angle -= arrowAngularSpeed;
                    angle = (Math.Cos(angle * Math.PI / 180) > 0) ? 90 : angle;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }

                else { }
            }


            //down
            else if (!GameManager.instance.inputHandler.inputActions.up.IsPressed && GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                if (angle == 270 || angle == -90) { }

                else if (Math.Cos(angle * Math.PI / 180) < 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Cos(angle * Math.PI / 180) > 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

            //right
            else if (GameManager.instance.inputHandler.inputActions.right.IsPressed && !GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                if (angle == 0 || angle==360) { }

                else if (Math.Sin(angle * Math.PI / 180) < 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Sin(angle * Math.PI / 180) > 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

            //left
            else if (!GameManager.instance.inputHandler.inputActions.right.IsPressed && GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                if (angle == 180 || angle==-180) { }

                else if (Math.Sin(angle * Math.PI / 180) > 0)
                {
                    angle += arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), arrowAngularSpeed);
                }

                else if (Math.Sin(angle * Math.PI / 180) < 0)
                {
                    angle -= arrowAngularSpeed;
                    arrowObject.transform.RotateAround(arrowObject.transform.position, new Vector3(0, 0, 1), -arrowAngularSpeed);
                }
                else { }
            }

            else { }
            #endregion
        }

    }
}
