using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;
using System.IO;
using System.Reflection;


namespace OriKnight.Skills
{
    public class Grenade: MonoBehaviour
    {

        internal static AssetBundle ab = null;



        public GameObject grenade = LoadGrenade();
        internal static GameObject arrow = LoadArrow();

        public bool canGrenade = true;
        internal static bool isExploding;
        private float explosionTime => 2;
        private float grenadeCD => 2;

        private DamageEnemies de;
        private CircleCollider2D collider;
        private Rigidbody2D body;

        public static void Hook()
        {
            On.HeroController.Start += NovoStart;
            //On.GameManager.EquipCharm += 
        }

        void Awake()
        {
            #region Set enviromental variables and stuff
            de = grenade.gameObject.transform.gameObject.AddComponent<DamageEnemies>();
            collider = gameObject.GetComponent<CircleCollider2D>();
            body = gameObject.GetComponent<Rigidbody2D>();

            #endregion
            
            #region Add Components to the gameObject: shaders damage stuff etc...
            grenade.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
            de.damageDealt = 70;
            de.ignoreInvuln = false;
        
            grenade.gameObject.layer = ((int)GlobalEnums.PhysLayers.HERO_ATTACK);

            #endregion
        }





        private void Update()
        {
            if (Input.GetKey(KeyCode.R) && canGrenade)
            {
                GameManager.instance.StartCoroutine(MakeGrenade());
               
            }
        }

        private IEnumerator MakeGrenade()
        {

            canGrenade = false;

            grenade.gameObject.transform.position = new Vector2(HeroController.instance.transform.position.x + (1*(HeroController.instance.cState.facingRight ? 1 : -1))
                                                                , HeroController.instance.transform.position.y+2);

            
            GameObject grenadeObject = Instantiate(grenade);
            grenadeObject.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(2, 2);
            GameManager.instance.StartCoroutine(GrenadeTimeOut(grenadeObject));


           /* arrowObject = Instantiate(arrowPrefab);

            arrowObject.transform.position = new Vector3(HeroController.instance.transform.position.x,
            HeroController.instance.transform.position.y - HeroController.instance.GetComponent<BoxCollider2D>().size.y / 2,
            HeroController.instance.transform.position.z);*/


            yield return new WaitForSeconds(grenadeCD);
            
            canGrenade = true;
        }

        private static void NovoStart(On.HeroController.orig_Start orig, HeroController self)
        {
            orig(self);
            HeroController.instance.gameObject.AddComponent<Grenade>();

        }

        private void  ExplodeGrenade(GameObject grenadeToExplode)
        {
            //grenadeToExplode
        }

        private IEnumerator GrenadeTimeOut(GameObject grenadeToExplode)
        {
            yield return new WaitForSeconds(explosionTime);
            ExplodeGrenade(grenadeToExplode);
        }

        public static GameObject LoadGrenade()
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


        private static GameObject LoadArrow()
        {
            string bundleN = "arrow2";
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

    }
}
