using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace SkillUpgrades.Util
{
    internal class DecideToStopSuperdash : ComponentAction<Rigidbody2D>
    {
        private readonly FsmFloat _hSpeed;
        private readonly FsmFloat _vSpeed;
        private readonly FsmBool _zeroLast;
        private readonly FsmFloat _angle;

        public DecideToStopSuperdash(FsmFloat hSpeed, FsmFloat vSpeed, FsmFloat angle, FsmBool zeroLast)
        {
            _hSpeed = hSpeed;
            _vSpeed = vSpeed;
            _angle = angle;
            _zeroLast = zeroLast;
        }


        public override void OnEnter()
        {
            try
            {
                UpdateCache(Fsm.FsmComponent.gameObject);
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopSuperdash (OnEnter/UpdateCache):\n" + e);
            }

            try
            {
                DecideToStop();
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopSuperdash (OnEnter):\n" + e);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                DecideToStop();
            }
            catch (Exception e)
            {
                LogError("Error in DecideToStopSuperdash (OnUpdate):\n" + e);
            }
        }

        private void DecideToStop()
        {
            Vector2 vector = rigidbody2d.velocity;

            if (Math.Abs(_hSpeed.Value) >= 0.2f && Math.Abs(vector.x) < 0.1f) _zeroLast.Value = true;
            if (Math.Abs(_vSpeed.Value) >= 0.2f && Math.Abs(vector.y) < 0.1f) _zeroLast.Value = true;
            _angle.Value = 0;
        }
    }
}