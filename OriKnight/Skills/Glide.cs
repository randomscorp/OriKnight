using Modding;
using UnityEngine;


namespace OriKnight.Skills
{
    internal static class Glide 
    {
        public static  bool isOnWind = false;
        private static bool isGliding = false;
        private static bool canGlide = true;

        public static KeyCode glideButton;// = (KeyCode)System.Enum.Parse(typeof(KeyCode), "LeftControl");

        public static void Hook()
        {

            On.HeroController.Move += MakeGlide;

        }

        internal static void MakeGlide(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            

            Rigidbody2D body = HeroController.instance.GetComponent<Rigidbody2D>();
            if (Input.GetKey(glideButton) && canGlide && !HeroController.instance.cState.onGround && !HeroController.instance.cState.swimming)
            {
                isGliding = true;

                HeroController.instance.ResetHardLandingTimer();
                if (isOnWind)
                {
                    HeroController.instance.ResetAirMoves();
                    Launch.canLaunch = true;
                    body.velocity = new Vector2(move_direction * (self.RUN_SPEED - 2.5f), 10);
                    return;
                }
                else { body.velocity = new Vector2(move_direction * (self.RUN_SPEED-2.5f), -5); return; }
            }
            else
            {
                isGliding = false;
                orig(self, move_direction);
            };


        }
    }
}
