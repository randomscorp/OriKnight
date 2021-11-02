using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Modding;
using System.IO;
using System.Reflection;
using System.Collections;
using GlobalEnums;


namespace OriKnight.Skills
{
    class Shuriken: MonoBehaviour
    {
        #region Variables definition

            #region Components
        private static CircleCollider2D collider;
        
        private static DamageEnemies damageEnemies;
        
        private static Rigidbody2D rigidbody;
        
        internal static AssetBundle ab;

        private static GameObject shurikenPrefab = LoadShuriken();
        private static GameObject shurikenInstance=null;
        private Utils.ShurikenBehaviour behaviour;

        #endregion

        #region Stats and constants
        public KeyCode shurikenKey = KeyCode.V;
        public int damage=30;
        public static float shurikenCD = 0.3f;

        #endregion

        #region Control variables
     
        public static bool canShuriken = true;

        #endregion

        #endregion

        #region Hooks and functions
        internal static void Hook()
        {
            On.HeroController.Start += HeroController_Start;
        }

        private static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            self.gameObject.AddComponent<Shuriken>();
        }
        #endregion

        #region Flag control functions

        private bool ShurikenCondition() { return shurikenInstance? false:true; }

        #endregion


        #region MonoB functions
        private void Awake()
        {


            //Add Components to the prefab, yes I am too lazy to do it in the editor 
            behaviour = shurikenPrefab.AddComponent<Utils.ShurikenBehaviour>();
            shurikenPrefab.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));

            damageEnemies =shurikenPrefab.transform.gameObject.AddComponent<DamageEnemies>();
            rigidbody = shurikenPrefab.AddComponent<Rigidbody2D>();
            collider = shurikenPrefab.AddComponent<CircleCollider2D>();


            shurikenPrefab.name = "oi";
            ConfigDamage();
            ConfigRigidBody();
            ConfigCollider();
        }
        private void Start()
        {
            
        }

        private void Update()
        {
            if(Input.GetKey(shurikenKey) && ShurikenCondition() && canShuriken)
            {
                shurikenPrefab.transform.position = HeroController.instance.transform.position + new Vector3(0.5f, 0, 0)*(HeroController.instance.cState.facingRight? 1:-1);
                behaviour.direction = InputVector();
                shurikenInstance = Instantiate(shurikenPrefab);
            }
         
        }
        #endregion


        private Vector2 InputVector()
        {
            Vector2 direction = new Vector2();
            
            if (GameManager.instance.inputHandler.inputActions.up.IsPressed)
            {
                direction.y = 1;
            }
            else if (GameManager.instance.inputHandler.inputActions.down.IsPressed)
            {
                direction.y = -1;
            }
            else {direction.y = 0; }

            if (GameManager.instance.inputHandler.inputActions.right.IsPressed)
            {
                direction.x = 1;
            }
            else if (GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                direction.x = -1;
            }

            if (direction == new Vector2(0, 0)) { direction = new Vector2(HeroController.instance.cState.facingRight ? 1 : -1, 0); }

            return direction;
        }

        public static void ShurikenCD() { Modding.Logger.Log("oisadsad"); }

        #region Components config functions
        private void ConfigCollider()
        {
           //collider.tag = "Nail Attack";
        }

        private void ConfigRigidBody()
        {

           
        }

        private void ConfigDamage()
        {

            damageEnemies.gameObject.layer = ((int)PhysLayers.HERO_ATTACK);
            damageEnemies.attackType=AttackTypes.Spell;
            damageEnemies.damageDealt = damage;
            damageEnemies.ignoreInvuln = false;
            

        }



        #endregion

        #region AssetBundle functions
        public static GameObject LoadShuriken()
        {
            string bundleN = "grenade";
            // You probably want this to be defined somewhere more global.
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
            return ab.LoadAsset<GameObject>("grenade");

        }


        #endregion 



    }
}
