using UnityEngine;
using Modding;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;


namespace SkillUpgrades.Util
{
    public static class ArrowStuff
    {
        public static  GameObject LoadArrow()
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

        public static void rotateAngle(FsmFloat angle, float maxAngle, float arrowAngularSpeed, GameObject arrowObject, FsmFloat OlderAngle)
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
